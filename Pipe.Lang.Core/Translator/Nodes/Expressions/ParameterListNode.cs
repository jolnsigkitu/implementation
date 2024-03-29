using System.Collections.Generic;
using System.Linq;
using Pipe.Lang.Core.Translator.TypeNodes;
using Pipe.Lang.Core.Types;
using static Pipe.Lang.Core.Grammar.LangParser;

namespace Pipe.Lang.Core.Translator.Nodes
{
    public class ParameterListNode : Node
    {
        public IEnumerable<(string, TypeNode)> NameTypePairs;
        public TypeNode ReturnType;

        public readonly IList<(string, IType)> EvaluatedNamePairs = new List<(string, IType)>();
        public ParameterListNode(IEnumerable<(string, TypeNode)> nameTypePairs, TypeNode returnType, TokenLocation location) : base(location)
        {
            NameTypePairs = nameTypePairs;
            ReturnType = returnType;
        }

        public override void Validate(Environment env)
        {
            foreach (var (name, typeAnnotation) in NameTypePairs)
            {
                var type = typeAnnotation.EvalType(env);
                env.Scopes.Values.Bind(name, new VariableBinding()
                {
                    Name = name,
                    Type = type,
                    IsConst = false,
                });
                EvaluatedNamePairs.Add((name, type));
            }
        }

        public override string ToString()
        {
            var paramStrs = EvaluatedNamePairs.Select((tup) =>
            {
                var (name, typ) = tup;
                return $"{typ.AsTranslatedName()} {name}";
            });

            return $"({string.Join(", ", paramStrs)})";
        }
    }
}
