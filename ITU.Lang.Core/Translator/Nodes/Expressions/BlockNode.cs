using System.Collections.Generic;
using ITU.Lang.Core.Grammar;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes.Expressions
{
    public class BlockNode : ExprNode
    {
        private IList<StatementNode> Statements;

        public BlockNode(IList<StatementNode> statements, TokenLocation location) : base(location)
        {
            Statements = statements;
        }

        protected override IType ValidateExpr(Environment env)
        {
            using var _ = env.Scopes.Use();

            IType foundReturnType = null;

            if (Statements != null)
            {
                foreach (var statement in Statements)
                {
                    if (foundReturnType != null)
                    {
                        throw new TranspilationException("Block cannot contain statements after a return statement.", Location);
                    }

                    statement.Validate(env);

                    if (statement is ReturnStatementNode r)
                    {
                        foundReturnType = r.ReturnType;
                    }
                }
            }

            return foundReturnType ?? new VoidType();
        }

        public override string ToString()
        {
            var content = "";
            if (Statements != null)
            {
                content = string.Join("\n", Statements);
            }
            return $"{{\n{content}\n}}";
        }
    }
}
