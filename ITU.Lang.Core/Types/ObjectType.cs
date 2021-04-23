using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Translator;

namespace ITU.Lang.Core.Types
{
    public class ObjectType : Type
    {
        public string Name { get; set; }

        public string AsTranslatedName() => Name;
        public string AsNativeName() => Name;

        public bool Equals(Type other)
        {
            if (other is ObjectType t)
            {
                return Name.Equals(t.Name);
            }
            return other is AnyType;
        }

        // Hash codes are needed for successful lookup in dictionaries
        public override int GetHashCode()
        {
            const int seed = 101;

            // We go unchecked in order to let the integer automatically overflow for better hashing chaos
            unchecked
            {
                return Name.GetHashCode() * seed;
            }
        }
    }
}
