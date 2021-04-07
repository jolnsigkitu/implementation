using System;
using System.Collections.Generic;

namespace ITU.Lang.StandardLib
{
    public static class Signal
    {
        public static PushSignal<int> Timer(int millisecondInterval)
        {
            return PushSignal<int>.Produce((push) =>
            {
                var i = 0;
                var timer = new System.Threading.Timer((_) => push(i++), null, 0, millisecondInterval);
            });
        }
    }

    public abstract class PushSignal<TInput>
    {
        internal abstract void Push(TInput value);
        public static ProducerSignal<TInput> Produce(Action<Action<TInput>> producer) => new ProducerSignal<TInput>(producer);
    }

    public abstract class ChainablePushSignal<TInput, TOutput> : PushSignal<TInput>
    {
        protected IList<PushSignal<TOutput>> next = new List<PushSignal<TOutput>>();

        internal void AddNext(PushSignal<TOutput> node) => next.Add(node);

        public MapSignal<TOutput, TResult> Map<TResult>(Func<TOutput, TResult> mapper)
        {
            var sig = new MapSignal<TOutput, TResult>(mapper);
            AddNext(sig);
            return sig;
        }

        public ReduceSignal<TOutput, TResult> Reduce<TResult>(Func<TResult, TOutput, TResult> reducer, TResult defaultResult = default(TResult))
        {
            var sig = new ReduceSignal<TOutput, TResult>(reducer, defaultResult);
            AddNext(sig);
            return sig;
        }

        public FilterSignal<TOutput> Filter(Predicate<TOutput> filter)
        {
            var sig = new FilterSignal<TOutput>(filter);
            AddNext(sig);
            return sig;
        }

        // public PushSignal<TOther> PullFrom(PullSignal<TOther> otherSignal) {
        //     var sig = new PullFromSignal(otherSignal);
        //     addNext(sig);
        //     return sig;
        // }

        public ForEachSignal<TOutput> ForEach(Action<TOutput> func)
        {
            var sig = new ForEachSignal<TOutput>(func);
            AddNext(sig);
            return sig;
        }

        internal override void Push(TInput value)
        {
            if (!ShouldPush(value)) return;
            TOutput output = GetNextValue(value);
            foreach (var node in next)
            {
                node.Push(output);
            }
        }

        protected virtual bool ShouldPush(TInput value) => true;

        protected abstract TOutput GetNextValue(TInput value);
    }

    public class ProducerSignal<TInput> : ChainablePushSignal<TInput, TInput>
    {
        internal ProducerSignal(Action<Action<TInput>> producer)
        {
            producer(Push);
        }

        protected override TInput GetNextValue(TInput value) => value;
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

    public class ForEachSignal<TInput> : PushSignal<TInput>
    {
        private Action<TInput> Function;

        internal ForEachSignal(Action<TInput> function)
        {
            Function = function;
        }

        internal override void Push(TInput value) => Function(value);
    }
}
