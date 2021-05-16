using Pipe.Lang.Core.Types;

namespace Pipe.Lang.Core.Translator.TypeNodes
{
    public abstract class TypeNode
    {
        public abstract IType EvalType(Environment env);
    }
}
