using Pipe.Lang.Core.Types;
using System.Collections.Generic;
using System.Linq;

namespace Pipe.Lang.Core.Translator.Nodes.Expressions
{
    public class AccessChainNode : ExprNode
    {
        public IList<AccessChainLink> Chain;

        public AccessChainNode(IList<AccessChainLink> chain, TokenLocation location) : base(location)
        {
            Chain = chain;
        }

        protected override IType ValidateExpr(Environment env)
        {
            return null;
        }

        public override string ToString() => string.Join(".", Chain);
    }

    public interface AccessChainLink
    {
        IType Access(Environment env, IClassType binding);
    }

    public class NameAccessChainLink : AccessChainLink
    {
        public string Name;
        public NameAccessChainLink(string name) => Name = name;
        public IType Access(Environment env, IClassType binding)
        {
            if (!binding.TryGetMember(Name, out var memberBinding))
            {
                throw new TranspilationException($"Cannot access undefined member {Name} on class {binding.AsNativeName()}");
            }

            return memberBinding;
        }

        public override string ToString() => Name;
    }

    public class FunctionAccessChainLink : AccessChainLink
    {
        public InvokeFunctionNode Function;
        public FunctionAccessChainLink(InvokeFunctionNode function) => Function = function;
        public IType Access(Environment env, IClassType binding)
        {
            if (!binding.TryGetMember(Function.Name, out var memberBinding))
            {
                throw new TranspilationException($"Cannot access undefined member {Function.Name} on class {binding.AsNativeName()}");
            }

            if (!(memberBinding is IFunctionType ft))
            {
                throw new TranspilationException($"Tried to invoke non-function member on class {binding.AsNativeName()}.");
            }

            Function.Type = ft.ReturnType;
            Function.Binding = new VariableBinding()
            {
                Type = ft,
            };

            Function.Validate(env);

            return Function.Type;
        }

        public override string ToString() => Function.ToString();
    }
}
