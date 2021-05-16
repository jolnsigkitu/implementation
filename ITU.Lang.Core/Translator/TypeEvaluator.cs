using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

using System.Linq;

using ITU.Lang.Core.Types;
using ITU.Lang.Core.Grammar;
using static ITU.Lang.Core.Grammar.LangParser;
using ITU.Lang.Core.Translator.TypeNodes;

namespace ITU.Lang.Core.Translator
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
            return this.VisitFirstChild<TypeNode, TypeNode>(new ParserRuleContext[] {
                context.typeRef(),
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
            var handle = context.genericTypeHandle().Invoke(VisitGenericTypeHandle);
            return new TypeRefNode(name, handle);
        }

        public override GenericTypeHandleNode VisitGenericTypeHandle([NotNull] GenericTypeHandleContext context)
        {
            return new GenericTypeHandleNode(context.typeRef().Select(typeRef => VisitTypeRef(typeRef)));
        }

        public override FuncTypeNode VisitFuncTypeExpr([NotNull] FuncTypeExprContext context)
        {
            var handle = context.genericHandle().Invoke(translator.VisitGenericHandle);

            var exprs = context.funcTypeExprParamList().typeExpr().Select(VisitTypeExpr);

            var returnExpr = context.Void() != null
                ? new StaticTypeNode(new VoidType())
                : VisitTypeExpr(context.typeExpr());

            return new FuncTypeNode(exprs, returnExpr, handle);
        }
    }
}
