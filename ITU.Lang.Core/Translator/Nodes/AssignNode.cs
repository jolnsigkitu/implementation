using System.Collections.Generic;
using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes.Expressions
{
    public class AssignNode : ExprNode
    {
        public IList<string> Names { get; }
        public ExprNode Expr { get; }

        public AssignNode(IList<string> names, ExprNode expr, TokenLocation location) : base(location)
        {
            Names = names;
            Expr = expr;
        }

        protected override Type ValidateExpr(Environment env)
        {
            var name = Names[0];

            if (Names.Count > 1)
            {
                // TODO: Fix when we get members sorted
                throw new System.NotImplementedException("Assignment to nested properties not implemented until members are fixed");
            }

            if (!env.Scopes.Values.HasBinding(name))
            {
                throw new TranspilationException($"Cannot access undeclared value '{name}'", Location);
            }
            var binding = env.Scopes.Values.GetBinding(name);

            var typ = binding.Type;

            Expr.Validate(env);
            Expr.AssertType(typ);

            return typ;
        }

        public override string ToString()
        {
            return $"{string.Join(".", Names)} = {Expr}";
        }
    }
}
