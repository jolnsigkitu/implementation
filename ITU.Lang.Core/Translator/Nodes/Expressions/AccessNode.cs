using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes.Expressions
{
    public class AccessNode : ExprNode
    {
        public string Name { get; }
        // We don't care about the type of the underlying ExprNode, as we get and validate later
        public ExprNode FirstExpr { get; }
        public AccessChainNode Chain { get; }
        public bool HasParens { get; }
        public AccessNode(string name, ExprNode firstExpr, AccessChainNode chain, bool hasParens, TokenLocation location) : base(location)
        {
            Name = name;
            FirstExpr = firstExpr;
            Chain = chain;
            HasParens = hasParens;
        }

        protected override IType ValidateExpr(Environment env)
        {
            // We use a scope to temporarily assign generic identifiers to resolved types for digging deeper
            using var _ = env.Scopes.Use();

            FirstExpr?.Validate(env);
            IType type = FirstExpr?.Type;

            if (Name != null)
            {
                if (!env.Scopes.Values.HasBinding(Name))
                {
                    throw new TranspilationException($"Cannot access undeclared value '{Name}'", Location);
                }
                var binding = env.Scopes.Values.GetBinding(Name);
                type = binding.Type;
            }

            if (Chain != null)
            {
                return ValidateChain(env, type);
            }

            return type;
        }

        private IType ValidateChain(Environment env, IType type)
        {
            foreach (var link in Chain.Chain)
            {
                if (!(type is IClassType classType))
                {
                    throw new TranspilationException("Cannot access member on non-object");
                }

                type = link.Access(env, classType);
            }

            return type;
        }

        public override string ToString()
        {
            var chainStr = Chain != null ? $".{Chain}" : "";
            var content = $"{(Name ?? FirstExpr.ToString())}";

            content = HasParens ? $"({content})" : content;

            return $"{content}{chainStr}";
        }
    }
}
