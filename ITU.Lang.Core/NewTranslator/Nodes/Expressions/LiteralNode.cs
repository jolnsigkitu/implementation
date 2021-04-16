using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.Nodes.Expressions
{
    public class LiteralNode : ExprNode
    {
        public string Value { get; private set; }
        public LiteralNode(string value, Type type, ParserRuleContext context) : base(type, context)
        {
            Value = value;
        }

        public override void Validate(Scopes scopes) { }

        public override string ToString() => Value;
    }
}
