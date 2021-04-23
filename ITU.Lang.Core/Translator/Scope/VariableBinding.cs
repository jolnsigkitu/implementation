using System.Collections.Generic;
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
            return $"{{Name: {Name?.ToString() ?? "null"}, Type: {Type?.ToString() ?? "null"}, IsConst: {IsConst}, Members: {Members?.ToString() ?? "null"}}}";
        }
    }
}
