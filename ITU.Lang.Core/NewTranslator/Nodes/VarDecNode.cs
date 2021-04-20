using ITU.Lang.Core.NewTranslator.Nodes.Expressions;
using ITU.Lang.Core.NewTranslator.TypeNodes;
using ITU.Lang.Core.Types;
using static ITU.Lang.Core.Grammar.LangParser;

namespace ITU.Lang.Core.NewTranslator.Nodes
{
    public class VarDecNode : StatementNode
    {
        private readonly string Name;
        private readonly ExprNode Expr;
        private readonly TypeNode TypeAnnotation;
        private readonly bool IsConst;

        private Type DerivedType;

        public VarDecNode(string name, bool isConst, ExprNode expr, TypeNode typeAnnotation, VardecContext ctx) : base(ctx)
        {
            Name = name;
            Expr = expr;
            TypeAnnotation = typeAnnotation;
            IsConst = isConst;
        }

        public override void Validate(Environment env)
        {
            Expr.Validate(env);

            DerivedType = Expr.Type;

            if (TypeAnnotation != null)
            {
                System.Console.WriteLine(Expr);
                System.Console.WriteLine($"TA: {TypeAnnotation}");
                var typ = TypeAnnotation.EvalType(env);
                Expr.AssertType(typ);
                DerivedType = typ;
            }

            env.Scopes.Values.Bind(Name, new VariableBinding()
            {
                Name = Name,
                Type = DerivedType,
                IsConst = IsConst,
            });
        }

        public override string ToString() => $"{DerivedType.AsTranslatedName()} {Name} = {Expr}";
    }
}
