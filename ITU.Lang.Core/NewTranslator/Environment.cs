using ITU.Lang.Core.Operators;
using OperatorFactoryOperators = ITU.Lang.Core.Operators.Operators;


namespace ITU.Lang.Core.NewTranslator
{
    public class Environment
    {
        public Scopes Scopes { get; } = new Scopes();

        public OperatorFactory Operators = OperatorFactoryOperators.InitializeOperators(new OperatorFactory());
    }
}
