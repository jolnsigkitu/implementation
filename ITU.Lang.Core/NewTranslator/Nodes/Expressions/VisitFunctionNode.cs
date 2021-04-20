using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.Nodes.Expressions
{
    public class VisitFunctionNode : ExprNode
    {
        public VisitFunctionNode(TokenLocation location) : base(location)
        {
        }

        public override Type ValidateExpr(Environment env)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString() => throw new System.NotImplementedException();
    }
}
