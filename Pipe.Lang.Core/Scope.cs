using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Pipe.Lang.Core
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

        public TValue RequireBinding(string varName)
        {
            if (!HasBinding(varName))
            {
                throw new TranspilationException($"Undefined type '{varName}'.");
            }

            return GetBinding(varName);
        }

        public TValue GetBinding(string varName)
        {
            var scope = GetScope(varName);
            return scope == null ? default(TValue) : scope[varName];
        }

        private Dictionary<string, TValue> GetScope(string varName)
        {
            foreach (var layer in scopes)
            {
                if (layer.ContainsKey(varName))
                {
                    return layer;
                }
            }
            return null;
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

        public void Rebind(string key, TValue value)
        {
            var binding = GetScope(key);
            if (binding == null)
            {
                throw new ScopeException("Cannot assign to undeclared variable '" + key + "'");
            }
            binding[key] = value;
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

        public IDisposable UseScope()
        {
            return new DisposableScope(this);
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

        public class DisposableScope : IDisposable
        {
            private Scope<TValue> scopes;
            public DisposableScope(Scope<TValue> s)
            {
                this.scopes = s;
                s.Push();
            }
            public void Dispose()
            {
                scopes.Pop();
            }
        }
    }

    public class ScopeException : TranspilationException
    {
        public ScopeException(string message) : base(message) { }

        public ScopeException(string message, Exception innerException) : base(message, innerException) { }
    }
}
