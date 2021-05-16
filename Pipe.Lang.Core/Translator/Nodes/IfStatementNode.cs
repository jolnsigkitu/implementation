using System.Collections.Generic;
using System.Linq;
using Pipe.Lang.Core.Translator.Nodes.Expressions;
using Pipe.Lang.Core.Types;

namespace Pipe.Lang.Core.Translator.Nodes
{
    public class IfStatementNode : StatementNode
    {
        public ExprNode Expr { get; }
        public BlockNode Block { get; }
        public IEnumerable<ElseIfStatementNode> ElseIfStatements { get; }
        public ElseStatementNode ElseStatement { get; }

        public IfStatementNode(ExprNode expr, BlockNode block, IEnumerable<ElseIfStatementNode> elseIfStatements, ElseStatementNode elseStatement, TokenLocation loc) : base(loc)
        {
            Expr = expr;
            Block = block;
            ElseIfStatements = elseIfStatements;
            ElseStatement = elseStatement;
        }

        public override void Validate(Environment env)
        {
            using (env.Scopes.Use())
            {
                Expr.Validate(env);
                Expr.AssertType(new BooleanType());

                Block.Validate(env);
            }

            foreach (var elseIfStatement in ElseIfStatements)
            {
                elseIfStatement.Validate(env);
            }

            ElseStatement?.Validate(env);
        }

        public override string ToString()
        {
            var elseIfsStr = string.Join("", ElseIfStatements);
            return $"if({Expr})\n{Block}{elseIfsStr}{ElseStatement}";
        }
    }
}
