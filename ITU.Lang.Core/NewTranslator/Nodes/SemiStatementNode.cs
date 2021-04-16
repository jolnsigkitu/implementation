using static ITU.Lang.Core.Grammar.LangParser;

namespace ITU.Lang.Core.NewTranslator.Nodes
{
    public class SemiStatementNode : StatementNode
    {
        private Node Statement;
        public SemiStatementNode(SemiStatementContext ctx, Node statement) : base(ctx)
        {
            Statement = statement;
        }

        public override void Validate(Scopes scopes) => Statement.Validate(scopes);

        public override string ToString() => Statement.ToString() + ";";
    }
}