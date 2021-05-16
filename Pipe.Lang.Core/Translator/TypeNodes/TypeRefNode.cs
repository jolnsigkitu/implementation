using System.Collections.Generic;
using System.Linq;
using Pipe.Lang.Core.Translator.Nodes;
using Pipe.Lang.Core.Types;

namespace Pipe.Lang.Core.Translator.TypeNodes
{
    public class TypeRefNode : TypeNode
    {
        private string Name;
        private GenericTypeHandleNode Handle;

        public TypeRefNode(string name, GenericTypeHandleNode handle)
        {
            Name = name;
            Handle = handle;
        }

        public override IType EvalType(Environment env)
        {
            if (!env.Scopes.Types.HasBinding(Name))
            {
                throw new TranspilationException($"Found undefined type '{Name}'");
            }

            var typ = env.Scopes.Types.GetBinding(Name);

            if (Handle == null)
            {
                return typ;
            }

            Handle.ParentType = typ;
            return Handle.EvalType(env);
        }
    }
}
