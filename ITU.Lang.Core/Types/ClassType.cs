using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Translator;

namespace ITU.Lang.Core.Types
{
    public class ClassType : Type
    {
        public Dictionary<string, (Type, Node)> Members = new Dictionary<string, (Type, Node)>();

        public string Name { get; set; }

        public string AsNativeName() => Name;
        public string AsTranslatedName() => Name;

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

        public ObjectType ToObjectType()
        {
            var objMembers = this.Members
                .Select((entry) =>
                {
                    var typ = entry.Value.Item1;
                    if (typ is ClassType t)
                        typ = t.ToObjectType();
                    return (entry.Key, typ);
                })
                .ToDictionary(x => x.Item1, x => x.Item2);
            return new ObjectType()
            {
                Name = this.Name,
                Members = objMembers,
            };
        }
    }
}
