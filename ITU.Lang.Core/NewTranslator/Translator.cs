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
    public class Translator : LangBaseVisitor<Node>
    {
        private ITokenStream tokenStream;

        private TypeEvaluator typeEvaluator = new TypeEvaluator();

        public Translator(ITokenStream tokenStream)
        {
            this.tokenStream = tokenStream;
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

            if (exprs.Length == 1)
            {
                var isPrefix = children[0] is OperatorContext;
                return new UnaryOperatorNode(op, exprs[0], isPrefix, context);
            }

            return new BinaryOperatorNode(op, exprs[0], exprs[1], context);
        }

        public override ExprNode VisitTerm([NotNull] TermContext context)
        {
            return VisitFirstChild<ExprNode>(new ParserRuleContext[] {
                context.literal(),
                context.access(),
                context.function(),
            });
        }

        #region Literals
        public override ExprNode VisitLiteral([NotNull] LiteralContext context)
        {
            return VisitFirstChild<ExprNode>(new ParserRuleContext[] {
                context.integer(),
                context.@bool(),
                context.stringLiteral(),
                context.charLiteral(),
            });
        }

        public override LiteralNode VisitInteger([NotNull] IntegerContext context)
        {
            return new LiteralNode(context.GetText(), new IntType(), context);
        }

        public override LiteralNode VisitBool([NotNull] BoolContext context)
        {
            return new LiteralNode(context.GetText(), new BooleanType(), context);
        }

        public override LiteralNode VisitStringLiteral([NotNull] StringLiteralContext context)
        {
            return new LiteralNode(context.GetText(), new StringType(), context);
        }

        public override LiteralNode VisitCharLiteral([NotNull] CharLiteralContext context)
        {
            return new LiteralNode(context.GetText(), new CharType(), context);
        }
        #endregion

        public override VarDecNode VisitVardec([NotNull] VardecContext context)
        {
            var typedName = context.typedName();

            var name = typedName.Name().GetText();
            var expr = VisitExpr(context.expr());
            var isConst = context.Const() != null;
            var typeAnnotation = InvokeIf(typedName.typeAnnotation()?.typeExpr(), typeEvaluator.VisitTypeExpr);

            return new VarDecNode(name, isConst, expr, typeAnnotation, context);
        }

        public override AccessNode VisitAccess([NotNull] AccessContext context)
        {
            var name = context.Name()?.GetText();
            var expr = VisitFirstChild<ExprNode>(new ParserRuleContext[] {
                context.invokeFunction(),
                context.instantiateObject(),
                context.expr(),
            });

            var chain = InvokeIf(context.accessChain(), VisitAccessChain);

            return new AccessNode(name, expr, chain, context);
        }

        public override AccessChainNode VisitAccessChain([NotNull] AccessChainContext context)
        {
            var list = new List<ChainNode>();

            for (var rest = context; rest != null; rest = rest.accessChain())
            {
                list.Add(new ChainNode
                {
                    Name = rest.Name().GetText(),
                    Function = InvokeIf(rest.invokeFunction(), VisitInvokeFunction),
                });
            }

            return new AccessChainNode(list, context);
        }

        public override ExprNode VisitInvokeFunction([NotNull] InvokeFunctionContext context)
        {
            throw new System.NotImplementedException();
        }

        private T VisitFirstChild<T>(ParserRuleContext[] children) where T : Node
        {
            var child = children.FirstOrDefault(s => s != null);
            if (child == null)
            {
                return default(T);
            }
            return (T)Visit(child);
        }

        private R InvokeIf<T, R>(T value, System.Func<T, R> func) =>
            value != null ? func(value) : default(R);
    }
}
