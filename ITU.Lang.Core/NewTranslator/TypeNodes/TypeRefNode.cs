using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.TypeNodes
{
    public class TypeRefNode : TypeNode
    {
        private string Name;
        private TypeNode Handle;

        public TypeRefNode(string name, TypeNode handle)
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

            var typ = env.Scopes.Types.GetBinding(Name);
            typ.Validate(env.Scopes.Types);
            return typ;
        }
    }
}
