using ITU.Lang.Core.NewTranslator.Nodes.Expressions;
using ITU.Lang.Core.Types;
using static ITU.Lang.Core.Grammar.LangParser;

namespace ITU.Lang.Core.NewTranslator.Nodes
{
    public class VarDecNode : StatementNode
    {
        private readonly string Name;
        private readonly ExprNode Expr;
        private readonly Type TypeAnnotation;
        private readonly bool IsConst;

        public VarDecNode(string name, bool isConst, ExprNode expr, Type typeAnnotation, VardecContext ctx) : base(ctx)
        {
            Name = name;
            Expr = expr;
            TypeAnnotation = typeAnnotation;
            IsConst = isConst;
        }

        public override void Validate(Environment env)
        {
            TypeAnnotation?.Validate(env.Scopes.Types);

            Expr.Validate(env);

            if (TypeAnnotation != null) Expr.AssertType(TypeAnnotation);

            env.Scopes.Values.Bind(Name, new VariableBinding()
            {
                Name = Name,
                Type = TypeAnnotation ?? Expr.Type,
                IsConst = IsConst,
                Expr = Expr,
            });
        }

        public override string ToString() => $"{(TypeAnnotation ?? Expr.Type).AsTranslatedName()} {Name} = {Expr}";
    }
}
