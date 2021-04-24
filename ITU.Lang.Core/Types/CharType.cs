using ITU.Lang.Core.Translator;

namespace ITU.Lang.Core.Types
{
    public class CharType : Type
    {
        public string AsNativeName() => "char";

        public string AsTranslatedName() => "char";

        public bool Equals(Type other) => other is CharType || other is AnyType;

        public override int GetHashCode() => 7;

        public override string ToString() => "new CharType()";
    }
}
