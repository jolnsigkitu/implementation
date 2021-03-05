using System.Text;
using System.Linq;
using Antlr4.Runtime.Misc;
using static LangParser;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace ITU.Lang.Core
{
    public class CSharpASTTranslator : LangBaseVisitor<CSharpASTNode>
    {
        private Scope<CSharpASTNode> scopes = new Scope<CSharpASTNode>();

        private ITokenStream tokenStream;

        public CSharpASTTranslator(ITokenStream tokenStream)
        {
            this.tokenStream = tokenStream;
        }

        public override CSharpASTNode VisitProg([NotNull] ProgContext context)
        {
            scopes.Push();
            var res = VisitStatements(context.statements());

            return new CSharpASTNode()
            {
                TranslatedValue = "using System; namespace App { public class Entrypoint { static void Main(string[] args) {" + res.TranslatedValue + "}}}",
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitChildren(Antlr4.Runtime.Tree.IRuleNode node)
        {
            var buf = new StringBuilder();
            string lastSeenType = null;
            for (var i = 0; i < node.ChildCount; i++)
            {
                var child = node.GetChild(i);
                if (child == null) continue;

                var res = Visit(child);
                if (res == null) continue;

                if (lastSeenType != null && res.TypeName != lastSeenType)
                {
                    var msg = $"Type mismatch: Expected type '{res.TypeName}' to be of type '{lastSeenType}'\n'{res.TranslatedValue}'";
                    throw new TranspilationException(msg, GetTokenLocation(child));
                }

                buf.Append(res.TranslatedValue);
                lastSeenType = res.TypeName;
            }

            return new CSharpASTNode()
            {
                TranslatedValue = buf.ToString(),
                TypeName = lastSeenType,
                Location = GetTokenLocation(node),
            };
        }

        #region Statements
        public override CSharpASTNode VisitSemiStatement([NotNull] SemiStatementContext context)
        {
            var children = VisitChildren(context);

            return new CSharpASTNode()
            {
                TranslatedValue = children.TranslatedValue + ";",
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitBlock([NotNull] BlockContext context)
        {
            scopes.Push();
            var subTree = base.VisitBlock(context);
            scopes.Pop();

            return new CSharpASTNode()
            {
                TranslatedValue = "{" + subTree.TranslatedValue + "}",
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitIfStatement([NotNull] IfStatementContext context)
        {
            var expr = this.VisitExpr(context.expr());
            expr.AssertType("boolean");

            var block = this.VisitBlock(context.block()).TranslatedValue;
            var elseIf = string.Join("", context.elseIfStatement().Select(x => VisitElseIfStatement(x).TranslatedValue));
            var elseRes = context.elseStatement() != null ? this.VisitElseStatement(context.elseStatement()).TranslatedValue : "";

            return new CSharpASTNode()
            {
                TranslatedValue = "if(" + expr.TranslatedValue + ")" + block + elseIf + elseRes,
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitElseIfStatement([NotNull] ElseIfStatementContext context)
        {
            var expr = this.VisitExpr(context.expr());

            expr.AssertType("boolean");

            var block = this.VisitBlock(context.block());

            return new CSharpASTNode()
            {
                TranslatedValue = "else if(" + expr.TranslatedValue + ")" + block.TranslatedValue,
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitElseStatement([NotNull] ElseStatementContext context)
        {
            return new CSharpASTNode()
            {
                TranslatedValue = "else" + base.VisitElseStatement(context).TranslatedValue,
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitVardec([NotNull] VardecContext context)
        {
            var typedName = context.typedName();
            var name = typedName.Name().GetText();
            var typeAnnotation = typedName.typeAnnotation()?.Name()?.GetText();

            var expr = VisitExpr(context.expr());
            var constPrefix = context.Const() != null ? "const " : "";

            if (typeAnnotation != null)
                expr.AssertType(typeAnnotation);

            var binding = new CSharpASTNode()
            {
                TranslatedValue = name,
                TypeName = typeAnnotation ?? expr.TypeName,
            };

            scopes.Bind(name, binding);

            return new CSharpASTNode()
            {
                TranslatedValue = $"{constPrefix}{expr.TypeName} {name} = {expr.TranslatedValue}",
                Location = GetTokenLocation(context),
            };
        }

        #endregion

        #region Expressions
        public override CSharpASTNode VisitExpr([NotNull] ExprContext context)
        {
            var leftParen = context.LeftParen()?.GetText() ?? "";
            var rightParen = context.RightParen()?.GetText() ?? "";
            var children = VisitChildren(context);

            return new CSharpASTNode()
            {
                TranslatedValue = leftParen + children.TranslatedValue + rightParen,
                TypeName = children.TypeName,
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitOperator([NotNull] OperatorContext context)
        {
            return new CSharpASTNode()
            {
                TranslatedValue = context.GetText(),
                TypeName = "int",
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitAccess([NotNull] AccessContext context)
        {
            var name = context.GetText();
            var res = scopes.GetBinding(name);
            if (!scopes.HasBinding(name))
            {
                throw new TranspilationException("Variable '" + name + "' was not declared before accessing!");
            }

            return res;
        }

        public override CSharpASTNode VisitBool([NotNull] BoolContext context)
        {
            return new CSharpASTNode()
            {
                TranslatedValue = (context.False() ?? context.True()).GetText(),
                TypeName = "boolean",
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitInt([NotNull] IntContext context)
        {
            return new CSharpASTNode()
            {
                TranslatedValue = context.Int().GetText(),
                TypeName = "int",
                Location = GetTokenLocation(context),
            };
        }

        // public override string VisitFunction([NotNull] FunctionContext context)
        // {

        // }
        #endregion

        #region Helpers
        private TokenLocation GetTokenLocation(ISyntaxTree node)
        {
            var interval = node.SourceInterval;
            var start = tokenStream.Get(interval.a);
            var end = tokenStream.Get(interval.b);
            return new TokenLocation(start, end);
        }
        #endregion
    }
}
