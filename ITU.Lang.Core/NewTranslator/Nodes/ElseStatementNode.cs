using ITU.Lang.Core.NewTranslator.Nodes.Expressions;

namespace ITU.Lang.Core.NewTranslator.Nodes
{
    public class ElseStatementNode : StatementNode
    {

        public BlockNode Block { get; }
        public TokenLocation Loc { get; }

        public ElseStatementNode(BlockNode block, TokenLocation loc) : base(loc)
        {
            Block = block;
            Loc = loc;
        }

        public override void Validate(Environment env) => Block.Validate(env);

        public override string ToString() => $"\nelse {Block}";
    }
}
