using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Translator.Nodes.Expressions;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class InvokeGenericFunctionNode : InvokeFunctionNode
    {
        public GenericHandleNode Handle { get; }

        public InvokeGenericFunctionNode(InvokeFunctionNode func, GenericHandleNode handle) : base(func.Name, func.Exprs, func.Location)
        {
            Handle = handle;
        }

        protected override GenericFunctionType EnsureValidBinding(Environment env)
        {
            var func = base.EnsureValidBinding(env);

            if (!(func is GenericFunctionType generic))
                throw new TranspilationException($"Cannot specify generic arguments for non-generic function {Name}", Location);

            return generic;
        }

        protected override Type ValidateExpr(Environment env)
        {
            var func = EnsureValidBinding(env);

            AssertParameterCount(func);

            Type returnType = func.ReturnType;

            Exprs.ForEach(expr => expr.Validate(env));

            FunctionType sf;

            if (Handle == null)
            {
                var exprTypes = Exprs.Select(e => e.Type);
                var resolvedGenerics = func.Resolve(exprTypes);

                sf = func.Specify(resolvedGenerics);
                returnType = sf.ReturnType;
            }
            else
            {
                var resolutions = Handle.Names
                    .Zip(func.GenericIdentifiers)
                    .ToDictionary((pair) => pair.Second, (pair) =>
                    {
                        if (!env.Scopes.Types.HasBinding(pair.First))
                        {
                            throw new TranspilationException($"Cannot find type '{pair.First}'", Handle.Location);
                        }
                        return env.Scopes.Types.GetBinding(pair.First).Type;
                    });

                sf = func.Specify(resolutions);
                returnType = sf.ReturnType;
            }

            Exprs.Zip(sf.ParameterTypes)
                .ForEach(pair => pair.First.AssertType(pair.Second));

            // We re-assign the name such that it will be converted properly
            Name = Binding.Name;

            return returnType;
        }

        public override string ToString() => $"{Name}{Handle}({string.Join(", ", Exprs)})";
    }
}
