using Pipe.Lang.Core.Types;

namespace Pipe.Lang.Core.Translator.TypeNodes
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
