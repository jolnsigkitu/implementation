using ITU.Lang.Core.Translator.Nodes.Expressions;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator
{
    public class VariableBinding
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool IsConst { get; set; }
    }
}
