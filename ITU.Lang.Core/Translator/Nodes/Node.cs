using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes
{
    public abstract class Node
    {
        public TokenLocation Location;

        public Node(TokenLocation location)
        {
            Location = location;
        }
        public abstract void Validate(Environment env);
        public abstract override string ToString();
    }
}
