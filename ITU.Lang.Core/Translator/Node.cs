using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator
{
    public class Node
    {
        public string TranslatedValue { get; set; }

        public Type Type { get; set; } = new VoidType();

        public TokenLocation Location { get; set; }

        public bool IsConst = false;

        public bool IsType(Type v)
        {
            return Type.Equals(v);
        }

        public bool IsType(Node n)
        {
            return IsType(n.Type);
        }

        public void AssertType(Type v)
        {
            if (IsType(v)) return;
            var msg = $"Expected type '{(v.AsNativeName())}', got '{Type.AsNativeName()}'";

            if (Location != null)
                throw new TranspilationException(msg, Location);
            else
                throw new TranspilationException(msg);
        }

        public void AssertType(Node n)
        {
            AssertType(n.Type);
        }
    }
}
