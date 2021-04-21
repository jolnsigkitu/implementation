using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes.Expressions
{
    public class AssignNode : ExprNode
    {
        public List<string> Names { get; }
        public ExprNode Expr { get; }

        public AssignNode(List<string> names, ExprNode expr, TokenLocation location) : base(location)
        {
            Names = names;
            Expr = expr;
        }

        protected override Type ValidateExpr(Environment env)
        {
            var name = Names[0];

            if (!env.Scopes.Values.HasBinding(name))
            {
                throw new TranspilationException($"Cannot access undeclared value '{name}'", Location);
            }

            var binding = env.Scopes.Values.GetBinding(name);

            if (binding.IsConst && Names.Count == 1)
            {
                throw new TranspilationException("Cannot reassign to const variable", Location);
            }

            if (Names.Count > 1)
            {
                foreach (var memberName in Names.GetRange(1, Names.Count - 1))
                {
                    if (binding.Members == null)
                    {
                        throw new TranspilationException("Cannot assign value to nested property on non-object", Location);
                    }
                    if (!binding.Members.TryGetValue(memberName, out var member))
                    {
                        throw new TranspilationException($"Cannot assign value to undefined member {memberName} on type {binding.Type}", Location);
                    }
                    binding = member;
                }
            }

            var typ = binding.Type;

            Expr.Validate(env);
            Expr.AssertType(typ);

            binding.Type = Expr.Type;

            return typ;
        }

        public override string ToString()
        {
            return $"{string.Join(".", Names)} = {Expr}";
        }
    }
}
