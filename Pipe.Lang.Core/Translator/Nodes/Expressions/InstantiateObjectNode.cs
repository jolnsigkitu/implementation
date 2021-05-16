using System.Collections.Generic;
using System.Linq;
using Pipe.Lang.Core.Translator.Nodes.Expressions;
using Pipe.Lang.Core.Types;

namespace Pipe.Lang.Core.Translator.Nodes
{
    public class InstantiateObjectNode : ExprNode
    {
        public IList<string> Names { get; private set; }
        public IList<ExprNode> Exprs { get; }
        public GenericHandleNode Handle { get; }

        public InstantiateObjectNode(IList<string> names, IList<ExprNode> exprs, GenericHandleNode handle, TokenLocation location) : base(location)
        {
            Names = names;
            Exprs = exprs;
            Handle = handle;
        }

        protected override IType ValidateExpr(Environment env)
        {
            if (Names.Count > 1)
            {
                throw new System.NotImplementedException("Instantiation of namespaced objects is not supported yet.");
            }

            var name = Names[0];

            if (!env.Scopes.Types.HasBinding(name))
            {
                throw new TranspilationException($"Cannot instantiate object of undefined type '{name}'", Location);
            }

            var type = env.Scopes.Types.GetBinding(name);

            if (!(type is IClassType classType))
            {
                throw new TranspilationException($"Cannot instantiate non-class value '{name}'", Location);
            }


            if (Handle != null)
            {
                if (!(type is GenericClassWrapper wrapper))
                {
                    throw new TranspilationException($"Cannot specify generic identifiers for non-generic class '{name}'", Location);
                }

                var resolutions = Handle.ResolveByIdentifier(wrapper.Handle, env);
                classType = wrapper.ResolveByIdentifier(resolutions);
            }

            var hasConstructor = classType.Members.TryGetValue("constructor", out var constructor);
            var constructorType = constructor;

            foreach (var expr in Exprs)
            {
                expr.Validate(env);
            }

            if (hasConstructor)
            {
                AssertExprsMatchesConstructor((FunctionType)constructorType);
            }

            return classType;
        }

        private void AssertExprsMatchesConstructor(FunctionType funcType)
        {
            var paramTypes = funcType.ParameterTypes;
            var exprTypes = Exprs.Select(expr => expr.Type).ToList();

            var exprCount = exprTypes.Count;
            var paramCount = paramTypes.Count;

            if (exprCount != paramCount)
            {
                throw new TranspilationException($"Class constructor takes {paramCount} parameters, but got {exprCount}", Location);
            }

            var i = 1;
            foreach (var (expr, param) in exprTypes.Zip(paramTypes))
            {
                if (!expr.Equals(param))
                {
                    throw new TranspilationException($"Class constructor could not be invoked: parameter {i} must be of type '{param.AsNativeName()}', but was '{expr.AsNativeName()}'", Location);
                }
                i++;
            }
        }

        public override string ToString()
        {
            var name = Names[0];
            var exprs = string.Join(", ", Exprs);
            return $"new {name}{Handle}({exprs})";
        }
    }
}
