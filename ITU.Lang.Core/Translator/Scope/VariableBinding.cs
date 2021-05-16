using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator
{
    public class VariableBinding
    {
        public string Name { get; set; }
        public IType Type { get; set; }
        public bool IsConst { get; set; }

        public override string ToString()
        {
            var name = Name == null ? "null" : $"\"{Name}\"";
            var type = Type == null ? "null" : Type.ToString();
            return $"new VariableBinding() {{ Name = {name}, Type = {type}, IsConst = {(IsConst ? "true" : "false")}}}";
        }
    }
}
