using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.Nodes.Expressions
{
    public class UnaryPostOperatorNode : UnaryOperatorNode
    {
        public UnaryPostOperatorNode(
            string op,
            ExprNode expr,
            Type returnType,
            ParserRuleContext context
        ) : base(op, expr, returnType, context)
        { }

        public override string ToString() => $"{Expr.ToString()}{Operator}";
    }
}
