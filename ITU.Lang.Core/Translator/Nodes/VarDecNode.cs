using System.Collections.Generic;
using ITU.Lang.Core.Translator.Nodes.Expressions;
using ITU.Lang.Core.Translator.TypeNodes;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class VarDecNode : StatementNode
    {
        private readonly string Name;
        private readonly ExprNode Expr;
        private readonly TypeNode TypeAnnotation;
        private readonly bool IsConst;

        private IType DerivedType;

        public VarDecNode(string name, bool isConst, ExprNode expr, TypeNode typeAnnotation, TokenLocation loc) : base(loc)
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
                DerivedType = TypeAnnotation.EvalType(env);
                Expr.AssertType(DerivedType);
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
