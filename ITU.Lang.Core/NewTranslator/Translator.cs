using System.Linq;
using System.Collections.Generic;

using Antlr4.Runtime.Misc;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using ITU.Lang.Core.Types;
using ITU.Lang.Core.Grammar;
using ITU.Lang.Core.NewTranslator.Nodes;
using static ITU.Lang.Core.Grammar.LangParser;
using ITU.Lang.Core.NewTranslator.Nodes.Expressions;
using ITU.Lang.Core.Operators;

namespace ITU.Lang.Core.NewTranslator
{
    class Translator : LangBaseVisitor<Node>
    {
        private ITokenStream tokenStream;

        private OperatorFactory operators = Operators.Operators.InitializeOperators(new OperatorFactory());

        public Translator(ITokenStream tokenStream)
        {
            this.tokenStream = tokenStream;

            MakeGlobalScope();
        }

        public override ProgNode VisitProg([NotNull] ProgContext context)
        {
            var statements = context.statements().statement().Select(VisitStatement).ToList();
            return new ProgNode(context, statements);
        }

        public override StatementNode VisitStatement([NotNull] StatementContext context)
        {
            return VisitFirstChild<StatementNode>(new ParserRuleContext[] {
                context.semiStatement(),
                context.ifStatement(),
                context.forStatement(),
                context.whileStatement(),
                context.doWhileStatement(),
                context.loopStatement(),
                context.returnStatement(),
            });
        }

        public override SemiStatementNode VisitSemiStatement([NotNull] SemiStatementContext context)
        {
            var inlineStatement = context.inlineStatement();
            var child = VisitFirstChild<Node>(new ParserRuleContext[] {
                inlineStatement.assign(),
                inlineStatement.vardec(),
                inlineStatement.typedec(),
                inlineStatement.expr(),
            });
            return new SemiStatementNode(context, child);
        }

        public override ExprNode VisitExpr([NotNull] ExprContext context)
        {
            var op = context.@operator()?.GetText();

            if (op == null) return VisitTerm(context.term());

            // operator-variant
            var exprs = context.expr().Select(VisitExpr).ToArray();
            var children = context.children;
            if (children[0] is OperatorContext) // unary pre
                return operators.UnaryPrefix.Get(op, exprs[0]);
            else if (exprs.Length == 1) // unary post
                return operators.UnaryPostfix.Get(op, exprs[0]);
            else // binary
                return operators.Binary.Get(op, exprs[0], exprs[1]);
        }

        public override ExprNode VisitTerm([NotNull] TermContext context)
        {
            return VisitFirstChild<ExprNode>(new ParserRuleContext[] {
                context.literal(),
                context.access(),
                context.function(),
            });
        }

        public override ExprNode VisitLiteral([NotNull] LiteralContext context)
        {
            return VisitFirstChild<ExprNode>(new ParserRuleContext[] {
                context.integer(),
                context.@bool(),
                context.stringLiteral(),
                context.charLiteral(),
            });
        }

        public override ExprNode VisitInteger([NotNull] IntegerContext context)
        {
            return new LiteralNode(context.GetText(), new IntType(), context);
        }

        public override ExprNode VisitBool([NotNull] BoolContext context)
        {
            return new LiteralNode(context.GetText(), new BooleanType(), context);
        }

        public override ExprNode VisitStringLiteral([NotNull] StringLiteralContext context)
        {
            return new LiteralNode(context.GetText(), new StringType(), context);
        }

        public override ExprNode VisitCharLiteral([NotNull] CharLiteralContext context)
        {
            return new LiteralNode(context.GetText(), new CharType(), context);
        }

        private T VisitFirstChild<T>(ParserRuleContext[] children) where T : Node
        {
            return (T)Visit(children.First(s => s != null));
        }

        private void MakeGlobalScope()
        {

        }
    }
}
