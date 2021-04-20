using ITU.Lang.Core.NewTranslator.Nodes.Expressions;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.Nodes
{
    public class ElseIfStatementNode : StatementNode
    {

        public ExprNode Expr { get; }
        public BlockNode Block { get; }
        public TokenLocation Loc { get; }

        public ElseIfStatementNode(ExprNode expr, BlockNode block, TokenLocation loc) : base(loc)
        {
            Expr = expr;
            Block = block;
            Loc = loc;
        }

        public override void Validate(Environment env)
        {
            using var _ = env.Scopes.Use();

            Expr.Validate(env);
            Expr.AssertType(new BooleanType());

            Block.Validate(env);
        }

        public override string ToString() => $"\nelse if ({Expr})\n{Block}";
    }
}
