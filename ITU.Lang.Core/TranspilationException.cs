using System;
using System.Runtime.Serialization;

namespace ITU.Lang.Core
{
    [Serializable]
    public class TranspilationException : Exception
    {
        public TranspilationException(string message) : base(message) { }

        public TranspilationException(string message, Exception innerException) : base(message, innerException) { }

        protected TranspilationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
