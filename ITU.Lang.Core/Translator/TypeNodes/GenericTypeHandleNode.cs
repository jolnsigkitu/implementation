using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Translator.Nodes;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.TypeNodes
{
    public class GenericTypeHandleNode : TypeNode
    {
        public IType ParentType;
        public IEnumerable<TypeRefNode> RefNodes;

        public GenericTypeHandleNode(IEnumerable<TypeRefNode> refNodes)
        {
            RefNodes = refNodes;
        }

        public override IType EvalType(Environment env)
        {
            if (!(ParentType is GenericWrapper wrapper))
            {
                throw new TranspilationException("Cannot specify generic types for non-generic type");
            }

            var bindings = RefNodes.Select(typeRef => typeRef.EvalType(env));

            return wrapper.ResolveByPosition(bindings);
        }
    }
}
