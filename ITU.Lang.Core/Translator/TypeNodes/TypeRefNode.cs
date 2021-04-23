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

        public override Type EvalType(Environment env)
        {
            if (!env.Scopes.Types.HasBinding(Name))
            {
                throw new TranspilationException($"Found undefined type '{Name}'");
            }

            var typ = env.Scopes.Types.GetBinding(Name).Type;

            if (Handle == null)
            {
                return typ;
            }

            return SpecifyGenericTypeRef(env, typ);
        }

        private Type SpecifyGenericTypeRef(Environment env, Type typ)
        {
            if (!(typ is IGenericType<Type> gt))
            {
                throw new TranspilationException("Cannot specify generic types for non-generic type");
            }

            var identifiers = gt.GenericIdentifiers;

            if (identifiers.Count != Handle.Names.Count())
            {
                throw new TranspilationException($"Tried to resolve generic '{Name}' with {identifiers.Count} identifiers, but was provided {Handle.Names.Count()}");
            }

            var names = Handle.ResolveHandle(identifiers, env);

            return gt.Specify(names);
        }
    }
}
