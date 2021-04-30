using System.Collections.Generic;

namespace ITU.Lang.Core.Types
{
    public interface IClassType : IType
    {
        string Name { get; set; }
        IDictionary<string, IType> Members { get; set; }
    }
}
