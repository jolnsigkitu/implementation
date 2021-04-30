using System.Collections.Generic;
using System.Linq;
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
        public void Bind(Environment env)
        {
            foreach (var name in Names)
            {
                env.Scopes.Types.Bind(name, new GenericTypeIdentifier(name));
            }
        }

        public override void Validate(Environment env) { }

        /// <summary>
        /// Resolve the parameter list of identifiers to the types contained within this handle.
        /// </summary>
        /// <param name="identifiers">
        /// List of specific types which are resolved 1-to-1 with the types within this handle.
        /// </param>
        /// <param name="env">
        /// Environment which should be used to lookup the names in this handle.
        /// </param>
        /// <returns>A dictionary mapping from the generic identifiers to the resolved types.</returns>
        public IDictionary<string, IType> ResolveByIdentifier(IList<string> identifiers, Environment env)
        {
            var resolvedTypes = new Dictionary<string, IType>();
            foreach (var (n, id) in Names.Zip(identifiers))
            {
                if (!env.Scopes.Types.HasBinding(n))
                    throw new TranspilationException($"Undefined type '{n}'", Location);

                var typ = env.Scopes.Types.GetBinding(n);
                resolvedTypes.Add(id, typ);
            }
            return resolvedTypes;
        }

        public override string ToString() => $"<{string.Join(", ", Names)}>";
    }
}
