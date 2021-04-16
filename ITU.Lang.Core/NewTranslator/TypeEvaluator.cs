using System.Linq;
using System.Collections.Generic;

using Antlr4.Runtime.Misc;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using ITU.Lang.Core.Types;
using ITU.Lang.Core.Grammar;
using ITU.Lang.Core.NewTranslator.Nodes;
using static ITU.Lang.Core.Grammar.LangParser;
using ITU.Lang.Core.NewTranslator.Nodes.Expressions;
using ITU.Lang.Core.Operators;

namespace ITU.Lang.Core.NewTranslator
{
    class TypeEvaluator : LangBaseVisitor<Type>
    {
    }
}
