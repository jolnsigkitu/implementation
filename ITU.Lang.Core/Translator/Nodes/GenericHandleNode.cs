using System.Collections.Generic;
using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class GenericHandleNode : Node
    {
        public IEnumerable<string> Names { get; }

        public GenericHandleNode(IEnumerable<string> names, TokenLocation location) : base(location)
        {
            Names = names;
        }
        public override void Validate(Environment env)
        {
            foreach (var name in Names)
            {
                env.Scopes.Types.Bind(name, new TypeBinding()
                {
                    Type = new GenericTypeIdentifier(name),
                });
            }
        }

        public override string ToString()
        {
            return $"<{string.Join(", ", Names)}>";
        }
    }
}
