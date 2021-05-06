using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes.Expressions
{
    public class TernaryExprNode : ExprNode
    {

        public ExprNode ConditionExpr { get; }
        public ExprNode TrueExpr { get; }
        public ExprNode FalseExpr { get; }
        public TernaryExprNode(ExprNode conditionExpr, ExprNode trueExpr, ExprNode falseExpr, TokenLocation location) : base(location)
        {
            ConditionExpr = conditionExpr;
            TrueExpr = trueExpr;
            FalseExpr = falseExpr;
            Location = location;
        }

        protected override Type ValidateExpr(Environment env)
        {
            ConditionExpr.Validate(env);
            ConditionExpr.AssertType(new BooleanType());

            TrueExpr.Validate(env);
            FalseExpr.Validate(env);

            FalseExpr.AssertType(TrueExpr.Type);

            return TrueExpr.Type;
        }

        public override string ToString() => $"{ConditionExpr} ? {TrueExpr} : {FalseExpr}";

    }
}
