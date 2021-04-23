using ITU.Lang.Core.Translator;

namespace ITU.Lang.Core.Types
{
    public class IntType : Type
    {
        public string AsNativeName() => "int";

        public string AsTranslatedName() => "int";

        public bool Equals(Type other) => other is IntType || other is AnyType;

        public override int GetHashCode() => 3;

        public override string ToString() => "int";
    }
}
