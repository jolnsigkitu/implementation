using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Pipe.Lang.Core.Grammar;
using Pipe.Lang.Core.Translator.Nodes;

namespace Pipe.Lang.Core
{
    static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var el in source)
                action(el);
        }

        public static R Invoke<T, R>(this T value, Func<T, R> func)
        {
            return value != null ? func(value) : default(R);
        }
    }

    static class VisitorExtensions
    {
        public static T VisitFirstChild<T, P>(this LangBaseVisitor<P> vis, ParserRuleContext[] children) where T : P
        {
            var child = children.FirstOrDefault(s => s != null);

            if (child == null)
                return default(T);

            return (T)vis.Visit(child);
        }
    }
}
