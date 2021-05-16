using Pipe.Lang.Core.Translator.Nodes.Expressions;
using Pipe.Lang.Core.Types;

namespace Pipe.Lang.Core.Translator.Nodes
{
    public class WhileStatementNode : StatementNode
    {
        private ExprNode Expr;
        private Node Body;

        public WhileStatementNode(ExprNode expr, BlockNode block, StatementNode statement, TokenLocation location) : base(location)
        {
            Expr = expr;
            Body = (Node)block ?? statement;
        }

        public override void Validate(Environment env)
        {
            using var _ = env.Scopes.Use();
            Expr.Validate(env);
            Expr.AssertType(new BooleanType());

            Body.Validate(env);
        }

        public override string ToString() => $"while({Expr}) {Body}";
    }
}
