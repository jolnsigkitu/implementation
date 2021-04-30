using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Translator.Nodes;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.TypeNodes
{
    public class FuncTypeNode : TypeNode
    {
        private IEnumerable<TypeNode> Exprs;
        private GenericHandleNode Handle;
        private TypeNode ReturnType;

        public FuncTypeNode(IEnumerable<TypeNode> exprs, TypeNode returnType, GenericHandleNode handle)
        {
            Exprs = exprs;
            ReturnType = returnType;
            Handle = handle;
        }


        public override IType EvalType(Environment env)
        {
            using var _ = env.Scopes.Use();

            if (Handle != null)
            {
                Handle.Bind(env);
            }

            var exprTypes = Exprs.Select(expr => expr.EvalType(env)).ToList();

            var returnType = ReturnType.EvalType(env);

            var result = new FunctionType
            {
                ReturnType = returnType,
                ParameterTypes = exprTypes,
            };

            if (Handle != null)
            {
                return new GenericWrapper(result, Handle.Names.ToList());
            }

            return result;
        }
    }
}
