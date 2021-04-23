using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Translator;
using ITU.Lang.Core.Translator.Nodes;

namespace ITU.Lang.Core.Types
{
    public class ClassType : Type
    {
        public string Name { get; set; }

        public string AsNativeName() => Name;
        public string AsTranslatedName() => Name;

        public bool Equals(Type other)
        {
            if (other is AnyType) return true;
            if (other is ClassType t)
                return Name.Equals(t.Name);
            return false;
        }

        // Hash codes are needed for successful lookup in dictionaries
        public override int GetHashCode()
        {
            const int seed = 97;

            // We go unchecked in order to let the integer automatically overflow for better hashing chaos
            unchecked
            {
                return Name.GetHashCode() * seed;
            }
        }

        public override string ToString() => AsNativeName();
    }
}
