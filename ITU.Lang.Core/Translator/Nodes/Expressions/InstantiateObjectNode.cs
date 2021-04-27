using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Translator.Nodes.Expressions;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes
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

        protected override Type ValidateExpr(Environment env)
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

            var typeBinding = env.Scopes.Types.GetBinding(name);
            var hasConstructor = typeBinding.Members.TryGetValue("constructor", out var constructor);
            var constructorType = hasConstructor ? constructor.Type : null;


            if (!(typeBinding.Type is ClassType classType))
            {
                throw new TranspilationException($"Cannot instantiate non-class value '{name}'", Location);
            }

            var typ = typeBinding.Type;

            if (Handle != null)
            {
                if (!(typ is GenericClassType ct))
                {
                    throw new TranspilationException($"Cannot specify generic identifiers for non-generic class '{name}'", Location);
                }

                var resolution = Handle.ResolveHandle(ct.GenericIdentifiers, env);

                var specClass = new SpecificClassType(ct, resolution);
                typ = specClass;
                if (hasConstructor)
                {
                    constructorType = specClass.SpecifyMember(constructor.Type);
                }
            }

            foreach (var expr in Exprs)
            {
                expr.Validate(env);
            }

            if (hasConstructor)
            {
                AssertExprsMatchesConstructor((FunctionType)constructorType);
            }

            return typ;
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
