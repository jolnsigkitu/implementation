using ITU.Lang.Core.Types;
using System.Collections.Generic;
using System.Linq;
using static ITU.Lang.Core.Grammar.LangParser;

namespace ITU.Lang.Core.NewTranslator.Nodes
{
    public class ProgNode : Node
    {
        public List<StatementNode> Statements { get; private set; }

        public ProgNode(ProgContext ctx, List<StatementNode> statements) : base(ctx)
        {
            Statements = statements;
        }

        public override void Validate()
        {
            foreach (var statement in Statements)
            {
                statement.Validate();
            }
        }
        public override string ToString()
        {
            var namespaces = "using System;\nusing ITU.Lang.StandardLib;";
            var statementStrs = Statements.Select(s => s.ToString());

            return namespaces + "\n\n" + string.Join("\n", statementStrs);
        }
    }
}
