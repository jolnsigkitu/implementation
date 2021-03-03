using System.Text;
using System.Linq;
using Antlr4.Runtime.Misc;
using static LangParser;

namespace ITU.Lang.Core
{
    public class CSharpASTTranslator : LangBaseVisitor<string>
    {
        private Scope<string> scopes = new Scope<string>();

        public override string VisitProg([NotNull] ProgContext context)
        {
            scopes.Push();
            var res = VisitStatements(context.statements());
            return "using System; namespace App { public class Entrypoint { static void Main(string[] args) {" + res + "}}}";
        }

        public override string VisitChildren(Antlr4.Runtime.Tree.IRuleNode node)
        {
            var buf = new StringBuilder();
            for (var i = 0; i < node.ChildCount; i++)
            {
                var child = node.GetChild(i);
                if (child != null)
                {
                    buf.Append(Visit(child));
                }
            }
            return buf.ToString();
        }

        public override string VisitSemiStatement([NotNull] SemiStatementContext context)
        {
            return VisitChildren(context) + ";";
        }

        public override string VisitBlock([NotNull] BlockContext context)
        {
            scopes.Push();
            var subTree = base.VisitBlock(context);
            scopes.Pop();

            return "{" + subTree + "}";
        }

        public override string VisitIfStatement([NotNull] IfStatementContext context)
        {
            var expr = this.VisitExpr(context.expr());
            var block = this.VisitBlock(context.block());
            var elseIf = string.Join("", context.elseIfStatement().Select(VisitElseIfStatement));
            var elseRes = context.elseStatement() != null ? this.VisitElseStatement(context.elseStatement()) : "";
            return "if(" + expr + ")" + block + elseIf + elseRes;
        }

        public override string VisitElseIfStatement([NotNull] ElseIfStatementContext context)
        {
            var expr = this.VisitExpr(context.expr());
            var block = this.VisitBlock(context.block());

            return "else if(" + expr + ")" + block;
        }

        public override string VisitElseStatement([NotNull] ElseStatementContext context)
        {
            return "else" + base.VisitElseStatement(context);
        }

        public override string VisitVardec([NotNull] VardecContext context)
        {
            var name = context.typedName()?.Name()?.GetText();
            var expr = VisitExpr(context.expr());

            scopes.Bind(name, expr);

            // Const does not work with var :/
            // TODO: Add const when types are being output correctly

            return "var " + name + "=" + expr;
        }

        public override string VisitExpr([NotNull] ExprContext context)
        {
            var leftParen = context.LeftParen()?.GetText() ?? "";
            var rightParen = context.RightParen()?.GetText() ?? "";
            return leftParen + VisitChildren(context) + rightParen;
        }

        public override string VisitOperator([NotNull] OperatorContext context)
        {
            return context.GetText();
        }

        public override string VisitLiteral([NotNull] LiteralContext context)
        {
            return context.GetText();
        }

        public override string VisitAccess([NotNull] AccessContext context)
        {
            var name = context.GetText();
            var res = scopes.GetBinding(name);
            if (!scopes.HasBinding(name))
            {
                throw new TranspilationException("Variable '" + name + "' was not declared before accessing!");
            }

            return res;
        }

        // public override string VisitFunction([NotNull] FunctionContext context)
        // {

        // }
    }
}
