using Pipe.Lang.Core.Translator;

namespace Pipe.Lang.Core.Types
{
    public class IntType : IType
    {
        public string AsNativeName() => "int";

        public string AsTranslatedName() => "int";

        public bool Equals(IType other) => other is IntType || other is AnyType;

        public override int GetHashCode() => 3;

        public override string ToString() => "new IntType()";
    }
}
