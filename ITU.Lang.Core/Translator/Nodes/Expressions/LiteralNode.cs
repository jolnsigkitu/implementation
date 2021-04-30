using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes.Expressions
{
    public class LiteralNode : ExprNode
    {
        public string Value { get; private set; }
        public LiteralNode(string value, IType type, TokenLocation location) : base(location)
        {
            Value = value;
            Type = type;
        }

        protected override IType ValidateExpr(Environment env) => Type;

        public override string ToString() => Value;
    }
}
