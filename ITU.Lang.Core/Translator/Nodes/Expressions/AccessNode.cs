using System.Collections.Generic;
using System.Linq;
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

        protected override Type ValidateExpr(Environment env)
        {
            // We use a scope to temporarily assign generic identifiers to resolved types for digging deeper
            using var _ = env.Scopes.Use();

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

            if (Chain == null)
            {
                return typ;
            }

            return ValidateChain(env, typ);
        }

        private Type ValidateChain(Environment env, Type typ)
        {
            if (!(typ is ClassType ct))
            {
                throw new TranspilationException("Cannot access member on non-object", Location);
            }

            if (!env.Scopes.Types.HasBinding(ct.Name))
            {
                throw new TranspilationException($"Cannot access member on undefined type '{ct.Name}'", Location);
            }

            IBinding binding = env.Scopes.Types.GetBinding(ct.Name);

            foreach (var link in Chain.Chain)
            {
                if (!(binding.Type is ClassType) || binding.Members == null)
                {
                    throw new TranspilationException("Cannot access member on non-object");
                }

                binding = link.Access(env, binding, ct);

                if (binding.Type is ClassType classType)
                {
                    if (!env.Scopes.Types.HasBinding(classType.Name))
                    {
                        throw new TranspilationException($"Unknown type {classType.Name}.");
                    }

                    binding = env.Scopes.Types.GetBinding(classType.Name);
                }
            }

            return typ;
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
