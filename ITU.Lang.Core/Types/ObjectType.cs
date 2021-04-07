using System.Collections.Generic;
using System.Linq;

namespace ITU.Lang.Core.Types
{
    public class ObjectType : Type
    {
        public Dictionary<string, Type> Members { get; set; } = new Dictionary<string, Type>();

        public Type GetMember(string name)
        {
            return Members.GetValueOrDefault(name);
        }

        public string Name { get; set; }

        public string AsTranslatedName() => Name;
        public string AsNativeName() => Name;

        public bool Equals(Type other)
        {
            if (other is ObjectType t)
            {
                return Name.Equals(t.Name) && Members.Equals(t.Members);
            }
            return false;
        }

        // Hash codes are needed for successful lookup in dictionaries
        public override int GetHashCode()
        {
            const int seed = 97;
            const int modifier = 19;

            // We go unchecked in order to let the integer automatically overflow for better hashing chaos
            unchecked
            {
                return Members.Aggregate(
                    (seed + Name.GetHashCode()) * modifier,
                    (cur, item) => ((cur * modifier) + item.Key.GetHashCode()) * modifier + item.Value.GetHashCode()
                );
            }
        }
    }
}
