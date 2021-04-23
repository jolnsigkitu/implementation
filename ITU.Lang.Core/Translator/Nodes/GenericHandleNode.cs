using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Resolve the parameter list of identifiers to the types contained within this handle.
        /// </summary>
        /// <param name="identifiers">
        /// List of specific types which are resolved 1-to-1 with the typess within this handle.
        /// </param>
        /// <param name="env">
        /// Environment which should be used to lookup the names in this handle.
        /// </param>
        /// <returns>A dictionary mapping from the generic identifiers to the resolved types.</returns>
        public IDictionary<string, Type> ResolveHandle(IList<string> identifiers, Environment env)
        {
            var resolvedNames = new Dictionary<string, Type>();
            foreach (var (n, id) in Names.Zip(identifiers))
            {
                if (!env.Scopes.Types.HasBinding(n))
                    throw new TranspilationException($"Undefined type '{n}'", Location);

                var binding = env.Scopes.Types.GetBinding(n);
                resolvedNames.Add(id, binding.Type);
            }
            return resolvedNames;
        }

        public override string ToString()
        {
            return $"<{string.Join(", ", Names)}>";
        }
    }
}
