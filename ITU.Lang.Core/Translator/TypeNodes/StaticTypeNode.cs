using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.TypeNodes
{
    public class StaticTypeNode : TypeNode
    {
        private IType Type;

        public StaticTypeNode(IType type)
        {
            Type = type;
        }

        public override IType EvalType(Environment env) => Type;
    }
}
