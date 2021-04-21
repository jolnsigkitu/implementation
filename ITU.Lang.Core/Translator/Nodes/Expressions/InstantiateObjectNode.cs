using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Translator.Nodes.Expressions;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class InstantiateObjectNode : ExprNode
    {
        public IList<string> Names { get; private set; }
        public IList<ExprNode> Exprs { get; }

        public InstantiateObjectNode(IList<string> names, IList<ExprNode> exprs, TokenLocation location) : base(location)
        {
            Names = names;
            Exprs = exprs;
        }

        protected override Type ValidateExpr(Environment env)
        {
            if (Names.Count > 1)
            {
                throw new System.NotImplementedException("Instantiation of nested objects is not supported yet.");
            }

            var name = Names[0];

            if (!env.Scopes.Types.HasBinding(name))
            {
                throw new TranspilationException($"Cannot instantiate object of undefined type '{name}'", Location);
            }

            var typeBinding = env.Scopes.Types.GetBinding(name);

            if (!(typeBinding.Type is ClassType classType))
            {
                throw new TranspilationException($"Cannot instantiate non-class value {name}", Location);
            }

            foreach (var expr in Exprs)
            {
                expr.Validate(env);
            }

            if (typeBinding.Members.TryGetValue("constructor", out var constructor))
            {
                var paramTypes = ((FunctionType)constructor.Type).ParameterTypes;

                if (Exprs.Count == 0 && paramTypes.Count != 0)
                {
                    throw new TranspilationException($"Class with constructor expects {paramTypes.Count} arguments, but got none.", Location);
                }

                var exprTypes = Exprs.Select(expr => expr.Type).ToList();

                if (!exprTypes.Equals(paramTypes))
                {
                    var exprCount = exprTypes.Count;
                    var paramCount = paramTypes.Count;

                    if (exprCount != paramCount)
                    {
                        throw new TranspilationException($"Function '{name}' takes {paramCount} parameters, but got {exprCount}", Location);
                    }

                    var i = 1;
                    foreach (var (expr, param) in exprTypes.Zip(paramTypes))
                    {
                        if (!expr.Equals(param))
                        {
                            throw new TranspilationException($"Function '{name}' could not be invoked: parameter {i} must be of type '{param.AsNativeName()}', but was '{expr.AsNativeName()}'", Location);
                        }
                        i++;
                    }
                }
            }

            return typeBinding.Type;
        }

        public override string ToString()
        {
            var name = Names[0];
            var exprs = string.Join(", ", Exprs);
            return $"new {name}({exprs})";
        }
    }
}
