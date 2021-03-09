namespace ITU.Lang.Core.Types
{
    public class VoidType : Type
    {
        public string AsNativeName()
        {
            return "void";
        }

        public string AsTranslatedName()
        {
            return "void";
        }

        public bool Equals(Type other)
        {
            return other is VoidType;
        }
    }
}
