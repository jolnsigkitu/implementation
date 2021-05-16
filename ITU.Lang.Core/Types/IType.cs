using System;
using ITU.Lang.Core.Translator;

namespace ITU.Lang.Core.Types
{
    public interface IType : IEquatable<IType>
    {
        string AsTranslatedName();
        string AsNativeName();
    }
}
