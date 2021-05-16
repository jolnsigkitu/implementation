using System;

namespace Pipe.Lang.StandardLib
{
    public partial interface PushSignalSource<TOutput>
    {
        MapSignal<TOutput, TResult> Map<TResult>(Func<TOutput, TResult> mapper);
    }

    public abstract partial class ChainablePushSignal<TInput, TOutput>
    {
        public MapSignal<TOutput, TResult> Map<TResult>(Func<TOutput, TResult> mapper)
        {
            var sig = new MapSignal<TOutput, TResult>(mapper);
            AddNext(sig);
            return sig;
        }
    }

    public class MapSignal<TInput, TOutput> : ChainablePushSignal<TInput, TOutput>
    {
        private Func<TInput, TOutput> mapper;

        internal MapSignal(Func<TInput, TOutput> mapper)
        {
            this.mapper = mapper;
        }

        protected override TOutput GetNextValue(TInput value) => mapper(value);
    }
}
