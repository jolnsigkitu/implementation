using ITU.Lang.Core.NewTranslator.Nodes.Expressions;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator
{
    public class VariableBinding
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool IsConst { get; set; }

        public ExprNode Expr { get; set; }
    }
}
