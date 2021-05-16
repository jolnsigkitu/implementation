using System;

namespace Pipe.Lang.StandardLib
{
    public partial interface PushSignalSource<TOutput>
    {
        ReduceSignal<TOutput, TResult> Reduce<TResult>(Func<TResult, TOutput, TResult> reducer, TResult defaultResult = default(TResult));
    }

    public abstract partial class ChainablePushSignal<TInput, TOutput>
    {
        public ReduceSignal<TOutput, TResult> Reduce<TResult>(Func<TResult, TOutput, TResult> reducer, TResult defaultResult = default(TResult))
        {
            var sig = new ReduceSignal<TOutput, TResult>(reducer, defaultResult);
            AddNext(sig);
            return sig;
        }
    }

    public class ReduceSignal<TInput, TOutput> : ChainablePushSignal<TInput, TOutput>
    {
        private Func<TOutput, TInput, TOutput> Reducer;
        private TOutput State;

        internal ReduceSignal(Func<TOutput, TInput, TOutput> reducer, TOutput defaultValue)
        {
            this.Reducer = reducer;
            this.State = defaultValue;
        }

        protected override TOutput GetNextValue(TInput value)
        {
            State = Reducer(State, value);

            return State;
        }
    }
}
