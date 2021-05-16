using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Translator.Nodes.Expressions;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class InvokeFunctionNode : ExprNode
    {
        public string Name { get; protected set; }
        public IList<ExprNode> Exprs { get; protected set; }
        public GenericHandleNode Handle { get; }
        public VariableBinding Binding { get; set; }

        public InvokeFunctionNode(string name, IList<ExprNode> exprs, GenericHandleNode handle, TokenLocation location) : base(location)
        {
            Name = name;
            Exprs = exprs;
            Handle = handle;
        }

        protected override IType ValidateExpr(Environment env)
        {
            EnsureValidBinding(env);

            if (!(Binding.Type is IFunctionType func))
            {
                throw new TranspilationException($"Cannot invoke non-invokable '{Name}'", Location);
            }

            AssertParameterCount(func);

            Exprs.ForEach(expr => expr.Validate(env));

            if (func is GenericFunctionWrapper wrapper)
                func = SpecifyFunc(wrapper, env);
            else if (Handle != null)
                throw new TranspilationException($"Cannot specify generic types for non-generic function '{Name}'.", Location);

            foreach (var (expr, type) in Exprs.Zip(func.ParameterTypes))
            {
                expr.AssertType(type);
            }

            // We re-assign the name such that it will be converted properly
            Name = Binding?.Name ?? Name;

            return func.ReturnType;
        }

        private void EnsureValidBinding(Environment env)
        {
            if (Binding == null)
            {
                if (!env.Scopes.Values.HasBinding(Name))
                {
                    throw new TranspilationException($"Tried to invoke non-initialized invokable '{Name}'", Location);
                }

                Binding = env.Scopes.Values.GetBinding(Name);
            }
        }

        private IFunctionType SpecifyFunc(GenericFunctionWrapper wrapper, Environment env)
        {
            if (Handle == null)
            {
                var exprTypes = Exprs.Select(expr => expr.Type);
                return wrapper.ResolveByParameterPosition(exprTypes);
            }

            var resolutions = Handle.ResolveByIdentifier(wrapper.Handle, env);
            return wrapper.ResolveByIdentifier(resolutions);
        }

        private void AssertParameterCount(IFunctionType ft)
        {
            var paramTypes = ft.ParameterTypes;

            if (paramTypes.Count() != Exprs.Count())
            {
                throw new TranspilationException($"Expected {paramTypes.Count()} parameter(s) when invoking function '{Name}', but got {Exprs.Count()}", Location);
            }
        }

        public override string ToString()
        {
            return $"{Name}({string.Join(", ", Exprs)})";
        }
    }
}
