namespace ITU.Lang.Core.Types
{
    public class VoidType : IType
    {
        public string AsNativeName() => "void";

        public string AsTranslatedName() => "void";

        public bool Equals(IType other) => other is VoidType;

        public override int GetHashCode() => 2;

        public override string ToString() => "new VoidType()";
    }
}
