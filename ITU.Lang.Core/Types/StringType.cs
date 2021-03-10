namespace ITU.Lang.Core.Types
{
    public class StringType : Type
    {
        public string AsNativeName()
        {
            return "string";
        }

        public string AsTranslatedName()
        {
            return "string";
        }

        public bool Equals(Type other)
        {
            return other is StringType;
        }
    }
}
