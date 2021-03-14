namespace ITU.Lang.Core.Types
{
    public class CharType : Type
    {
        public string AsNativeName()
        {
            return "char";
        }

        public string AsTranslatedName()
        {
            return "char";
        }

        public bool Equals(Type other)
        {
            return other is CharType;
        }

        public override int GetHashCode()
        {
            return 7;
        }
    }
}
