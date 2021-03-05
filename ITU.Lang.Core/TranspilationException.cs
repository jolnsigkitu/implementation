using System;
using System.Runtime.Serialization;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace ITU.Lang.Core
{
    public class TranspilationException : Exception
    {
        private static readonly string ERR_MSG = "An error occured during transpilation";
        public TranspilationException(string message) : base($"{ERR_MSG}:\n\t{message}") { }

        public TranspilationException(string message, TokenLocation loc) : this(message, loc, null) { }

        public TranspilationException(string message, Exception innerException) : base(message, innerException) { }
        public TranspilationException(string message, TokenLocation loc, Exception innerException) : base($"{ERR_MSG} at line {loc.StartLine}:{loc.StartColumn} - {loc.EndLine}:{loc.EndColumn}:\n{message}", innerException) { }
    }
}
