using ITU.Lang.Core.Translator.Nodes.Expressions;
using ITU.Lang.Core.Translator.TypeNodes;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class ClassMemberNode : Node
    {
        public string Name { get; }
        public ExprNode Expr { get; }
        public FunctionNode Func { get; }
        public TypeNode Annotation { get; }
        public VariableBinding Binding { get; } = new VariableBinding();

        public IType DerivedType { get; private set; }

        public ClassMemberNode(string name, ExprNode expr, FunctionNode func, TypeNode annotation, TokenLocation location) : base(location)
        {
            Name = name;
            Expr = expr;
            Func = func;
            Annotation = annotation;
            Binding.Name = Name;

            if (Func != null) Func.IsClassMember = true;
        }

        public override void Validate(Environment env)
        {
            IType memberType = null;
            if (Annotation != null)
            {
                memberType = Annotation.EvalType(env);
            }

            var node = Expr ?? Func;

            if (node != null)
            {
                node.Validate(env);

                if (memberType != null)
                {
                    node.AssertType(memberType);
                }
                else
                {
                    memberType = node.Type;
                }
            }

            // memberType is guaranteed not null from here
            DerivedType = memberType;
            Binding.Type = DerivedType;
        }

        public string ToString(string className)
        {
            if (Func != null)
            {
                var isConstructor = Name == "constructor";
                var actualName = isConstructor ? className : Name;
                var returnType = isConstructor ? "" : $" {((IFunctionType)Func.Type).ReturnType.AsTranslatedName()}";
                var suffix = Func.IsLambda ? ";" : "";
                return $"public{returnType} {actualName} {Func.Handle}{Func}{suffix}";
            }

            var prefix = $"public {DerivedType.AsTranslatedName()} {Name}";

            if (Expr != null)
            {
                return $"{prefix} = {Expr};";
            }

            return $"{prefix};";
        }

        public override string ToString()
        {
            throw new System.NotImplementedException("Use the alternative method where 'className' is passed down");
        }
    }
}
