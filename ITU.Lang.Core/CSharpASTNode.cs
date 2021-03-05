using System;

namespace ITU.Lang.Core
{
    public class CSharpASTNode
    {
        public string TranslatedValue { get; set; }
        public string TypeName { get; set; }

        public TokenLocation Location { get; set; }

        public void AssertType(string v)
        {
            if (TypeName == v) return;
            var msg = $"Expected type '{v}', got '{TypeName}'";

            if (Location != null)
                throw new TranspilationException(msg, Location);
            else
                throw new TranspilationException(msg);
        }
    }
}
