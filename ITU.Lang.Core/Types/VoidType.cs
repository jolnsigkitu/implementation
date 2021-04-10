namespace ITU.Lang.Core.Types
{
    public class VoidType : Type
    {
        public string AsNativeName() => "void";

        public string AsTranslatedName() => "void";

        public bool Equals(Type other) => other is VoidType;

        public override int GetHashCode() => 2;

        public override string ToString() => "void";
    }
}
