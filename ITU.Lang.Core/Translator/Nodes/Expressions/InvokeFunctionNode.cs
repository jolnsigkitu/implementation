using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Translator.Nodes.Expressions;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class InvokeFunctionNode : ExprNode
    {
        public string Name { get; protected set; }
        public IEnumerable<ExprNode> Exprs { get; protected set; }
        public GenericHandleNode Handle { get; }
        public VariableBinding Binding { get; set; }

        public InvokeFunctionNode(string name, IEnumerable<ExprNode> exprs, GenericHandleNode handle, TokenLocation location) : base(location)
        {
            Name = name;
            Exprs = exprs;
            Handle = handle;
        }

        protected virtual FunctionType EnsureValidBinding(Environment env)
        {
            if (Binding == null)
            {
                if (!env.Scopes.Values.HasBinding(Name))
                {
                    throw new TranspilationException($"Tried to invoke non-initialized invokable '{Name}'", Location);
                }

                Binding = env.Scopes.Values.GetBinding(Name);
            }

            if (!(Binding.Type is FunctionType ft))
            {
                throw new TranspilationException($"Cannot invoke non-invokable '{Name}'", Location);
            }

            return ft;
        }

        protected void AssertParameterCount(FunctionType ft)
        {
            var paramTypes = ft.ParameterTypes;

            if (paramTypes.Count() != Exprs.Count())
            {
                throw new TranspilationException($"Expected {paramTypes.Count()} parameter(s) when invoking function '{Name}', but got {Exprs.Count()}", Location);
            }
        }

        protected override IType ValidateExpr(Environment env)
        {
            if (Handle != null)
                throw new System.NotImplementedException("TODO: No generic resolve in my function invokation");
            var func = EnsureValidBinding(env);

            AssertParameterCount(func);

            foreach (var (expr, type) in Exprs.Zip(func.ParameterTypes))
            {
                expr.Validate(env);
                expr.AssertType(type);
            }

            // We re-assign the name such that it will be converted properly
            Name = Binding.Name;

            return func.ReturnType;
        }

        public override string ToString() => $"{Name}({string.Join(", ", Exprs)})";
    }
}
