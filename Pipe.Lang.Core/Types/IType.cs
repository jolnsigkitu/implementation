using System;
using Pipe.Lang.Core.Translator;

namespace Pipe.Lang.Core.Types
{
    public interface IType : IEquatable<IType>
    {
        string AsTranslatedName();
        string AsNativeName();
    }
}
