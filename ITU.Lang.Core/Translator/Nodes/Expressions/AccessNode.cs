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

            IBinding binding = env.Scopes.Types.GetBinding(ct.Name);

            foreach (var link in Chain.Chain)
            {
                if (!(binding.Type is ClassType) || binding.Members == null)
                {
                    throw new TranspilationException("Cannot access member on non-object", Location);
                }

                var func = (InvokeFunctionNode)link?.Function;
                var bindingName = link.Function != null ? func.Name : link.Name;

                if (!binding.Members.TryGetValue(bindingName, out var memberBinding))
                {
                    throw new TranspilationException($"Cannot access undefined member {bindingName} on type {typ}", Location);
                }

                binding = memberBinding;
                typ = binding.Type;

                if (ct is SpecificClassType sct)
                {
                    typ = sct.SpecifyMember(typ);
                }

                if (func != null)
                {
                    if (!(typ is FunctionType ft))
                    {
                        throw new TranspilationException($"Tried to invoke non-function member on class '{ct}'", Location);
                    }
                    func.Binding = new VariableBinding()
                    {
                        Name = memberBinding.Name,
                        Type = ft,
                    };
                    func.Validate(env);
                    func.Type = ft.ReturnType;
                    typ = ft.ReturnType;
                }

                if (typ is ClassType classType)
                {
                    if (!env.Scopes.Types.HasBinding(classType.Name))
                    {
                        throw new TranspilationException($"Unknown type {classType.Name}.", Location);
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
