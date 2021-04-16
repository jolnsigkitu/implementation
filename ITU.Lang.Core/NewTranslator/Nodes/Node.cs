using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.Nodes
{
    public abstract class Node
    {
        public ParserRuleContext Context { get; private set; }

        public Interval Interval
        {
            get => Context.SourceInterval;
        }

        public Node(ParserRuleContext context)
        {
            Context = context;
        }
        public abstract void Validate(Scopes scopes);
        public abstract override string ToString();
    }
}
