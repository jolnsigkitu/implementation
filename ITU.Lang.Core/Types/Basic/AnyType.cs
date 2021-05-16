using ITU.Lang.Core.Translator;

namespace ITU.Lang.Core.Types
{
    public class AnyType : IType
    {
        public string AsNativeName() => "any";

        public string AsTranslatedName() => "dynamic";

        public bool Equals(IType other) => true;

        public override int GetHashCode() => 13;

        public override string ToString() => "new AnyType()";
    }
}
