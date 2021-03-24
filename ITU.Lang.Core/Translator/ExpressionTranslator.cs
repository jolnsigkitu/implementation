using System.Text;
using System.Linq;

using Antlr4.Runtime.Misc;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using ITU.Lang.Core.Operators;
using ITU.Lang.Core.Types;
using static ITU.Lang.Core.Grammar.LangParser;
using System.Collections.Generic;

namespace ITU.Lang.Core.Translator
{
    public partial class Translator
    {
        OperatorFactory operators = Operators.Operators.InitializeOperators(new OperatorFactory());
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

            var exprs = context.expr().Select(VisitExpr).ToArray();

            var children = context.children;

            var node = getNode();

            node.Location = GetTokenLocation(context);

            return node;

            Node getNode()
            {
                if (children[0] is OperatorContext) // unary pre
                    return operators.UnaryPrefix.Get(op, exprs[0]);
                else if (exprs.Length == 1) // unary post
                    return operators.UnaryPostfix.Get(op, exprs[0]);
                else // binary
                    return operators.Binary.Get(op, exprs[0], exprs[1]);
            };
        }

        public override Node VisitAccess([NotNull] AccessContext context)
        {
            var name = context.GetText();
            var res = scopes.GetBinding(name);
            if (!scopes.HasBinding(name))
            {
                throw new TranspilationException("Variable '" + name + "' was not declared before accessing!", GetTokenLocation(context));
            }

            return new Node()
            {
                TranslatedValue = name,
                Type = res.Type,
                Location = GetTokenLocation(context),
            };
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
            using var _ = UseScope();

            var blockFun = context.blockFunction();
            var lambdaFun = context.lambdaFunction();

            return InnerVisitFunction(blockFun, lambdaFun);
        }

        private Node InnerVisitFunction(BlockFunctionContext blockFun, LambdaFunctionContext lambdaFun, bool isMember = false)
        {
            var functionParameterList = blockFun?.functionParameterList() ?? lambdaFun?.functionParameterList();

            if (functionParameterList == null)
            {
                return null;
            }

            var args = functionParameterList.functionArguments();

            var paramNames = args.Name().Select(p => p.GetText()).ToList();
            var paramTypes = args.typeAnnotation().Select(x => EvalTypeExpr(x.typeExpr())).ToList();

            foreach (var (name, type) in paramNames.Zip(paramTypes, System.ValueTuple.Create))
            {
                scopes.Bind(name, new Node()
                {
                    TranslatedValue = name,
                    Type = type,
                });
            }

            var body = Visit(((IParseTree)lambdaFun?.expr()) ?? blockFun.block());

            var returnType = body.Type;

            if (functionParameterList.Void() != null)
            {
                returnType = new VoidType();
            }
            else if (functionParameterList.typeAnnotation() != null)
            {
                returnType = EvalTypeExpr(functionParameterList.typeAnnotation().typeExpr());
            }

            if (body.Type != null)
            {
                body.AssertType(returnType);
            }

            var signature = isMember ? "" : $"({string.Join(",", paramNames)})";
            var seperator = !isMember || lambdaFun != null ? " =>" : "";

            return new Node()
            {

                TranslatedValue = $"{signature}{seperator} {body.TranslatedValue}",
                Location = GetTokenLocation(((ISyntaxTree)blockFun) ?? lambdaFun),
                Type = new FunctionType()
                {
                    ReturnType = returnType,
                    ParameterTypes = paramTypes,
                    ParameterNames = paramNames,
                    IsLambda = lambdaFun != null,
                },
            };
        }

        public override Node VisitInvokeFunction([NotNull] InvokeFunctionContext context)
        {
            var name = context.nestedName().GetText(); // TODO: Cannot work for `.`-names yet

            var function = scopes.GetBinding(name);

            if (!(function?.Type is FunctionType))
            {
                throw new TranspilationException($"Cannot call non-invokable '{name}'", GetTokenLocation(context));
            }

            var exprs = context.arguments().expr().Select(expr => VisitExpr(expr));

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

        public override Node VisitClassExpr([NotNull] ClassExprContext context)
        {
            using var _ = UseScope();
            var members = new Dictionary<string, (Type, Node)>();

            context.classMember()
                .ToList()
                .ForEach((ctx) =>
                {
                    var name = ctx.Name().GetText();
                    var val = VisitClassMember(ctx);
                    members.Add(name, (val.Type, val));
                    scopes.Bind(name, val);
                });

            var classType = new ClassType()
            {
                Members = members,
            };

            return new Node()
            {
                TranslatedValue = "",
                Location = GetTokenLocation(context),
                Type = classType,
            };
        }

        public override Node VisitClassMember([NotNull] ClassMemberContext context)
        {
            var fun = InnerVisitFunction(context.blockFunction(), context.lambdaFunction(), true);
            var expr = context.expr() != null ? VisitExpr(context.expr()) : null;
            var typ = context.typeAnnotation()?.typeExpr() != null
                ? EvalTypeExpr(context.typeAnnotation().typeExpr())
                : null;

            var val = fun ?? expr;

            if (typ != null)
            {
                val?.AssertType(typ);
            }

            return new Node()
            {
                TranslatedValue = val?.TranslatedValue ?? "",
                Location = GetTokenLocation(context),
                Type = typ ?? val.Type,
            };
        }

        public override Node VisitInstantiateObject([NotNull] InstantiateObjectContext context)
        {
            var name = context.nestedName().GetText(); // TODO: Cannot work for `.`-names yet

            var exprs = context.arguments()?.expr().Select(expr => VisitExpr(expr));

            // TODO: Actually perform typechecking

            var exprText = exprs != null ? string.Join(",", exprs.Select(expr => expr.TranslatedValue)) : "";

            // Construct type of what the call would be, so that we can compare it to the variable in scope of Name
            return new Node()
            {
                TranslatedValue = $"new {name}({exprText})",
                Type = new ObjectType() { Name = name },
                Location = GetTokenLocation(context),
            };
        }
    }
}
