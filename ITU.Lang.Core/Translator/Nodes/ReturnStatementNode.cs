using ITU.Lang.Core.Translator.Nodes.Expressions;
using ITU.Lang.Core.Types;
using static ITU.Lang.Core.Grammar.LangParser;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class ReturnStatementNode : StatementNode
    {
        private ExprNode Expr;
        public Type ReturnType;
        public ReturnStatementNode(ExprNode expr, TokenLocation location) : base(location)
        {
            Expr = expr;
        }

        public override void Validate(Environment env)
        {
            Expr.Validate(env);
            ReturnType = Expr.Type;
        }

        public override string ToString() => $"return {Expr};";
    }
}
