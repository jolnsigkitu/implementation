using System.Linq;
using System.Collections.Generic;

using Antlr4.Runtime.Misc;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using ITU.Lang.Core.Types;
using ITU.Lang.Core.Grammar;
using ITU.Lang.Core.Translator.Nodes;
using static ITU.Lang.Core.Grammar.LangParser;
using ITU.Lang.Core.Translator.Nodes.Expressions;
using ITU.Lang.Core.Translator.TypeNodes;

namespace ITU.Lang.Core.Translator
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
            return new ProgNode(statements, GetLocation(context));
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
            return new SemiStatementNode(child, GetLocation(context));
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
                return new UnaryOperatorNode(op, exprs[0], isPrefix, GetLocation(context));
            }

            return new BinaryOperatorNode(op, exprs[0], exprs[1], GetLocation(context));
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
            return new LiteralNode(context.GetText(), new IntType(), GetLocation(context));
        }

        public override LiteralNode VisitBool([NotNull] BoolContext context)
        {
            return new LiteralNode(context.GetText(), new BooleanType(), GetLocation(context));
        }

        public override LiteralNode VisitStringLiteral([NotNull] StringLiteralContext context)
        {
            return new LiteralNode(context.GetText(), new StringType(), GetLocation(context));
        }

        public override LiteralNode VisitCharLiteral([NotNull] CharLiteralContext context)
        {
            return new LiteralNode(context.GetText(), new CharType(), GetLocation(context));
        }
        #endregion

        public override VarDecNode VisitVardec([NotNull] VardecContext context)
        {
            var typedName = context.typedName();

            var name = typedName.Name().GetText();
            var expr = VisitExpr(context.expr());
            var isConst = context.Const() != null;
            var typeAnnotation = InvokeIf(typedName.typeAnnotation(), typeEvaluator.VisitTypeAnnotation);

            return new VarDecNode(name, isConst, expr, typeAnnotation, GetLocation(context));
        }

        public override TypeDecNode VisitTypedec([NotNull] TypedecContext context)
        {
            var name = context.Name().GetText();
            var typeDecNode = InvokeIf(context.typeExpr(), typeEvaluator.VisitTypeExpr);
            var classDecNode = InvokeIf(context.classExpr(), VisitClassExpr);

            // Since name is not available in ClassExprContext we monkey-patch it in here
            if (classDecNode != null)
            {
                classDecNode.Type.Name = name;
            }

            return new TypeDecNode(name, typeDecNode, classDecNode, GetLocation(context));
        }

        public override ClassNode VisitClassExpr([NotNull] ClassExprContext context)
        {
            var members = context.classMember().Select(VisitClassMember).ToList();
            return new ClassNode(members, GetLocation(context));
        }

        public override ClassMemberNode VisitClassMember([NotNull] ClassMemberContext context)
        {
            var name = context.Name().GetText();
            // Signify Field member
            var expr = InvokeIf(context.expr(), VisitExpr);
            // Signify Method member
            var func = InnerVisitFunction(context.blockFunction(), context.lambdaFunction());
            var annotation = InvokeIf(context.typeAnnotation(), typeEvaluator.VisitTypeAnnotation);
            return new ClassMemberNode(name, expr, func, annotation, GetLocation(context));
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

            return new AccessNode(name, expr, chain, hasParens, GetLocation(context));
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

            return new AccessChainNode(list, GetLocation(context));
        }

        public override Node VisitAssign([NotNull] AssignContext context)
        {
            var names = context.nestedName().Name().Select(x => x.GetText()).ToList();
            var expr = VisitExpr(context.expr());

            return new AssignNode(names, expr, GetLocation(context));
        }

        public override ExprNode VisitInstantiateObject([NotNull] InstantiateObjectContext context)
        {
            throw new System.NotImplementedException();
        }

        public override FunctionNode VisitFunction([NotNull] FunctionContext context)
        {
            return InnerVisitFunction(context.blockFunction(), context.lambdaFunction());
        }

        private FunctionNode InnerVisitFunction(BlockFunctionContext blockFun, LambdaFunctionContext lambdaFun)
        {
            if (blockFun == null && lambdaFun == null) return null;

            var handleCtx = blockFun?.genericHandle() ?? lambdaFun?.genericHandle();
            var handle = InvokeIf(handleCtx, VisitGenericHandle);

            var parameterListCtx = blockFun?.functionParameterList() ?? lambdaFun.functionParameterList();
            var parameterList = InvokeIf(parameterListCtx, VisitFunctionParameterList);

            var blockFunctionBody = InvokeIf(blockFun?.block(), VisitBlock);
            var lambdaFunctionBody = InvokeIf(lambdaFun?.expr(), VisitExpr);
            var body = blockFunctionBody ?? lambdaFunctionBody;

            var isLambda = lambdaFunctionBody != null;

            return new FunctionNode(parameterList, body, handle, isLambda, GetLocation(((ISyntaxTree)blockFun) ?? lambdaFun));
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

            return new ParameterListNode(nameTypePairs, returnType, GetLocation(context));
        }

        public override InvokeFunctionNode VisitInvokeFunction([NotNull] InvokeFunctionContext context)
        {
            var name = context.Name().GetText();
            var arguments = context.arguments()?.expr()?.Select(VisitExpr);

            return new InvokeFunctionNode(name, arguments, GetLocation(context));
        }

        public override BlockNode VisitBlock([NotNull] BlockContext context)
        {
            var statements = InvokeIf(context.statements(), VisitStatements);
            return new BlockNode(statements, GetLocation(context));
        }

        public override ReturnStatementNode VisitReturnStatement([NotNull] ReturnStatementContext context)
        {
            var expr = VisitExpr(context.expr());

            return new ReturnStatementNode(expr, GetLocation(context));
        }

        public override GenericHandleNode VisitGenericHandle([NotNull] GenericHandleContext context)
        {
            var names = context.Name().Select(n => n.GetText());
            return new GenericHandleNode(names, GetLocation(context));
        }

        public override IfStatementNode VisitIfStatement([NotNull] IfStatementContext context)
        {
            var expr = VisitExpr(context.expr());
            var block = VisitBlock(context.block());

            var elseIfStatements = context.elseIfStatement().Select(VisitElseIfStatement);
            var elseStatement = InvokeIf(context.elseStatement(), VisitElseStatement);

            return new IfStatementNode(expr, block, elseIfStatements, elseStatement, GetLocation(context));
        }

        public override ElseIfStatementNode VisitElseIfStatement([NotNull] ElseIfStatementContext context)
        {
            var expr = VisitExpr(context.expr());
            var block = VisitBlock(context.block());

            return new ElseIfStatementNode(expr, block, GetLocation(context));
        }
        public override ElseStatementNode VisitElseStatement([NotNull] ElseStatementContext context)
        {
            var block = VisitBlock(context.block());

            return new ElseStatementNode(block, GetLocation(context));
        }

        public override WhileStatementNode VisitWhileStatement([NotNull] WhileStatementContext context)
        {
            var expr = VisitExpr(context.expr());

            var block = InvokeIf(context.block(), VisitBlock);
            var statement = InvokeIf(context.statement(), VisitStatement);

            return new WhileStatementNode(expr, block, statement, GetLocation(context));
        }

        public override DoWhileStatementNode VisitDoWhileStatement([NotNull] DoWhileStatementContext context)
        {
            var expr = VisitExpr(context.expr());

            var block = InvokeIf(context.block(), VisitBlock);

            return new DoWhileStatementNode(expr, block, GetLocation(context));
        }

        public override ForStatementNode VisitForStatement([NotNull] ForStatementContext context)
        {
            var declaration = InvokeIf(context.forDecStatement()?.inlineStatement(), VisitInlineStatement);
            var condition = InvokeIf(context.forConExpression()?.expr(), VisitExpr);
            var increment = InvokeIf(context.forIncStatement()?.inlineStatement(), VisitInlineStatement);
            var block = InvokeIf(context.block(), VisitBlock);
            var statement = InvokeIf(context.statement(), VisitStatement);
            var body = (Node)block ?? statement;
            return new ForStatementNode(declaration, condition, increment, body, GetLocation(context));
        }

        public override LoopStatementNode VisitLoopStatement([NotNull] LoopStatementContext context)
        {
            var block = InvokeIf(context.block(), VisitBlock);
            var statement = InvokeIf(context.statement(), VisitStatement);
            var body = (Node)block ?? statement;

            return new LoopStatementNode(body, GetLocation(context));
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

        private TokenLocation GetLocation(ISyntaxTree node)
        {
            var interval = node.SourceInterval;
            var start = tokenStream.Get(interval.a);
            var end = tokenStream.Get(interval.b);
            return new TokenLocation(start, end);
        }
    }
}
