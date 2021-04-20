using Antlr4.Runtime;
using ITU.Lang.Core.Types;
using System.Collections.Generic;

namespace ITU.Lang.Core.NewTranslator.Nodes.Expressions
{
    public class AccessChainNode : ExprNode
    {
        public IList<ChainNode> Chain;

        public AccessChainNode(IList<ChainNode> chain, ParserRuleContext context) : base(context)
        {
            Chain = chain;
        }

        public override Type ValidateExpr(Environment env)
        {
            return null;
        }

        public override string ToString() => string.Join(".", Chain);
    }

    public struct ChainNode
    {
        public string Name;
        // TODO: Change to InvokeFunctionNode when implemented
        public ExprNode Function;
    }
}
