namespace ITU.Lang.Core.Translator.Nodes
{
    public class LoopStatementNode : StatementNode
    {
        private Node Body;

        public LoopStatementNode(Node body, TokenLocation location) : base(location)
        {
            Body = body;
        }

        public override void Validate(Environment env)
        {
            Body.Validate(env);
        }

        public override string ToString()
        {
            return $"while(true){Body}";
        }

    }
}
