using ITU.Lang.Core.Types;
using System.Collections.Generic;
using System.Linq;
using static ITU.Lang.Core.Grammar.LangParser;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class ProgNode : Node
    {
        public IList<StatementNode> Statements { get; private set; }

        public IList<ClassType> Classes { get; private set; }

        public ProgNode(IList<StatementNode> statements, TokenLocation location) : base(location)
        {
            Statements = statements;
        }

        public override void Validate(Environment env)
        {
            foreach (var statement in Statements)
            {
                statement.Validate(env);
            }

            Classes = env.Classes;
        }
        public override string ToString()
        {
            var namespaces = "using System;\nusing ITU.Lang.StandardLib;";
            var statementStrs = Statements.Select(s => s.ToString());
            var classes = Classes.Select(c => c.ToFullClass());

            return $"{namespaces}\n\n{string.Join("\n", statementStrs)}\n{string.Join("\n", classes)}";
        }
    }
}
