using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ITU.Lang.Core
{
    public class Scope<TValue>
    {
        private Stack<Dictionary<string, TValue>> scopes = new Stack<Dictionary<string, TValue>>();

        public void Push()
        {
            scopes.Push(new Dictionary<string, TValue>());
        }

        public void Pop()
        {
            scopes.Pop();
        }

        public TValue GetBinding(string varName)
        {
            foreach (var layer in scopes)
            {
                if (layer.ContainsKey(varName))
                {
                    return layer[varName];
                }
            }
            return default(TValue);
        }

        public void Bind(string key, TValue value)
        {
            var scope = scopes.Peek();
            if (scope.ContainsKey(key))
            {
                throw new ScopeException("Cannot redeclare variable '" + key + "'");
            }
            scope[key] = value;
        }

        public bool HasBinding(string varName)
        {
            foreach (var layer in scopes)
            {
                if (layer.ContainsKey(varName))
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            var strBuilder = new StringBuilder();
            var indentation = "";

            foreach (var dict in scopes.Reverse())
            {
                foreach (var entry in dict)
                {
                    strBuilder.Append(indentation + entry.ToString() + ", \n");
                }
                indentation += "- ";
            }

            return strBuilder.ToString();
        }
    }

    public class ScopeException : TranspilationException
    {
        public ScopeException(string message) : base(message) { }

        public ScopeException(string message, Exception innerException) : base(message, innerException) { }
    }
}
