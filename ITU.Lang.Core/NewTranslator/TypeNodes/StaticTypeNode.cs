using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.TypeNodes
{
    public class StaticTypeNode : TypeNode
    {
        private Type Type;

        public StaticTypeNode(Type type)
        {
            Type = type;
        }

        public override Type EvalType(Environment env) => Type;
    }
}