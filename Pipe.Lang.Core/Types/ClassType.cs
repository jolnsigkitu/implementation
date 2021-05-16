using System.Collections.Generic;
using System.Linq;
using Pipe.Lang.Core.Translator;
using Pipe.Lang.Core.Translator.Nodes;

namespace Pipe.Lang.Core.Types
{
    public interface IClassType : IType
    {
        string Name { get; set; }
        IDictionary<string, IType> Members { get; set; }
        bool TryGetMember(string key, out IType member);
    }

    public class ClassType : IClassType
    {
        public string Name { get; set; }
        public IDictionary<string, IType> Members { get; set; } = new Dictionary<string, IType>();

        public virtual string AsNativeName() => Name;
        public virtual string AsTranslatedName() => Name;

        public bool Equals(IType other)
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

        public override string ToString()
        {
            var name = $"Name = \"{Name}\"";
            var memberItems = string.Join(", ", Members.Select(pair => $"{{ \"{pair.Key}\", {pair.Value} }}"));
            var members = $"Members = new Dictionary<string, IType>(){{ {memberItems} }}";
            return $"new ClassType() {{ {name}, {members} }}";
        }

        public bool TryGetMember(string key, out IType member) => Members.TryGetValue(key, out member);
    }
}
