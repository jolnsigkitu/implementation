using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.Nodes.Expressions
{
    public class AccessNode : ExprNode
    {
        public ChainNode FirstPart { get; }
        public AccessChainNode Chain { get; }
        // We don't care about the type of the underlying ExprNode, as we get and validate later
        public AccessNode(ChainNode firstPart, AccessChainNode chain, ParserRuleContext context) : base(null, context)
        {
            FirstPart = firstPart;
            Chain = chain;
        }

        public override void Validate(Scopes scopes)
        {
            // if (Name != null)
            // {
            //     var binding = scopes.Values.GetBinding(Name);
            //     Type = binding.Type;
            // }
            if (FirstPart != null)
            {
                FirstPart.Validate(scopes);
                Type = FirstPart.Type;
            }
            if (Chain != null)
            {
                Chain.Type = Type;
                Chain.Validate(scopes);
            }
        }

        public override string ToString() => $"{Name ?? FirstPart.ToString()}.{Chain}";
    }
}
