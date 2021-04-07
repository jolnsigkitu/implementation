namespace ITU.Lang.Core.Types
{
    public class BooleanType : Type
    {
        public string AsNativeName()
        {
            return "boolean";
        }

        public string AsTranslatedName()
        {
            return "bool";
        }

        public bool Equals(Type other)
        {
            return (other is BooleanType);
        }

        public override int GetHashCode()
        {
            // We just choose a unique prime for our base types, probably shouldn't bring us many problems
            return 11;
        }
    }
}