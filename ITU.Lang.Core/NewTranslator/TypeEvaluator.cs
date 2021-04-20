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

        public override TypeNode VisitTypeRef([NotNull] TypeRefContext context)
        {
            var name = context.Name().GetText();
            var handle = InvokeIf(context.genericHandle(), VisitGenericHandle);
            return new TypeRefNode(name, handle);
        }

        public override TypeNode VisitGenericHandle([NotNull] GenericHandleContext context)
        {
            var names = context.Name().Select(n => n.GetText());
            return new GenericHandleNode(names);
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
