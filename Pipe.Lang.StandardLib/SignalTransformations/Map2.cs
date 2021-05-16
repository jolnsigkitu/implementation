using System;

namespace Pipe.Lang.StandardLib
{
    public partial interface PushSignalSource<TOutput>
    {
        Map2Signal<TOutput, TResult> Map2<TResult>(Func<TOutput, TOutput, TResult> mapper);
    }

    public abstract partial class ChainablePushSignal<TInput, TOutput>
    {
        public Map2Signal<TOutput, TResult> Map2<TResult>(Func<TOutput, TOutput, TResult> mapper)
        {
            var sig = new Map2Signal<TOutput, TResult>(mapper);
            AddNext(sig);
            return sig;
        }
    }

    public class Map2Signal<TInput, TOutput> : ChainablePushSignal<TInput, TOutput>
    {
        private Func<TInput, TInput, TOutput> mapper;

        private TInput cache;

        internal Map2Signal(Func<TInput, TInput, TOutput> mapper)
        {
            this.mapper = mapper;
        }

        protected override bool ShouldPush(TInput value)
        {
            if (cache == null)
            {
                cache = value;
                return false;
            }

            return true;
        }

        protected override TOutput GetNextValue(TInput value) => mapper(cache, value);
    }
}
