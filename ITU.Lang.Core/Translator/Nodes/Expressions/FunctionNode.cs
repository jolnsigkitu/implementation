using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using ITU.Lang.Core.Grammar;
using ITU.Lang.Core.Translator.TypeNodes;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes.Expressions
{
    public class FunctionNode : ExprNode
    {
        public ParameterListNode ParameterList { get; }
        public ExprNode Body { get; }
        public GenericHandleNode Handle { get; }
        public bool IsLambda { get; }
        public bool IsClassMember { get; set; }

        public FunctionNode(ParameterListNode parameterList, ExprNode body, GenericHandleNode handle, bool isLambda, TokenLocation location) : base(location)
        {
            ParameterList = parameterList;
            Body = body;
            Handle = handle;
            IsLambda = isLambda;
        }

        protected override Type ValidateExpr(Environment env)
        {
            using var _ = env.Scopes.Use();

            if (Handle != null)
            {
                Handle.Validate(env);
            }

            ParameterList.Validate(env);

            Body.Validate(env);

            var returnType = Body.Type;

            // If parameter list has an explicit return type annotation
            var annotatedReturnTypeNode = ParameterList.ReturnType;
            if (annotatedReturnTypeNode != null)
            {
                var annotatedReturnType = annotatedReturnTypeNode.EvalType(env);
                Body.AssertType(annotatedReturnType);
                returnType = annotatedReturnType;
            }

            var result = new FunctionType()
            {
                IsLambda = IsLambda,
                ReturnType = returnType,
                ParameterTypes = ParameterList.EvaluatedNamePairs.Select(x => x.Item2).ToList(),
                ParameterNames = ParameterList.EvaluatedNamePairs.Select(x => x.Item1).ToList(),
            };

            return Handle != null ? new GenericFunctionType(result, Handle.Names.ToList()) : result;
        }

        public override string ToString()
        {
            var parametersStr = string.Join(", ", ParameterList);
            var arrow = !IsClassMember || IsLambda ? " =>" : "";

            return $"{parametersStr}{arrow} {Body}";
        }
    }
}
