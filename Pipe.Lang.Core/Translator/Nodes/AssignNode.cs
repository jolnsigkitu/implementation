using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Pipe.Lang.Core.Types;

namespace Pipe.Lang.Core.Translator.Nodes.Expressions
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

        protected override IType ValidateExpr(Environment env)
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

            var type = binding.Type;

            if (Names.Count > 1)
            {
                foreach (var memberName in Names.GetRange(1, Names.Count - 1))
                {
                    if (!(type is IClassType ct))
                    {
                        throw new TranspilationException("Cannot assign value to nested property on non-object", Location);
                    }
                    if (!ct.Members.TryGetValue(memberName, out type))
                    {
                        throw new TranspilationException($"Cannot assign value to undefined member {memberName} on type {binding.Type}", Location);
                    }
                }
            }

            Expr.Validate(env);
            Expr.AssertType(type);

            return type;
        }

        public override string ToString()
        {
            return $"{string.Join(".", Names)} = {Expr}";
        }
    }
}
