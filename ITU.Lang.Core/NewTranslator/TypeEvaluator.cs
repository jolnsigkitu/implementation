using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

using System.Linq;

using ITU.Lang.Core.Types;
using ITU.Lang.Core.Grammar;
using static ITU.Lang.Core.Grammar.LangParser;
using ITU.Lang.Core.NewTranslator.TypeNodes;

namespace ITU.Lang.Core.NewTranslator
{
    class TypeEvaluator : LangBaseVisitor<TypeNode>
    {
        private readonly Translator translator;

        public TypeEvaluator(Translator translator)
        {
            this.translator = translator;
        }
        public override TypeNode VisitTypeExpr([NotNull] TypeExprContext context)
        {
            return VisitFirstChild<TypeNode>(new ParserRuleContext[] {
                context.typeRef(),
                context.classExpr(),
                context.funcTypeExpr(),
            });
        }

        public override TypeNode VisitTypeAnnotation([NotNull] TypeAnnotationContext context)
        {
            return VisitTypeExpr(context.typeExpr());
        }

        public override TypeRefNode VisitTypeRef([NotNull] TypeRefContext context)
        {
            var name = context.Name().GetText();
            var handle = InvokeIf(context.genericHandle(), translator.VisitGenericHandle);
            return new TypeRefNode(name, handle);
        }

        public override FuncTypeNode VisitFuncTypeExpr([NotNull] FuncTypeExprContext context)
        {
            var handle = InvokeIf(context.genericHandle(), translator.VisitGenericHandle);

            var exprs = context.funcTypeExprParamList().typeExpr().Select(VisitTypeExpr);

            var returnExpr = context.Void() != null
                ? new StaticTypeNode(new VoidType())
                : VisitTypeExpr(context.typeExpr());

            return new FuncTypeNode(exprs, returnExpr, handle);
        }

        private T VisitFirstChild<T>(ParserRuleContext[] children) where T : TypeNode
        {
            var child = children.FirstOrDefault(s => s != null);
            if (child == null)
            {
                return default(T);
            }
            return (T)Visit(child);
        }

        private R InvokeIf<T, R>(T value, System.Func<T, R> func) =>
            value != null ? func(value) : default(R);
    }
}
