using Antlr4.Runtime;

namespace ITU.Lang.Core
{
    public class TokenLocation
    {
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }

        public TokenLocation(IToken start, IToken end)
        {
            StartLine = start.Line;
            StartColumn = start.Column;
            EndLine = end.Line;
            EndColumn = end.Column;
        }

        public override string ToString() => $"{StartLine}:{StartColumn} - {EndLine}:{EndColumn}:";
    }
}
