using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Translator;

namespace ITU.Lang.Core.Types
{
    public class ClassType : Type
    {
        public IDictionary<string, (Type, Node)> Members = new Dictionary<string, (Type, Node)>();

        public string Name { get; set; }

        public string AsNativeName() => Name;
        public string AsTranslatedName() => "";

        public bool Equals(Type other)
        {
            if (other is ClassType t)
                return Name.Equals(t.Name) && Members.Equals(t.Members);
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
