using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes.Expressions
{
    public class VisitFunctionNode : ExprNode
    {
        public VisitFunctionNode(TokenLocation location) : base(location)
        {
        }

        protected override IType ValidateExpr(Environment env)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString() => throw new System.NotImplementedException();
    }
}
