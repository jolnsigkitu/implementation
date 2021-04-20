using ITU.Lang.Core.Translator.Nodes.Expressions;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class ForStatementNode : StatementNode
    {
        public Node Init { get; }
        public ExprNode Condition { get; }
        public Node Effect { get; }
        public Node Body { get; }

        public ForStatementNode(Node init, ExprNode condition, Node effect, Node body, TokenLocation location) : base(location)
        {
            Init = init;
            Condition = condition;
            Effect = effect;
            Body = body;
            Location = location;
        }

        public override void Validate(Environment env)
        {
            using var _ = env.Scopes.Use();
            Init.Validate(env);

            Condition.Validate(env);
            Condition.AssertType(new BooleanType());

            Effect.Validate(env);

            Body.Validate(env);
        }

        public override string ToString() => $"for({Init}; {Condition}; {Effect}) {Body}";
    }
}
