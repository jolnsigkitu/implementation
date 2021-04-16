using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.Nodes.Expressions
{
    public class BinaryOperatorNode : ExprNode
    {
        public string Operator { get; private set; }
        private readonly ExprNode Expr1;
        private readonly ExprNode Expr2;
        public BinaryOperatorNode(string op, ExprNode expr1, ExprNode expr2, Type returnType, ParserRuleContext context) : base(returnType, context)
        {
            Expr1 = expr1;
            Expr2 = expr2;
            Operator = op;
        }

        public override void Validate()
        {
            Expr1.Validate();
            Expr2.Validate();
        }

        public override string ToString() => $"{Expr1} {Operator} {Expr2}";
    }
}
