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
            var statements = context.statements().Invoke(VisitStatements);
            statements ??= new List<StatementNode>();
            return new ProgNode(statements, GetLocation(context));
        }

        public new IList<StatementNode> VisitStatements([NotNull] StatementsContext context)
        {
            return context.statement().Select(VisitStatement).ToList();
        }

        public override StatementNode VisitStatement([NotNull] StatementContext context)
        {
            return this.VisitFirstChild<StatementNode, Node>(new ParserRuleContext[] {
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
            var child = this.VisitFirstChild<Node, Node>(new ParserRuleContext[] {
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
            return this.VisitFirstChild<ExprNode, Node>(new ParserRuleContext[] {
                context.literal(),
                context.access(),
                context.function(),
            });
        }

        #region Literals
        public override ExprNode VisitLiteral([NotNull] LiteralContext context)
        {
            return this.VisitFirstChild<ExprNode, Node>(new ParserRuleContext[] {
                context.number(),
                context.@bool(),
                context.stringLiteral(),
                context.charLiteral(),
            });
        }

        public override LiteralNode VisitNumber([NotNull] NumberContext context)
        {
            var doub = context.Double();
            var txt = context.GetText();
            IType typ = new IntType();
            if (doub != null)
            {
                typ = new DoubleType();
            }

            return new LiteralNode(context.GetText(), typ, GetLocation(context));
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
            var typeAnnotation = typedName.typeAnnotation().Invoke(typeEvaluator.VisitTypeAnnotation);

            return new VarDecNode(name, isConst, expr, typeAnnotation, GetLocation(context));
        }

        public override TypeDecNode VisitTypedec([NotNull] TypedecContext context)
        {
            var name = context.Name().GetText();
            var isExtern = context.Extern() != null;
            var typeDecNode = context.typeExpr().Invoke(typeEvaluator.VisitTypeExpr);
            var classDecNode = context.classExpr().Invoke(VisitClassExpr);

            // Since name is not available in ClassExprContext we monkey-patch it in here
            if (classDecNode != null)
            {
                classDecNode.ClassName = name;
            }

            return new TypeDecNode(name, typeDecNode, classDecNode, isExtern, GetLocation(context));
        }

        public override AccessNode VisitAccess([NotNull] AccessContext context)
        {
            var name = context.Name()?.GetText();
            var expr = this.VisitFirstChild<ExprNode, Node>(new ParserRuleContext[] {
                context.invokeFunction(),
                context.instantiateObject(),
                context.expr(),
            });

            // No need to check for right parenthesis because of parser rules enforcing matching parens
            var hasParens = context.LeftParen() != null;

            var chain = context.accessChain().Invoke(VisitAccessChain);

            return new AccessNode(name, expr, chain, hasParens, GetLocation(context));
        }

        public override AccessChainNode VisitAccessChain([NotNull] AccessChainContext context)
        {
            var list = new List<AccessChainLink>();

            for (var rest = context; rest != null; rest = rest.accessChain())
            {
                if (rest.Name() != null)
                {

                    list.Add(new NameAccessChainLink(rest.Name().GetText()));
                }
                else
                {
                    var node = VisitInvokeFunction(rest.invokeFunction());
                    list.Add(new FunctionAccessChainLink(node));
                }
            }

            return new AccessChainNode(list, GetLocation(context));
        }

        public override Node VisitAssign([NotNull] AssignContext context)
        {
            var names = context.nestedName().Name().Select(x => x.GetText()).ToList();
            var expr = VisitExpr(context.expr());

            return new AssignNode(names, expr, GetLocation(context));
        }

        public override FunctionNode VisitFunction([NotNull] FunctionContext context)
        {
            return InnerVisitFunction(context.blockFunction(), context.lambdaFunction());
        }

        private FunctionNode InnerVisitFunction(BlockFunctionContext blockFun, LambdaFunctionContext lambdaFun)
        {
            if (blockFun == null && lambdaFun == null) return null;

            var handleCtx = blockFun?.genericHandle() ?? lambdaFun?.genericHandle();
            var handle = handleCtx.Invoke(VisitGenericHandle);

            var parameterListCtx = blockFun?.functionParameterList() ?? lambdaFun.functionParameterList();
            var parameterList = parameterListCtx.Invoke(VisitFunctionParameterList);

            var blockFunctionBody = blockFun?.block().Invoke(VisitBlock);
            var lambdaFunctionBody = lambdaFun?.expr().Invoke(VisitExpr);
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

            var typeAnnotation = context.typeAnnotation().Invoke(typeEvaluator.VisitTypeAnnotation);

            var returnType = context.Void() != null
                ? new StaticTypeNode(new VoidType())
                : typeAnnotation;

            return new ParameterListNode(nameTypePairs, returnType, GetLocation(context));
        }

        public override InvokeFunctionNode VisitInvokeFunction([NotNull] InvokeFunctionContext context)
        {
            var name = context.Name().GetText();
            var arguments = context.arguments()?.expr()?.Select(VisitExpr)?.ToList();
            var handle = context.genericHandle().Invoke(VisitGenericHandle);

            return new InvokeFunctionNode(name, arguments, handle, GetLocation(context));
        }

        public override BlockNode VisitBlock([NotNull] BlockContext context)
        {
            var statements = context.statements().Invoke(VisitStatements);
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
            var elseStatement = context.elseStatement().Invoke(VisitElseStatement);

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

            var block = context.block().Invoke(VisitBlock);
            var statement = context.statement().Invoke(VisitStatement);

            return new WhileStatementNode(expr, block, statement, GetLocation(context));
        }

        public override DoWhileStatementNode VisitDoWhileStatement([NotNull] DoWhileStatementContext context)
        {
            var expr = VisitExpr(context.expr());

            var block = context.block().Invoke(VisitBlock);

            return new DoWhileStatementNode(expr, block, GetLocation(context));
        }

        public override ForStatementNode VisitForStatement([NotNull] ForStatementContext context)
        {
            var declaration = context.forDecStatement()?.inlineStatement().Invoke(VisitInlineStatement);
            var condition = context.forConExpression()?.expr().Invoke(VisitExpr);
            var increment = context.forIncStatement()?.inlineStatement().Invoke(VisitInlineStatement);
            var block = context.block().Invoke(VisitBlock);
            var statement = context.statement().Invoke(VisitStatement);
            var body = (Node)block ?? statement;
            return new ForStatementNode(declaration, condition, increment, body, GetLocation(context));
        }

        public override LoopStatementNode VisitLoopStatement([NotNull] LoopStatementContext context)
        {
            var block = context.block().Invoke(VisitBlock);
            var statement = context.statement().Invoke(VisitStatement);
            var body = (Node)block ?? statement;

            return new LoopStatementNode(body, GetLocation(context));
        }

        #region Class
        public override ClassNode VisitClassExpr([NotNull] ClassExprContext context)
        {
            var members = context.classMember().Select(VisitClassMember).ToList();
            var handle = context.genericHandle().Invoke(VisitGenericHandle);

            return new ClassNode(members, handle, GetLocation(context));
        }

        public override ClassMemberNode VisitClassMember([NotNull] ClassMemberContext context)
        {
            var name = context.Name().GetText();
            // Signify Field member
            var expr = context.expr().Invoke(VisitExpr);
            // Signify Method member
            var func = InnerVisitFunction(context.blockFunction(), context.lambdaFunction());
            var annotation = context.typeAnnotation().Invoke(typeEvaluator.VisitTypeAnnotation);
            return new ClassMemberNode(name, expr, func, annotation, GetLocation(context));
        }

        public override InstantiateObjectNode VisitInstantiateObject([NotNull] InstantiateObjectContext context)
        {
            var names = context.nestedName().Name().Select(x => x.GetText()).ToList();

            var arguments = context.arguments()?.expr()?.Select(VisitExpr).ToList();

            // If no arguments has been passed, set to default list instead of empty
            arguments ??= new List<ExprNode>();

            var handle = context.genericHandle().Invoke(VisitGenericHandle);

            return new InstantiateObjectNode(names, arguments, handle, GetLocation(context));
        }
        #endregion

        private TokenLocation GetLocation(ISyntaxTree node)
        {
            var interval = node.SourceInterval;
            if (interval.Length == 0)
            {
                return new TokenLocation(tokenStream.Get(0), tokenStream.Get(0));
            }
            var start = tokenStream.Get(interval.a);
            var end = tokenStream.Get(interval.b);
            return new TokenLocation(start, end);
        }
    }
}
