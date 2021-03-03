using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;

namespace ITU.Lang.Core
{
    public class CSharpASTTranslator : LangBaseVisitor<string>
    {
        private Stack<Dictionary<string, string>> scopes = new Stack<Dictionary<string, string>>();
        // private Dictionary<string, string> variableBindings = new Dictionary<string, string>();
        Dictionary<string, string> variableBindings
        {
            get
            {
                return scopes.Peek();
            }
        }

        public CSharpASTTranslator()
        {
            pushScope();
        }

        private void pushScope()
        {
            scopes.Push(new Dictionary<string, string>());
        }

        private void popScope()
        {
            scopes.Pop();
        }

        public override string VisitProg([NotNull] LangParser.ProgContext context)
        {
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

        public override string VisitSemiStatement([NotNull] LangParser.SemiStatementContext context)
        {
            return VisitChildren(context) + ";";
        }

        public override string VisitBlock([NotNull] LangParser.BlockContext context)
        {
            pushScope();
            var subTree = base.VisitBlock(context);
            popScope();

            return "{" + subTree + "}";
        }

        public override string VisitIfStatement([NotNull] LangParser.IfStatementContext context)
        {
            var expr = this.VisitExpr(context.expr());
            var block = this.VisitBlock(context.block());
            var elseIf = string.Join("", context.elseIfStatement().Select(VisitElseIfStatement));
            var elseRes = context.elseStatement() != null ? this.VisitElseStatement(context.elseStatement()) : "";
            return "if(" + expr + ")" + block + elseIf + elseRes;
        }

        public override string VisitElseIfStatement([NotNull] LangParser.ElseIfStatementContext context)
        {
            var expr = this.VisitExpr(context.expr());
            var block = this.VisitBlock(context.block());

            return "else if(" + expr + ")" + block;
        }

        public override string VisitElseStatement([NotNull] LangParser.ElseStatementContext context)
        {
            return "else" + base.VisitElseStatement(context);
        }

        public override string VisitVardec([NotNull] LangParser.VardecContext context)
        {
            var name = context.typedName()?.Name()?.GetText();

            if (variableBindings.ContainsKey(name))
            {
                throw new TranspilationException("Variable '" + name + "' has already been declared!");
            }
            variableBindings.Add(name, "stuff");

            // Const does not work with var :/
            // TODO: Add const when types are being output correctly

            return "var " + name + "=" + VisitExpr(context.expr());
        }

        public override string VisitExpr([NotNull] LangParser.ExprContext context)
        {
            var leftParen = context.LeftParen()?.GetText() ?? "";
            var rightParen = context.RightParen()?.GetText() ?? "";
            return leftParen + VisitChildren(context) + rightParen;
        }

        public override string VisitOperator([NotNull] LangParser.OperatorContext context)
        {
            return context.GetText();
        }

        public override string VisitLiteral([NotNull] LangParser.LiteralContext context)
        {
            return context.GetText();
        }

        public override string VisitAccess([NotNull] LangParser.AccessContext context)
        {
            var name = context.GetText();

            if (!variableBindings.ContainsKey(name))
            {
                throw new TranspilationException("Variable '" + name + "' was not declared before accessing!");
            }

            return name;
        }

    }
}
