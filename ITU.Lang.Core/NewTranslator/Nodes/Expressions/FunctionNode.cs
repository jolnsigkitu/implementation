using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using ITU.Lang.Core.Grammar;
using ITU.Lang.Core.NewTranslator.TypeNodes;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.Nodes.Expressions
{
    public class FunctionNode : ExprNode
    {
        public ParameterListNode ParameterList { get; }
        public ExprNode Body { get; }
        public GenericHandleNode Handle { get; }
        public bool IsLambda { get; }

        public FunctionNode(ParameterListNode parameterList, ExprNode body, GenericHandleNode handle, bool isLambda, TokenLocation location) : base(location)
        {
            ParameterList = parameterList;
            Body = body;
            Handle = handle;
            IsLambda = isLambda;
        }

        public override Type ValidateExpr(Environment env)
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
                ParameterTypes = ParameterList.EvaluatedNamePairs.Select(x => x.Item2),
                ParameterNames = ParameterList.EvaluatedNamePairs.Select(x => x.Item1),
            };

            return Handle != null ? new GenericFunctionType(result) : result;
        }

        public override string ToString()
        {
            var parametersStr = string.Join(", ", ParameterList);
            // TODO: Support class members
            // return $"{parametersStr} {(IsLambda ? "=> " : "")}{Body}";
            return $"{parametersStr} => {Body}";
        }
    }
}
