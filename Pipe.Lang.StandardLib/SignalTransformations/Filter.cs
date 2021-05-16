using System;

namespace Pipe.Lang.StandardLib
{
    public abstract partial class ChainablePushSignal<TInput, TOutput>
    {
        public FilterSignal<TOutput> Filter(Predicate<TOutput> filter)
        {
            var sig = new FilterSignal<TOutput>(filter);
            AddNext(sig);
            return sig;
        }
    }

    public class FilterSignal<TInput> : ChainablePushSignal<TInput, TInput>
    {
        private Predicate<TInput> Predicate;

        internal FilterSignal(Predicate<TInput> predicate)
        {
            Predicate = predicate;
        }

        protected override bool ShouldPush(TInput value) => Predicate(value);

        protected override TInput GetNextValue(TInput value) => value;
    }
}
