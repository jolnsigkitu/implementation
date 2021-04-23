using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Translator.Nodes.Expressions;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class InvokeFunctionNode : ExprNode
    {
        public string Name { get; private set; }
        public IEnumerable<ExprNode> Exprs { get; }

        public VariableBinding Binding { get; set; }

        public InvokeFunctionNode(string name, IEnumerable<ExprNode> exprs, TokenLocation location) : base(location)
        {
            Name = name;
            Exprs = exprs;
        }

        protected override Type ValidateExpr(Environment env)
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

            var paramTypes = ft.ParameterTypes;

            if (paramTypes.Count() != Exprs.Count())
            {
                throw new TranspilationException($"Expected {paramTypes.Count()} parameter(s) when invoking function '{Name}', but got {Exprs.Count()}", Location);
            }

            var func = ft;

            var exprTypes = Exprs.Select(e => e.Type);

            if (ft is GenericFunctionType generic)
            {
                var resolvedGenerics = generic.Resolve(exprTypes);

                func = generic.Specify(resolvedGenerics);
                paramTypes = func.ParameterTypes;
            }

            foreach (var (expr, type) in Exprs.Zip(paramTypes))
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
