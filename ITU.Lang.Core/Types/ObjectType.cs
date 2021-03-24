using System.Collections.Generic;
using System.Linq;

namespace ITU.Lang.Core.Types
{
    public class ObjectType : Type
    {
        public IDictionary<string, Type> FieldsAndMembers = new Dictionary<string, Type>();

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
                return Name.Equals(t.Name) && FieldsAndMembers.Equals(t.FieldsAndMembers);
            return false;
        }

        public Type GetMember(string member)
        {
            if (!FieldsAndMembers.TryGetValue(member, out var type))
            {
                throw new TranspilationException($"Could not access member '{member}' on object");
            }

            return type;
        }

        // Hash codes are needed for successful lookup in dictionaries
        public override int GetHashCode()
        {
            const int seed = 97;
            const int modifier = 19;

            // We go unchecked in order to let the integer automatically overflow for better hashing chaos
            unchecked
            {
                return FieldsAndMembers.Aggregate(
                    (seed + Name.GetHashCode()) * modifier,
                    (cur, item) => ((cur * modifier) + item.Key.GetHashCode()) * modifier + item.Value.GetHashCode()
                );
            }
        }
    }
}
