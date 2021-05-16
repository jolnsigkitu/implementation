using ITU.Lang.Core.Translator;

namespace ITU.Lang.Core.Types
{
    public class GenericTypeIdentifier : IType
    {
        public string Identifier;

        public GenericTypeIdentifier(string identifier)
        {
            Identifier = identifier;
        }

        public string AsNativeName() => Identifier;

        public string AsTranslatedName() => Identifier;

        public bool Equals(IType other)
        {
            if (other is AnyType) return true;
            if (!(other is GenericTypeIdentifier id)) return false;

            return this.Identifier == id.Identifier;
        }

        public override int GetHashCode() => 17;

        public override string ToString() => $"new GenericTypeIdentifier(\"{Identifier}\")";
    }
}
