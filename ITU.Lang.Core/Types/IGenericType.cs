using System.Collections.Generic;

namespace ITU.Lang.Core.Types
{
    internal interface IGenericType<TSpecificType> where TSpecificType : Type
    {
        TSpecificType Specify(IDictionary<string, Type> specificTypes);
    }
}
