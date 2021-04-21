using Antlr4.Runtime;
using ITU.Lang.Core.Types;
using System.Collections.Generic;

namespace ITU.Lang.Core.Translator.Nodes.Expressions
{
    public class AccessChainNode : ExprNode
    {
        public IList<ChainNode> Chain;

        public AccessChainNode(IList<ChainNode> chain, TokenLocation location) : base(location)
        {
            Chain = chain;
        }

        protected override Type ValidateExpr(Environment env)
        {
            return null;
        }

        public override string ToString() => string.Join(".", Chain);
    }

    public class ChainNode
    {
        public string Name;
        public InvokeFunctionNode Function;

        public override string ToString() => Name != null ? Name : Function.ToString();
    }
}
