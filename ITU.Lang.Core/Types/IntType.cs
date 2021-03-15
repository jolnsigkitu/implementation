namespace ITU.Lang.Core.Types
{
    public class IntType : Type
    {
        public string AsNativeName()
        {
            return "int";
        }

        public string AsTranslatedName()
        {
            return "int";
        }

        public bool Equals(Type other)
        {
            return other is IntType;
        }

        public override int GetHashCode()
        {
            return 3;
        }
    }
}
