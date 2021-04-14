using System.Text;
using System.Linq;

using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

using ITU.Lang.Core.Types;
using static ITU.Lang.Core.Grammar.LangParser;

namespace ITU.Lang.Core.Translator
{
    public partial class Translator
    {

        public override Node VisitSemiStatement([NotNull] SemiStatementContext context)
        {
            var children = VisitChildren(context);

            return new Node()
            {
                TranslatedValue = string.IsNullOrEmpty(children.TranslatedValue) ? "" : children.TranslatedValue + ";\n",
                Location = GetTokenLocation(context),
                Type = children.Type,
            };
        }

        public override Node VisitBlock([NotNull] BlockContext context)
        {
            Type blockType = new VoidType();
            var hasReturnStatement = false;
            var buf = new StringBuilder();

            foreach (var statement in context.statements()?.statement() ?? new StatementContext[0])
            {
                if (hasReturnStatement) throw new TranspilationException("Statements after return will be ignored");

                var returnStatement = statement.returnStatement();

                Node res;
                if (returnStatement != null)
                {
                    res = VisitReturnStatement(returnStatement);
                    blockType = res.Type;
                    hasReturnStatement = true;
                }
                else
                {
                    res = Visit(statement);
                }

                buf.Append(res.TranslatedValue);
            }

            return new Node()
            {
                TranslatedValue = "{" + buf.ToString() + "}",
                Location = GetTokenLocation(context),
                Type = blockType,
            };
        }

        public override Node VisitIfStatement([NotNull] IfStatementContext context)
        {
            PushScope();

            var expr = this.VisitExpr(context.expr());
            expr.AssertType(new BooleanType());

            var block = this.VisitBlock(context.block()).TranslatedValue;

            PopScope();

            var elseIf = string.Join("", context.elseIfStatement().Select(x => VisitElseIfStatement(x).TranslatedValue));
            var elseRes = context.elseStatement() != null ? this.VisitElseStatement(context.elseStatement()).TranslatedValue : "";

            return new Node()
            {
                TranslatedValue = "if(" + expr.TranslatedValue + ")" + block + elseIf + elseRes,
                Location = GetTokenLocation(context),
            };
        }

        public override Node VisitElseIfStatement([NotNull] ElseIfStatementContext context)
        {
            using var _ = UseScope();

            var expr = this.VisitExpr(context.expr());

            expr.AssertType(new BooleanType());

            var block = this.VisitBlock(context.block());

            return new Node()
            {
                TranslatedValue = "else if(" + expr.TranslatedValue + ")" + block.TranslatedValue,
                Location = GetTokenLocation(context),
            };
        }

        public override Node VisitElseStatement([NotNull] ElseStatementContext context)
        {
            using var _ = UseScope();

            var val = "else" + base.VisitElseStatement(context).TranslatedValue;
            return new Node()
            {
                TranslatedValue = val,
                Location = GetTokenLocation(context),
            };
        }

        public override Node VisitVardec([NotNull] VardecContext context)
        {
            var typedName = context.typedName();
            var name = typedName.Name().GetText();
            var typeAnnotation = typedName.typeAnnotation() != null
                ? EvalTypeExpr(typedName.typeAnnotation().typeExpr())
                : null;

            var expr = VisitExpr(context.expr());

            if (expr.Type.Equals(new VoidType()))
            {
                throw new TranspilationException("Cannot assign variables to values of type 'void'!", GetTokenLocation(context));
            }

            if (typeAnnotation != null)
            {
                expr.AssertType(typeAnnotation);
            }

            var binding = new Node()
            {
                TranslatedValue = name,
                Type = typeAnnotation ?? expr.Type,
                IsConst = context.Const() != null,
            };

            scopes.Bind(name, binding);
            return new Node()
            {
                TranslatedValue = $"{binding.Type.AsTranslatedName()} {name} = {expr.TranslatedValue}",
                Location = GetTokenLocation(context),
            };
        }

        public override Node VisitTypedec([NotNull] TypedecContext context)
        {
            var name = context.Name().GetText();

            if (typeScopes.HasBinding(name))
            {
                throw new TranspilationException($"Cannot redeclare type '{name}'", GetTokenLocation(context));
            }

            if (context.typeExpr()?.classExpr() != null)
            {
                var classVar = VisitClassExpr(context.typeExpr().classExpr());

                var classType = (ClassType)classVar.Type;
                classType.Name = name;

                classes.Add(classType);
                typeScopes.Bind(name, classType);
            }
            else
            {
                var typeExpr = EvalTypeExpr(context.typeExpr());

                if (typeExpr is VoidType)
                {
                    throw new TranspilationException("Cannot assign void to a type", GetTokenLocation(context));
                }

                typeScopes.Bind(name, typeExpr);
            }

            return new Node()
            {
                TranslatedValue = "",
                Location = GetTokenLocation(context),
            };
        }

        private Type EvalTypeExpr([NotNull] TypeExprContext context)
        {
            var func = context.funcTypeExpr();

            if (func != null)
            {
                var typ = new FunctionType()
                {
                    ReturnType = InvokeIf(func.typeExpr(), EvalTypeExpr) ?? new VoidType(),
                    ParameterTypes = func.funcTypeExprParamList().typeExpr().Select(EvalTypeExpr).ToList(),
                };

                var hasGenericReturnType = typ.ReturnType is GenericTypeIdentifier || typ.ReturnType is GenericFunctionType;
                var hasGenericParameterType = typ.ParameterTypes.Any(t => t is GenericTypeIdentifier || t is GenericFunctionType);
                if (hasGenericReturnType || hasGenericParameterType)
                {
                    typ = new GenericFunctionType(typ);
                }

                return typ;
            }

            var typeRef = context.typeRef();
            var name = typeRef.Name().GetText();
            if (name == "void")
            {
                return new VoidType();
            }
            var boundType = typeScopes.GetBinding(name);

            if (boundType == null)
            {
                throw new TranspilationException($"Type '{name}' is not yet bound", GetTokenLocation(context));
            }

            var handle = typeRef.genericHandle();

            if (handle == null)
            {
                return boundType;
            }

            if (!(boundType is GenericFunctionType generic))
            {
                throw new TranspilationException("Cannot specify generic arguments for non-generic type", GetTokenLocation(context));
            }

            var handleNames = handle.Name().Select(n => n.GetText());
            var handleTypes = handleNames.Select(n => typeScopes.GetBinding(n)).ToList();

            var resolutions = generic.Resolve(handleTypes);
            return generic.Specify(resolutions);
        }

        public override Node VisitAssign([NotNull] AssignContext context)
        {
            var names = context.nestedName().Name().Select(x => x.GetText()).ToList();
            var fullName = string.Join(".", names);
            var firstName = names[0];

            if (!scopes.HasBinding(firstName))
            {
                throw new TranspilationException($"Variable '{firstName}' was not declared before assigning", GetTokenLocation(context));
            }

            var binding = scopes.GetBinding(firstName);
            var typ = binding.Type;

            foreach (var name in names.GetRange(1, names.Count - 1))
            {
                if (!(typ is ObjectType n))
                    throw new TranspilationException($"Cannot access property '{name}' on non-object", GetTokenLocation(context));

                var member = n.GetMember(name);

                if (member == null)
                    throw new TranspilationException($"Cannot access member '{name}' on object '{n.AsNativeName()}'", GetTokenLocation(context));

                typ = member;
            }

            var expr = VisitExpr(context.expr());

            if (!expr.IsType(typ))
            {
                var msg = $"Cannot assign value of type '{expr.Type.AsNativeName()}' to variable '{fullName}' of type '{typ.AsNativeName()}'";
                throw new TranspilationException(msg, GetTokenLocation(context));
            }

            if (names.Count == 1)
            {
                if (binding.IsConst)
                    throw new TranspilationException($"Cannot update const variable '{fullName}'", GetTokenLocation(context.nestedName()));

                scopes.Rebind(firstName, expr);
            }

            return new Node()
            {
                TranslatedValue = $"{fullName} = {expr.TranslatedValue}",
                Location = GetTokenLocation(context),
            };
        }

        #region Loop statements
        public override Node VisitWhileStatement([NotNull] WhileStatementContext context)
        {
            using var _ = UseScope();

            var expr = VisitExpr(context.expr());

            expr.AssertType(new BooleanType());

            var block = VisitIfExists(context.block());
            var statement = VisitIfExists(context.statement());
            var body = block?.TranslatedValue ?? $"{{{statement.TranslatedValue}}}";

            return new Node()
            {
                TranslatedValue = $"while({expr.TranslatedValue}){body}",
                Location = GetTokenLocation(context),
            };
        }

        public override Node VisitDoWhileStatement([NotNull] DoWhileStatementContext context)
        {
            using var _ = UseScope();

            var exprContext = context.expr();
            var expr = VisitExpr(exprContext);

            expr.AssertType(new BooleanType());

            return new Node()
            {
                TranslatedValue = $"do {VisitBlock(context.block()).TranslatedValue} while({expr.TranslatedValue});",
                Location = GetTokenLocation(context),
            };
        }

        public override Node VisitLoopStatement([NotNull] LoopStatementContext context)
        {
            using var _ = UseScope();

            var block = VisitIfExists(context.block());
            var statement = VisitIfExists(context.statement());
            var body = block?.TranslatedValue ?? $"{{{statement.TranslatedValue}}}";

            return new Node()
            {
                TranslatedValue = $"while(true){body}",
                Location = GetTokenLocation(context),
            };
        }

        public override Node VisitForStatement([NotNull] ForStatementContext context)
        {
            using var _ = UseScope();

            var decExpr = VisitIfExists(context.forDecStatement()?.inlineStatement());
            var conExpr = VisitIfExists(context.forConExpression()?.expr());
            var incExpr = VisitIfExists(context.forIncStatement()?.inlineStatement());

            conExpr?.AssertType(new BooleanType());

            var decExprText = decExpr?.TranslatedValue ?? "";
            var incExprText = incExpr?.TranslatedValue ?? "";
            var conExprText = conExpr?.TranslatedValue ?? "";

            var block = VisitIfExists(context.block());
            var statement = VisitIfExists(context.statement());
            var body = block?.TranslatedValue ?? $"{{{statement.TranslatedValue}}}";

            return new Node()
            {
                TranslatedValue = $"for({decExprText};{conExprText};{incExprText}){body}",
                Location = GetTokenLocation(context),
            };
        }
        #endregion
    }
}
