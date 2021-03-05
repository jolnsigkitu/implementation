namespace ITU.Lang.Core.Types
{
    public class BooleanType : Type
    {
        public string AsTranslatedName()
        {
            return "bool";
        }

        public bool Equals(Type other)
        {
            return (other is BooleanType);
        }
    }
}
