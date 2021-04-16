using Antlr4.Runtime;
using ITU.Lang.Core.Types;
using System.Collections.Generic;

namespace ITU.Lang.Core.NewTranslator.Nodes.Expressions
{
    public class AccessChainNode : ExprNode
    {
        public IList<ChainNode> Chain;

        public AccessChainNode(ChainNode node, ParserRuleContext context) : base(null, context)
        {
            Chain = new List<ChainNode>() { node };
        }

        public void Add(ChainNode node)
        {
            Chain.Add(node);
        }

        public override void Validate(Scopes scopes)
        {
        }

        public override string ToString() => string.Join(".", Chain);
    }

    struct ChainNode
    {
        public string Name;
        // TODO: Change to InvokeFunction
        public Node InvokeFunction;
    }
}
