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
                if (!(typ is ClassType ct))
                {
                    throw new TranspilationException("Cannot access member on non-object", Location);
                }

                IBinding binding = env.Scopes.Types.GetBinding(ct.Name);

                foreach (var link in Chain.Chain)
                {
                    // function
                    if (link.Function != null)
                    {
                        var func = link.Function;
                        func.Validate(env);

                        typ = ((FunctionType)func.Type).ReturnType;

                        if (!(typ is ClassType returnClassType))
                        {
                            throw new TranspilationException("Cannot access member on non-object", Location);
                        }

                        if (env.Scopes.Types.HasBinding(returnClassType.Name))
                        {
                            throw new TranspilationException($"Unknown type {returnClassType.Name}.", Location);
                        }

                        binding = env.Scopes.Types.GetBinding(returnClassType.Name);
                    }
                    // name
                    else
                    {
                        if (binding.Members == null)
                        {
                            throw new TranspilationException("Cannot access member on non-object", Location);
                        }

                        if (!binding.Members.TryGetValue(link.Name, out var memberBinding))
                        {
                            throw new TranspilationException($"Cannot access undefined member {link.Name} on type {typ}", Location);
                        }

                        binding = memberBinding;
                        typ = binding.Type;
                    }
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
