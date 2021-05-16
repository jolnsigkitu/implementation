using Pipe.Lang.Core.Translator;

namespace Pipe.Lang.Core.Types
{
    public class CharType : IType
    {
        public string AsNativeName() => "char";

        public string AsTranslatedName() => "char";

        public bool Equals(IType other) => other is CharType || other is AnyType;

        public override int GetHashCode() => 7;

        public override string ToString() => "new CharType()";
    }
}
