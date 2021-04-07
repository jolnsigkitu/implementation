using System.Text;
using System.Linq;
using System.Collections.Generic;

using Antlr4.Runtime.Misc;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using ITU.Lang.Core.Operators;
using ITU.Lang.Core.Types;
using static ITU.Lang.Core.Grammar.LangParser;

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
                TranslatedValue = buf.ToString(),
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

        private R InvokeIf<T, R>(T value, System.Func<T, R> func) =>
            value != null ? func(value) : default(R);

        public override Node VisitAccess([NotNull] AccessContext context)
        {
            var node = InvokeIf(context.GetRuleContext<ParserRuleContext>(0), Visit);
            var typ = node?.Type;

            var name = context.Name()?.GetText();

            if (name != null)
            {
                if (!scopes.HasBinding(name))
                    throw new TranspilationException($"Variable '{name}' was not declared before accessing", GetTokenLocation(context));

                node = scopes.GetBinding(name);
                typ = node.Type;
            }

            var leftParen = context.LeftParen()?.GetText() ?? "";
            var rightParen = context.RightParen()?.GetText() ?? "";
            node.TranslatedValue = leftParen + node.TranslatedValue + rightParen;

            var chain = AccumulateAccessChain(context.accessChain());
            var chainParts = new List<string>();
            foreach (var link in chain)
            {
                if (!(typ is ObjectType n))
                    throw new TranspilationException($"Cannot access property '{name}' on non-object", GetTokenLocation(context));

                var memberName = link is InvokeFunctionContext l ? l.Name().GetText() : link.GetText();
                var member = n.GetMember(memberName);

                if (member == null)
                    throw new TranspilationException($"Cannot access member '{name}' on object '{n.AsNativeName()}'", GetTokenLocation(context));


                // TODO: Make type check on parameter types vs expr types, maybe just visit the stuff
                if (member is FunctionType f)
                {
                    var functionNode = VisitInvokeFunction((InvokeFunctionContext)link);
                    member = functionNode.Type;
                    chainParts.Add(functionNode.TranslatedValue);
                }
                else
                {
                    chainParts.Add(memberName);
                }

                typ = member;
            }

            return new Node()
            {
                TranslatedValue = string.Join(".", chainParts),
                Type = typ,
                Location = GetTokenLocation(context),
            };
        }

        public IList<IParseTree> AccumulateAccessChain(AccessChainContext context)
        {
            var list = new List<IParseTree>();

            for (var rest = context; rest != null; rest = rest.accessChain())
            {
                var name = rest.Name();
                var function = rest.invokeFunction();
                list.Add((IParseTree)name ?? function);
            }

            return list;
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
            var access = VisitAccess(context.access());
            var name = access.TranslatedValue;

            if (!(access.Type is FunctionType funcType))
            {
                throw new TranspilationException($"Cannot call non-invokable '{name}'", GetTokenLocation(context));
            }

            var exprs = context.arguments().expr().Select(expr => VisitExpr(expr));

            var exprTypes = exprs.Select(expr => expr.Type).ToList();
            var paramTypes = funcType.ParameterTypes;

            if (!exprTypes.Equals(paramTypes))
            {
                var exprCount = exprTypes.Count;
                var paramCount = paramTypes.Count;

                if (exprCount != paramCount)
                {
                    throw new TranspilationException($"Function '{name}' takes {paramCount} parameters, but got {exprCount}", GetTokenLocation(context));
                }

                for (int i = 0; i < exprCount; i++)
                {
                    var expr = exprTypes[i];
                    var param = paramTypes[i];

                    if (!expr.Equals(param))
                    {
                        throw new TranspilationException($"Function '{name}' could not be invoked: parameter {i + 1} must be of type '{param.AsNativeName()}', but was '{expr.AsNativeName()}'", GetTokenLocation(context));
                    }
                }
            }

            var exprText = string.Join(",", exprs.Select(expr => expr.TranslatedValue));

            // Construct type of what the call would be, so that we can compare it to the variable in scope of Name
            return new Node()
            {
                TranslatedValue = $"{name}({exprText})",
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
            var objMembers = new Dictionary<string, Type>();

            // When we construct the members, we should act as if 'this' is a object
            var node = new Node()
            {
                TranslatedValue = "this",
                Location = GetTokenLocation(context),
                Type = new ObjectType()
                {
                    Members = objMembers,
                },
                IsConst = true,
            };

            scopes.Bind("this", node);

            context.classMember()
                .ToList()
                .ForEach((ctx) =>
                {
                    var name = ctx.Name().GetText();
                    var val = VisitClassMember(ctx);

                    if (name == "constructor" && !(val.Type is FunctionType))
                        throw new TranspilationException("Member 'constructor' must be a method", GetTokenLocation(ctx));

                    members.Add(name, (val.Type, val));
                    objMembers.Add(name, val.Type);
                    scopes.Bind(name, val);
                });

            // After members have been constructed, actually make this node a class instead of an object
            node.TranslatedValue = "";
            node.Type = new ClassType()
            {
                Members = members,
            };

            return node;
        }

        public override Node VisitClassMember([NotNull] ClassMemberContext context)
        {
            using var _ = UseScope();

            var fun = InnerVisitFunction(context.blockFunction(), context.lambdaFunction(), true);

            var expr = context.expr() != null ? VisitExpr(context.expr()) : null;

            var typeExpr = context.typeAnnotation()?.typeExpr();
            var typ = typeExpr != null
                ? EvalTypeExpr(typeExpr)
                : null;

            var val = fun ?? expr;

            if (typ != null)
            {
                val?.AssertType(typ);
            }

            if (fun != null) return fun;

            return new Node()
            {
                TranslatedValue = val?.TranslatedValue ?? "",
                Location = GetTokenLocation(context),
                Type = typ ?? val.Type,
            };
        }

        public override Node VisitInstantiateObject([NotNull] InstantiateObjectContext context)
        {
            var name = context.nestedName().GetText(); // TODO: Cannot work for `.`-names yet (think namespacing)

            if (!typeScopes.HasBinding(name))
                throw new TranspilationException($"Cannot instantiate non-existant class {name}", GetTokenLocation(context));

            var classType = typeScopes.GetBinding(name);

            if (!(classType is ClassType c))
                throw new TranspilationException($"Cannot instantiate non-class value {name}", GetTokenLocation(context));

            var exprs = context.arguments()?.expr().Select(expr => VisitExpr(expr));

            if (c.Members.TryGetValue("constructor", out var constructor))
            {
                var paramTypes = ((FunctionType)constructor.Item1).ParameterTypes;

                if (exprs == null && paramTypes.Count != 0)
                    throw new TranspilationException($"Class with constructor expects {paramTypes.Count} arguments, but got none.", GetTokenLocation(context));

                // If constructor expects no arguments, exprs is null, since the user did not type any arguments, so we make an empty list.
                exprs ??= new List<Node>();

                var exprTypes = exprs.Select(expr => expr.Type).ToList();

                if (!exprTypes.Equals(paramTypes))
                {
                    var exprCount = exprTypes.Count;
                    var paramCount = paramTypes.Count;

                    if (exprCount != paramCount)
                    {
                        throw new TranspilationException($"Function '{name}' takes {paramCount} parameters, but got {exprCount}", GetTokenLocation(context));
                    }

                    for (int i = 0; i < exprCount; i++)
                    {
                        var expr = exprTypes[i];
                        var param = paramTypes[i];

                        if (!expr.Equals(param))
                        {
                            throw new TranspilationException($"Function '{name}' could not be invoked: parameter {i + 1} must be of type '{param.AsNativeName()}', but was '{expr.AsNativeName()}'", GetTokenLocation(context));
                        }
                    }
                }
            }
            else if (exprs?.Count() != 0)
            {
                throw new TranspilationException("Cannot pass arguments to class without a constructor", GetTokenLocation(context));
            }

            var exprText = exprs != null ? string.Join(",", exprs.Select(expr => expr.TranslatedValue)) : "";

            return new Node()
            {
                TranslatedValue = $"new {name}({exprText})",
                Type = c.ToObjectType(),
                Location = GetTokenLocation(context),
            };
        }
    }
}
