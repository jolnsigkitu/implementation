using ITU.Lang.Core.Translator;

namespace ITU.Lang.Core.Types
{
    public class GenericTypeIdentifier : Type
    {
        public string Identifier;

        public GenericTypeIdentifier(string identifier)
        {
            Identifier = identifier;
        }

        public string AsNativeName() => Identifier;

        public string AsTranslatedName() => Identifier;

        public bool Equals(Type other) => other is AnyType || other is GenericTypeIdentifier a && a.Identifier == this.Identifier;

        public override int GetHashCode() => 17;

        public override string ToString() => $"new GenericTypeIdentifier(\"{Identifier}\")";
    }
}
