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
using ITU.Lang.Core.NewTranslator.TypeNodes;

namespace ITU.Lang.Core.NewTranslator
{
    public class Translator : LangBaseVisitor<Node>
    {
        private ITokenStream tokenStream;

        private TypeEvaluator typeEvaluator;

        public Translator(ITokenStream tokenStream)
        {
            this.tokenStream = tokenStream;
            typeEvaluator = new TypeEvaluator(this);
        }

        public override ProgNode VisitProg([NotNull] ProgContext context)
        {
            var statements = VisitStatements(context.statements());
            return new ProgNode(context, statements);
        }

        public new IList<StatementNode> VisitStatements([NotNull] StatementsContext context)
        {
            return context.statement().Select(VisitStatement).ToList();
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
            var typeAnnotation = InvokeIf(typedName.typeAnnotation(), typeEvaluator.VisitTypeAnnotation);

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

            // No need to check for right parenthesis because of parser rules enforcing matching parens
            var hasParens = context.LeftParen() != null;

            var chain = InvokeIf(context.accessChain(), VisitAccessChain);

            return new AccessNode(name, expr, chain, hasParens, context);
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

        public override ExprNode VisitInstantiateObject([NotNull] InstantiateObjectContext context)
        {
            throw new System.NotImplementedException();
        }

        public override FunctionNode VisitFunction([NotNull] FunctionContext context)
        {
            var handle = InvokeIf(context.genericHandle(), VisitGenericHandle);

            var blockFun = context.blockFunction();
            var lambdaFun = context.lambdaFunction();

            var parameterListCtx = blockFun?.functionParameterList() ?? lambdaFun.functionParameterList();
            var parameterList = InvokeIf(parameterListCtx, VisitFunctionParameterList);

            var blockFunctionBody = InvokeIf(blockFun?.block(), VisitBlock);
            var lambdaFunctionBody = InvokeIf(lambdaFun?.expr(), VisitExpr);
            var body = blockFunctionBody ?? lambdaFunctionBody;

            var isLambda = lambdaFunctionBody != null;

            return new FunctionNode(parameterList, body, handle, isLambda, context);
        }

        public override ParameterListNode VisitFunctionParameterList([NotNull] FunctionParameterListContext context)
        {
            var args = context.functionArguments();
            var names = args.Name().Select(n => n.GetText());
            var types = args.typeAnnotation().Select(t => typeEvaluator.VisitTypeExpr(t.typeExpr()));
            var nameTypePairs = names.Zip(types, (n, t) => (n, t));

            var typeAnnotation = InvokeIf(context.typeAnnotation(), typeEvaluator.VisitTypeAnnotation);

            var returnType = context.Void() != null
                ? new StaticTypeNode(new VoidType())
                : typeAnnotation;

            return new ParameterListNode(nameTypePairs, returnType, context);
        }

        public override InvokeFunctionNode VisitInvokeFunction([NotNull] InvokeFunctionContext context)
        {
            var name = context.Name().GetText();
            var arguments = context.arguments()?.expr()?.Select(VisitExpr);

            return new InvokeFunctionNode(name, arguments, context);
        }

        public override BlockNode VisitBlock([NotNull] BlockContext context)
        {
            var statements = InvokeIf(context.statements(), VisitStatements);
            return new BlockNode(statements, context);
        }

        public override ReturnStatementNode VisitReturnStatement([NotNull] ReturnStatementContext context)
        {
            var expr = VisitExpr(context.expr());

            return new ReturnStatementNode(expr, context);
        }

        public override GenericHandleNode VisitGenericHandle([NotNull] GenericHandleContext context)
        {
            var names = context.Name().Select(n => n.GetText());
            return new GenericHandleNode(names, context);
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
