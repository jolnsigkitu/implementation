using ITU.Lang.Core.Translator;

namespace ITU.Lang.Core.Types
{
    public class StringType : IType
    {
        public string AsNativeName() => "string";

        public string AsTranslatedName() => "string";

        public bool Equals(IType other) => other is StringType || other is AnyType;

        public override int GetHashCode() => 5;

        public override string ToString() => "new StringType()";
    }
}
