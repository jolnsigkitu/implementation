using System.Collections.Generic;

namespace ITU.Lang.Core.Types
{
    internal interface IGenericType<out TSpecificType> where TSpecificType : Type
    {
        TSpecificType Specify(IDictionary<string, Type> specificTypes);
        IList<string> GenericIdentifiers { get; set; }
    }
}
