using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator
{
    public class TypeBinding : IBinding
    {
        public Type Type { get; set; }
        public IDictionary<string, VariableBinding> Members { get; set; }

        public TypeBinding(Type type)
        {
            Type = type;
        }

        public TypeBinding()
        {

        }

        // public override string ToString()
        // {
        //     var membersStr = Members != null ? $"{{{string.Join(", ", Members.Keys)}}}" : "null";
        //     return $"{{Type: {Type?.ToString() ?? "null"}, Members: {membersStr}}}";
        // }

        public override string ToString()
        {
            var members = string.Join(", ", Members?.Select(member => $"{{ \"{member.Key}\", {member.Value} }}") ?? new string[0]);
            var memberDictStr = string.IsNullOrEmpty(members)
                ? ""
                : $", Members = new Dictionary<string, VariableBinding>() {{ {members} }}";
            return $"new TypeBinding() {{ Type = {Type}{memberDictStr} }}";
        }
    }
}
