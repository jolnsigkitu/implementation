using System.Collections.Generic;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator
{
    public class TypeBinding : IBinding
    {
        public Type Type { get; set; }
        public Dictionary<string, VariableBinding> Members { get; set; }

        public TypeBinding(Type type)
        {
            Type = type;
        }

        public TypeBinding()
        {

        }

        public override string ToString()
        {
            var membersStr = Members != null ? $"{{{string.Join(", ", Members.Keys)}}}" : "null";
            return $"{{Type: {Type?.ToString() ?? "null"}, Members: {membersStr}}}";
        }
    }
}
