using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator
{
    public class VariableBinding : IBinding
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool IsConst { get; set; }

        public IDictionary<string, VariableBinding> Members { get; set; }

        public override string ToString()
        {
            var name = Name == null ? "null" : $"\"{Name}\"";
            var type = Type == null ? "null" : Type.ToString();
            var members = string.Join(", ", Members?.Select(member => $"{{ \"{member.Key}\", {member.Value} }}") ?? new string[0]);
            var memberDictStr = string.IsNullOrEmpty(members)
                ? ""
                : $", Members = new Dictionary<string, VariableBinding>() {{ {members} }}";
            return $"new VariableBinding() {{ Name = {name}, Type = {type}, IsConst = {(IsConst ? "true" : "false")}{memberDictStr}}}";
        }
    }
}
