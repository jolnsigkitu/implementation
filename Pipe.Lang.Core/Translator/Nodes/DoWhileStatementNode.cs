using Pipe.Lang.Core.Translator.Nodes.Expressions;
using Pipe.Lang.Core.Types;

namespace Pipe.Lang.Core.Translator.Nodes
{
    public class DoWhileStatementNode : StatementNode
    {
        private ExprNode Expr;
        private BlockNode Block;

        public DoWhileStatementNode(ExprNode expr, BlockNode block, TokenLocation location) : base(location)
        {
            Expr = expr;
            Block = block;
        }

        public override void Validate(Environment env)
        {
            using var _ = env.Scopes.Use();
            Expr.Validate(env);
            Expr.AssertType(new BooleanType());

            Block.Validate(env);
        }

        public override string ToString() => $"do {Block} while({Expr});";
    }
}
