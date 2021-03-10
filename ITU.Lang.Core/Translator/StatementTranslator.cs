using System.Text;
using System.Linq;

using Antlr4.Runtime.Misc;

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
                TranslatedValue = children.TranslatedValue + ";",
                Location = GetTokenLocation(context),
                Type = children.Type,
            };
        }

        public override Node VisitBlock([NotNull] BlockContext context)
        {
            Type blockType = null;
            var hasReturnStatement = false;
            var buf = new StringBuilder();

            scopes.Push();

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

            scopes.Pop();

            return new Node()
            {
                TranslatedValue = "{" + buf.ToString() + "}",
                Location = GetTokenLocation(context),
                Type = blockType,
            };
        }

        public override Node VisitIfStatement([NotNull] IfStatementContext context)
        {
            var expr = this.VisitExpr(context.expr());
            expr.AssertType(new BooleanType());

            var block = this.VisitBlock(context.block()).TranslatedValue;
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
            return new Node()
            {
                TranslatedValue = "else" + base.VisitElseStatement(context).TranslatedValue,
                Location = GetTokenLocation(context),
            };
        }

        public override Node VisitVardec([NotNull] VardecContext context)
        {
            var typedName = context.typedName();
            var name = typedName.Name().GetText();
            var typeAnnotationName = typedName.typeAnnotation()?.Name()?.GetText();
            Type typeAnnotation = null;

            var expr = VisitExpr(context.expr());

            if (expr.Type.Equals(new VoidType()))
            {
                throw new TranspilationException("Cannot assign variables to values of type 'void'!", GetTokenLocation(context));
            }

            if (typeAnnotation != null)
            {
                typeAnnotation = scopes.GetBinding(typeAnnotationName).Type;
                expr.AssertType(typeAnnotation);
            }

            var binding = new Node()
            {
                TranslatedValue = name,
                Type = typeAnnotation ?? expr.Type,
                IsConst = context.Const() != null,
            };

            scopes.Bind(name, binding);
            var constPrefix = binding.IsConst ? "const " : "";
            return new Node()
            {
                TranslatedValue = $"{constPrefix}{binding.Type.AsTranslatedName()} {name} = {expr.TranslatedValue}",
                Location = GetTokenLocation(context),
            };
        }

        public override Node VisitAssign([NotNull] AssignContext context)
        {
            var name = context.Name().GetText();
            if (!scopes.HasBinding(name))
            {
                throw new TranspilationException($"Variable {name} was not declared!", GetTokenLocation(context.Name()));
            }

            var expr = VisitExpr(context.expr());
            var cur = scopes.GetBinding(name);

            if (cur.IsConst)
            {
                throw new TranspilationException($"Cannot update const variable '{name}'!", GetTokenLocation(context.Name()));
            }

            if (!cur.IsType(expr))
            {
                var msg = $"Cannot assign value of type '{expr.Type.AsNativeName()}' to variable '{name}' of type '{cur.Type.AsNativeName()}'";
                throw new TranspilationException(msg, GetTokenLocation(context));
            }

            scopes.Rebind(name, expr);

            return new Node()
            {
                TranslatedValue = $"{name} = {expr.TranslatedValue}",
                Location = GetTokenLocation(context),
            };
        }
    }
}
