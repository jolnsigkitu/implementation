using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.Nodes.Expressions
{
    public class UnaryPreOperatorNode : UnaryOperatorNode
    {
        public UnaryPreOperatorNode(
            string op,
            ExprNode expr,
            Type returnType,
            ParserRuleContext context
        ) : base(op, expr, returnType, context)
        { }

        public override string ToString() => $"{Operator}{Expr.ToString()}";
    }
}
