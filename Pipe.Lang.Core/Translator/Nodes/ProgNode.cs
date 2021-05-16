using Pipe.Lang.Core.Types;
using System.Collections.Generic;
using System.Linq;
using static Pipe.Lang.Core.Grammar.LangParser;

namespace Pipe.Lang.Core.Translator.Nodes
{
    public class ProgNode : Node
    {
        public IList<StatementNode> Statements { get; private set; }

        public IList<ClassNode> Classes { get; private set; }

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
            var namespaces = "using System;\nusing Pipe.Lang.StandardLib;";

            var statementStrs = Statements.Select(s => s.ToString()).Where(s => s != "");
            var statements = string.Join("\n", statementStrs);

            var classes = string.Join("\n", Classes);

            return $"{namespaces}\n\n{statements}\n\n{classes}";
        }
    }
}
