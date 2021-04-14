namespace ITU.Lang.Core.Types
{
    public class AnyType : Type
    {
        public string AsNativeName() => "any";

        public string AsTranslatedName() => "dynamic";

        public bool Equals(Type other) => true;

        public override int GetHashCode() => 13;

        public override string ToString() => "any";
    }
}
