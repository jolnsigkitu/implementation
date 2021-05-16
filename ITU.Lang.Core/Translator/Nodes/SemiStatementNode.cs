using static ITU.Lang.Core.Grammar.LangParser;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class SemiStatementNode : StatementNode
    {
        private Node Statement;
        public SemiStatementNode(Node statement, TokenLocation loc) : base(loc)
        {
            Statement = statement;
        }

        public override void Validate(Environment env)
        {
            Statement.Validate(env);
        }

        public override string ToString()
        {
            var str = Statement.ToString();

            return string.IsNullOrEmpty(str) ? "" : $"{str};";
        }
    }
}
