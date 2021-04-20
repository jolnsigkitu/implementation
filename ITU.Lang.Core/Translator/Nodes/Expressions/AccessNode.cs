using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes.Expressions
{
    public class AccessNode : ExprNode
    {
        public string Name { get; }
        public ExprNode FirstExpr { get; }
        public AccessChainNode Chain { get; }

        public bool HasParens { get; }
        // We don't care about the type of the underlying ExprNode, as we get and validate later
        public AccessNode(string name, ExprNode firstExpr, AccessChainNode chain, bool hasParens, TokenLocation location) : base(location)
        {
            Name = name;
            FirstExpr = firstExpr;
            Chain = chain;
            HasParens = hasParens;
        }

        public override Type ValidateExpr(Environment env)
        {
            FirstExpr?.Validate(env);
            Type typ = FirstExpr?.Type;

            if (Name != null)
            {
                if (!env.Scopes.Values.HasBinding(Name))
                {
                    throw new TranspilationException($"Cannot access undeclared value '{Name}'", Location);
                }
                var binding = env.Scopes.Values.GetBinding(Name);
                typ = binding.Type;
            }

            if (Chain != null)
            {
                // TODO: Fix chain when we get members sorted
                throw new System.NotImplementedException("Access chain not implemented until members are fixed");
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

            return typ;
        }

        public override string ToString()
        {
            var content = $"{(Name ?? FirstExpr.ToString())}";

            return HasParens ? $"({content})" : content;
        }
    }
}
