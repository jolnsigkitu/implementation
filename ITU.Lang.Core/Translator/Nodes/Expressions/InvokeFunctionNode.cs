using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Translator.Nodes.Expressions;
using ITU.Lang.Core.Types;
using static ITU.Lang.Core.Grammar.LangParser;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class InvokeFunctionNode : ExprNode
    {
        public string Name { get; private set; }
        public IEnumerable<ExprNode> Exprs { get; }

        public InvokeFunctionNode(string name, IEnumerable<ExprNode> exprs, TokenLocation location) : base(location)
        {
            Name = name;
            Exprs = exprs;
        }

        public override Type ValidateExpr(Environment env)
        {
            if (!env.Scopes.Values.HasBinding(Name))
            {
                throw new TranspilationException($"Tried to invoke non-initialized invokable '{Name}'", Location);
            }

            var binding = env.Scopes.Values.GetBinding(Name);

            if (!(binding.Type is FunctionType ft))
            {
                throw new TranspilationException($"Cannot invoke non-invokable '{Name}'", Location);
            }

            var paramTypes = ft.ParameterTypes;

            if (paramTypes.Count() != Exprs.Count())
            {
                throw new TranspilationException($"Expected {paramTypes.Count()} parameter(s) when invoking function '{Name}', but got {Exprs.Count()}", Location);
            }

            foreach (var (expr, type) in Exprs.Zip(paramTypes))
            {
                expr.Validate(env);
                expr.AssertType(type);
            }

            // We re-assign the name such that it will be converted properly
            Name = binding.Name;

            return ft.ReturnType;
        }

        public override string ToString() => $"{Name}({string.Join(", ", Exprs)})";
    }
}
