using ITU.Lang.Core.Types;
using System.Collections.Generic;
using System.Linq;

namespace ITU.Lang.Core.Translator.Nodes.Expressions
{
    public class AccessChainNode : ExprNode
    {
        public IList<AccessChainLink> Chain;

        public AccessChainNode(IList<AccessChainLink> chain, TokenLocation location) : base(location)
        {
            Chain = chain;
        }

        protected override Type ValidateExpr(Environment env)
        {
            return null;
        }

        public override string ToString() => string.Join(".", Chain);
    }

    public interface AccessChainLink
    {
        IBinding Access(Environment env, IBinding binding, ClassType classType);
    }

    public class NameAccessChainLink : AccessChainLink
    {
        public string Name;
        public NameAccessChainLink(string name) => Name = name;
        public IBinding Access(Environment env, IBinding binding, ClassType classType)
        {
            if (!binding.Members.TryGetValue(Name, out var memberBinding))
            {
                throw new TranspilationException($"Cannot access undefined member {Name} on type {classType}");
            }

            return memberBinding;
        }
    }

    public class FunctionAccessChainLink : AccessChainLink
    {
        public InvokeFunctionNode Function;
        public FunctionAccessChainLink(InvokeFunctionNode function) => Function = function;
        public IBinding Access(Environment env, IBinding binding, ClassType classType)
        {
            if (!binding.Members.TryGetValue(Function.Name, out var memberBinding))
            {
                throw new TranspilationException($"Cannot access undefined member {Function.Name} on type {classType}");
            }

            var memberType = memberBinding.Type;

            if (memberType is GenericFunctionType gft)
            {
                var exprTypes = Function.Exprs.Select(e =>
                {
                    e.Validate(env);
                    return e.Type;
                });

                System.Console.WriteLine($"Func exprs: {string.Join(", ", exprTypes)}");
                var resolutions = gft.Resolve(exprTypes);
                System.Console.WriteLine($"Resolutions ({resolutions.Count}): {string.Join(", ", resolutions)}");
                memberType = gft.Specify(resolutions);
            }

            if (classType is SpecificClassType sct)
            {
                memberType = sct.SpecifyMember(memberType);
            }

            if (!(memberType is FunctionType ft))
            {
                throw new TranspilationException($"Tried to invoke non-function member on class '{classType}'");
            }

            Function.Binding = new VariableBinding()
            {
                Name = memberBinding.Name,
                Type = ft,
            };
            Function.Validate(env);
            Function.Type = ft.ReturnType;

            return new VariableBinding()
            {
                Name = memberBinding.Name,
                Members = memberBinding.Members,
                IsConst = memberBinding.IsConst,
                Type = ft.ReturnType
            };
        }
    }
}
