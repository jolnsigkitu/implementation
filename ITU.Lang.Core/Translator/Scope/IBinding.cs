using System.Collections.Generic;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator
{
    interface IBinding
    {
        Type Type { get; set; }
        Dictionary<string, VariableBinding> Members { get; set; }
    }
}
