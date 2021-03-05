using ITU.Lang.Core.Types;

namespace ITU.Lang.Core
{
    public class CSharpASTNode
    {
        public string TranslatedValue { get; set; }

        public Type Type { get; set; }

        public TokenLocation Location { get; set; }

        public bool IsConst = false;

        public void AssertType(Type v)
        {
            if (Type.Equals(v)) return;
            var msg = $"Expected type '{v}', got '{Type}'";

            if (Location != null)
                throw new TranspilationException(msg, Location);
            else
                throw new TranspilationException(msg);
        }
    }
}
