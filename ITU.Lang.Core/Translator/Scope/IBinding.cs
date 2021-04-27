using System.Collections.Generic;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator
{
    public interface IBinding
    {
        Type Type { get; set; }
        IDictionary<string, VariableBinding> Members { get; set; }
    }
}
