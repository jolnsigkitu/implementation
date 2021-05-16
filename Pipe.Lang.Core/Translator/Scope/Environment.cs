using System.Collections.Generic;
using Pipe.Lang.Core.Operators;
using Pipe.Lang.Core.Translator.Nodes;
using OperatorFactoryOperators = Pipe.Lang.Core.Operators.Operators;

namespace Pipe.Lang.Core.Translator
{
    public class Environment
    {
        public Scopes Scopes { get; } = new Scopes();

        public OperatorFactory Operators = OperatorFactoryOperators.InitializeOperators(new OperatorFactory());

        public IList<ClassNode> Classes = new List<ClassNode>();

        public override string ToString() => $"{{ Scopes: {Scopes}, Classes: {Classes} }}";
    }
}
