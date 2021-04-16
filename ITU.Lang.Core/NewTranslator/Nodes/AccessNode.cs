using System;
using Antlr4.Runtime;

namespace ITU.Lang.Core.NewTranslator.Nodes.Expressions
{
    public class AccessNode : ExprNode
    {
        public string Name { get; }
        public ExprNode FirstExpr { get; }
        public AccessChainNode Chain { get; }
        // We don't care about the type of the underlying ExprNode, as we get and validate later
        public AccessNode(string name, ExprNode firstExpr, AccessChainNode chain, ParserRuleContext context) : base(firstExpr.Type, context)
        {
            Name = name;
            FirstExpr = firstExpr;
            Chain = chain;
        }

        public override void Validate(Scopes scopes)
        {
            ExprNode node = FirstExpr;
            if (Name != null)
            {
                var binding = scopes.Values.GetBinding(Name);
                node = binding.Expr;
            }

            if (Chain != null)
            {
                // TODO: Fix chain when we get members sorted
                throw new NotImplementedException("Access chain not implemented until members are fixed");
                // foreach (var link in Chain.Chain)
                // {

                //     if (link.Function != null)
                //     {
                //         var func = link.Function;
                //         func.Validate();
                //         node = func;
                //     }
                // }
            }
            // Type = node.Type;
            node.Validate(scopes);
        }

        public override string ToString() => $"{(Name ?? FirstExpr.ToString())}";
    }
}
