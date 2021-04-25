using System.Collections.Generic;
using ITU.Lang.Core.Operators;
using ITU.Lang.Core.Translator.Nodes;
using OperatorFactoryOperators = ITU.Lang.Core.Operators.Operators;

namespace ITU.Lang.Core.Translator
{
    public class Environment
    {
        public Scopes Scopes { get; } = new Scopes();

        public OperatorFactory Operators = OperatorFactoryOperators.InitializeOperators(new OperatorFactory());

        public IList<ClassNode> Classes = new List<ClassNode>();

        public override string ToString() => $"{{ Scopes: {Scopes}, Classes: {Classes} }}";
    }
}
