namespace ITU.Lang.Core.Types
{
    public class ObjectType : Type
    {
        // TODO:
        // public IDictionary<string, Type> FieldsAndMembers = new Dictionary<string, Type>();

        public string Name { get; init; }

        public string AsTranslatedName()
        {
            return Name;
        }

        public string AsNativeName()
        {
            return Name;
        }

        public bool Equals(Type other)
        {
            if (other is ObjectType t)
                return Name.Equals(t.Name);
            return false;
        }

        // Hash codes are needed for successful lookup in dictionaries
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
