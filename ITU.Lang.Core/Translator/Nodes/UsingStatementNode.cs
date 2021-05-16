using ITU.Lang.Core.Translator.Nodes.Expressions;
using ITU.Lang.Core.Types;
using static ITU.Lang.Core.Grammar.LangParser;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class UsingStatementNode : StatementNode
    {
        public string Namespace;
        public UsingStatementNode(string @namespace, TokenLocation location) : base(location)
        {
            Namespace = @namespace;
        }

        public override void Validate(Environment env) { }

        public override string ToString() => $"using {Namespace};";
    }
}
