using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.Nodes.Expressions
{
    public abstract class ExprNode : Node
    {
        public Type Type { get; private set; }
        protected ExprNode(Type type, ParserRuleContext context) : base(context)
        {
            Type = type;
        }

    }
}
