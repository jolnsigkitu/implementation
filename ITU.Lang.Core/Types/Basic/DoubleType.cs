using ITU.Lang.Core.Translator;

namespace ITU.Lang.Core.Types
{
    public class DoubleType : IType
    {
        public string AsNativeName() => "double";

        public string AsTranslatedName() => "double";

        public bool Equals(IType other) => other is DoubleType || other is AnyType;

        public override int GetHashCode() => 37;

        public override string ToString() => "new DoubleType()";
    }
}
