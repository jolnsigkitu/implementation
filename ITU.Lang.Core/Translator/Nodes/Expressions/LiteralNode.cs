using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes.Expressions
{
    public class LiteralNode : ExprNode
    {
        public string Value { get; private set; }
        public LiteralNode(string value, Type type, TokenLocation location) : base(location)
        {
            Value = value;
            Type = type;
        }

        public override Type ValidateExpr(Environment env) => Type;

        public override string ToString() => Value;
    }
}
