using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.TypeNodes
{
    public abstract class TypeNode
    {
        public abstract IType EvalType(Environment env);
    }
}
