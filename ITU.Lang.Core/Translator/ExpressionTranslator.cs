using System.Text;
using System.Linq;

using Antlr4.Runtime.Misc;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using ITU.Lang.Core.Types;
using static ITU.Lang.Core.Grammar.LangParser;

namespace ITU.Lang.Core.Translator
{
    public partial class Translator
    {
        public override Node VisitExpr([NotNull] ExprContext context)
        {
            if (context.@operator() != null)
            {
                return HandleOperatorExpr(context);
            }

            var leftParen = context.LeftParen()?.GetText() ?? "";
            var rightParen = context.RightParen()?.GetText() ?? "";

            var buf = new StringBuilder();
            Type lastSeenType = null;
            var hasVisitedFirstChild = false;

            foreach (var child in context.children)
            {
                if (child == null) continue;

                var res = Visit(child);

                if (res == null) continue;

                if (!hasVisitedFirstChild)
                {
                    hasVisitedFirstChild = true;
                    lastSeenType = res.Type;
                }
                else if (!lastSeenType.Equals(res.Type))
                {
                    var msg = $"Type mismatch: Expected expression '{res.TranslatedValue}' of type '{res.Type.AsNativeName()}' to be of type '{lastSeenType?.AsNativeName()}'";
                    throw new TranspilationException(msg, GetTokenLocation(child));
                }

                buf.Append(res.TranslatedValue);
            }

            if (lastSeenType == null)
            {
                throw new TranspilationException("Could not derive type of expression", GetTokenLocation(context));
            }

            return new Node()
            {
                TranslatedValue = leftParen + buf.ToString() + rightParen,
                Type = lastSeenType,
                Location = GetTokenLocation(context),
            };
        }

        private Node HandleOperatorExpr(ExprContext context)
        {
            var op = context.@operator().GetText();

            var exprs = context.expr().Select(Visit).ToArray();

            var children = context.children;

            var node = getNode();

            node.Location = GetTokenLocation(context);

            return node;

            Node getNode()
            {
                return new Node()
                {
                    TranslatedValue = "",
                };
                if (children[0] is OperatorContext) // unary pre
                    return operators.GetUnaryPrefix(op, exprs[0]);
                else if (exprs.Length == 1) // unary post
                    return operators.GetUnaryPostfix(op, exprs[0]);
                else // binary
                    return operators.GetBinary(op, exprs[0], exprs[1]);
            };
        }

        public override Node VisitAccess([NotNull] AccessContext context)
        {
            var name = context.GetText();
            var res = scopes.GetBinding(name);
            if (!scopes.HasBinding(name))
            {
                throw new TranspilationException("Variable '" + name + "' was not declared before accessing!");
            }

            return res;
        }

        public override Node VisitBool([NotNull] BoolContext context)
        {
            return new Node()
            {
                TranslatedValue = (context.False() ?? context.True()).GetText(),
                Type = new BooleanType(),
                Location = GetTokenLocation(context),
            };
        }

        public override Node VisitInteger([NotNull] IntegerContext context)
        {
            return new Node()
            {
                TranslatedValue = context.Int().GetText(),
                Type = new IntType(),
                Location = GetTokenLocation(context),
            };
        }

        public override Node VisitStringLiteral([NotNull] StringLiteralContext context)
        {
            var content = context.StringLiteral().GetText();

            return new Node()
            {
                TranslatedValue = content,
                Type = new StringType(),
                Location = GetTokenLocation(context),
            };
        }

        public override Node VisitCharLiteral([NotNull] CharLiteralContext context)
        {
            var content = context.CharLiteral().GetText();

            return new Node()
            {
                TranslatedValue = content,
                Type = new CharType(),
                Location = GetTokenLocation(context),
            };
        }

        public override Node VisitFunction([NotNull] FunctionContext context)
        {
            var args = context.functionArguments();

            var paramNames = args.Name().Select(p => p.GetText());
            var paramTypes = args.typeAnnotation().Select(x =>
            {
                var name = x.Name().GetText();
                if (scopes.HasBinding(name))
                {
                    return scopes.GetBinding(name);
                }

                if (name == "void")
                {
                    throw new TranspilationException("Cannot use void as a parameter type", GetTokenLocation(x));
                }
                throw new TranspilationException("Type '" + name + "' was not declared before used in function argument!", GetTokenLocation(context));
            }).ToList();

            var body = Visit(((IParseTree)context.expr()) ?? context.block());

            var returnTypeName = context?.typeAnnotation()?.Name()?.GetText();

            var returnType = body.Type;

            if (returnTypeName != null)
            {
                returnType = returnTypeName == "void" ? new VoidType() : scopes.GetBinding(returnTypeName).Type;
            }

            if (body.Type != null)
            {
                body.AssertType(returnType);
            }

            var functionType = new FunctionType()
            {
                ReturnType = returnType,
                ParameterTypes = paramTypes.Select(p => p.Type).ToList(),
            };

            return new Node()
            {
                TranslatedValue = $"({string.Join(",", paramNames)}) => {body.TranslatedValue}",
                Location = GetTokenLocation(context),
                Type = functionType,
            };
        }

        public override Node VisitInvokeFunction([NotNull] InvokeFunctionContext context)
        {
            var name = context.Name().GetText();

            var function = scopes.GetBinding(name);

            if (!(function?.Type is FunctionType))
            {
                throw new TranspilationException($"Cannot call non-invokable '{name}'", GetTokenLocation(context));
            }

            var exprs = context.expr().Select(expr => VisitExpr(expr));

            var exprTypes = exprs.Select(expr => expr.Type).ToList();
            var funcType = (FunctionType)function.Type;
            var paramTypes = funcType.ParameterTypes;

            if (!exprTypes.Equals(paramTypes))
            {
                var exprCount = exprTypes.Count;
                var paramCount = paramTypes.Count;

                if (exprCount != paramCount)
                {
                    throw new TranspilationException($"Function '{name}' takes {paramCount} parameters, got {exprCount}", GetTokenLocation(context));
                }

                for (int i = 0; i > exprCount; i++)
                {
                    var expr = exprTypes[i];
                    var param = paramTypes[i];
                    if (!expr.Equals(param))
                    {
                        throw new TranspilationException($"Function '{name}' could not be invoked: parameter {i} must be of type '{param}', but was '{expr}'", GetTokenLocation(context));
                    }
                }
            }

            var exprText = string.Join(",", exprs.Select(expr => expr.TranslatedValue));

            // Construct type of what the call would be, so that we can compare it to the variable in scope of Name
            return new Node()
            {
                TranslatedValue = $"{function.TranslatedValue}({exprText})",
                Type = funcType.ReturnType,
                Location = GetTokenLocation(context),
            };
        }

        public override Node VisitTerm([NotNull] TermContext context)
        {
            return Visit(context.GetRuleContext<ParserRuleContext>(0));
        }

        public override Node VisitLiteral([NotNull] LiteralContext context)
        {
            return Visit(context.GetRuleContext<ParserRuleContext>(0));
        }

        public override Node VisitReturnStatement([NotNull] ReturnStatementContext context)
        {
            var expr = Visit(context.expr());

            return new Node()
            {
                TranslatedValue = $"return {expr.TranslatedValue};",
                Location = GetTokenLocation(context),
                Type = expr.Type,
            };
        }
    }
}
