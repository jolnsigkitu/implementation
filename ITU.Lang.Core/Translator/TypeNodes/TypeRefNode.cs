using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Translator.Nodes;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.TypeNodes
{
    public class TypeRefNode : TypeNode
    {
        private string Name;
        private GenericHandleNode Handle;

        public TypeRefNode(string name, GenericHandleNode handle)
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

            return SpecifyGenericTypeRef(env, typ);
        }

        private IType SpecifyGenericTypeRef(Environment env, IType typ)
        {
            if (!(typ is GenericWrapper wrapper))
            {
                throw new TranspilationException("Cannot specify generic types for non-generic type");
            }

            var bindings = Handle.Names.Select(name => env.Scopes.Types.RequireBinding(name));

            return wrapper.ResolveByPosition(bindings);
        }
    }
}
