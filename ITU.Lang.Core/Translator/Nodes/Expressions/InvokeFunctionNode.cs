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
        public GenericHandleNode Handle { get; }
        public VariableBinding Binding { get; set; }

        public InvokeFunctionNode(string name, IEnumerable<ExprNode> exprs, GenericHandleNode handle, TokenLocation location) : base(location)
        {
            Name = name;
            Exprs = exprs;
            Handle = handle;
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
                if (Handle != null)
                {
                    var resolutions = Handle.Names
                        .Zip(generic.GenericIdentifiers)
                        .ToDictionary((pair) => pair.Second, (pair) =>
                        {
                            if (!env.Scopes.Types.HasBinding(pair.First))
                            {
                                throw new TranspilationException($"Cannot find type '{pair.First}'", Handle.Location);
                            }
                            return env.Scopes.Types.GetBinding(pair.First).Type;
                        });

                    func = generic.Specify(resolutions);
                }
                else
                {
                    var resolvedGenerics = generic.Resolve(exprTypes);

                    func = generic.Specify(resolvedGenerics);
                }

                paramTypes = func.ParameterTypes;
            }
            else if (Handle != null)
            {
                throw new TranspilationException($"Cannot specify generic arguments for non-generic function '{Name}'", Handle.Location);
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

        public override string ToString() => $"{Name}{Handle}({string.Join(", ", Exprs)})";
    }
}
