using Antlr4.Runtime;
using Pipe.Lang.Core.Types;

namespace Pipe.Lang.Core.Translator.Nodes.Expressions
{
    public class BinaryOperatorNode : ExprNode
    {
        public string Operator { get; private set; }
        private readonly ExprNode Expr1;
        private readonly ExprNode Expr2;
        public BinaryOperatorNode(string op, ExprNode expr1, ExprNode expr2, TokenLocation location) : base(location)
        {
            Expr1 = expr1;
            Expr2 = expr2;
            Operator = op;
        }
        protected override IType ValidateExpr(Environment env)
        {
            Expr1.Validate(env);
            Expr2.Validate(env);

            return env.Operators.Binary.Get(Operator, Expr1.Type, Expr2.Type, Location);
        }

        public override string ToString() => $"{Expr1} {Operator} {Expr2}";
    }
}
