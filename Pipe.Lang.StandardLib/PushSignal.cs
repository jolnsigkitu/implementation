using System;
using System.Collections.Generic;

namespace Pipe.Lang.StandardLib
{
    public static class Signal
    {
        public static ProducerSignal<int> Timer(int millisecondInterval)
        {
            return PushSignal.Produce<int>((push) =>
            {
                var i = 0;
                var timer = new System.Threading.Timer((_) => push(i++), null, 0, millisecondInterval);
            });
        }
    }

    public static class PushSignal
    {
        public static ProducerSignal<TInput> Produce<TInput>(Action<Action<TInput>> producer) => new ProducerSignal<TInput>(producer);
    }

    public abstract class PushSignalSink<TInput>
    {
        internal abstract void Push(TInput value);
    }

    public class Pair<TFirst, TSecond>
    {
        public TFirst First;
        public TSecond Second;
    }

    public interface PushSignalSource<TOutput>
    {
        MapSignal<TOutput, TResult> Map<TResult>(Func<TOutput, TResult> mapper);
        Map2Signal<TOutput, TResult> Map2<TResult>(Func<TOutput, TOutput, TResult> mapper);
        ReduceSignal<TOutput, TResult> Reduce<TResult>(Func<TResult, TOutput, TResult> reducer, TResult defaultResult = default(TResult));
        FilterSignal<TOutput> Filter(Predicate<TOutput> filter);
        ForEachSignal<TOutput> ForEach(Action<TOutput> func);
        ZipRepeatSignal<TOutput, TOther> ZipRepeat<TOther>(PushSignalSource<TOther> otherSignal);
    }

    public abstract class ChainablePushSignal<TInput, TOutput> : PushSignalSink<TInput>, PushSignalSource<TOutput>
    {
        protected IList<PushSignalSink<TOutput>> next = new List<PushSignalSink<TOutput>>();

        internal void AddNext(PushSignalSink<TOutput> node) => next.Add(node);

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

        public Map2Signal<TOutput, TResult> Map2<TResult>(Func<TOutput, TOutput, TResult> mapper)
        {
            var sig = new Map2Signal<TOutput, TResult>(mapper);
            AddNext(sig);
            return sig;
        }

        public ZipRepeatSignal<TOutput, TOther> ZipRepeat<TOther>(PushSignalSource<TOther> otherSignal)
        {
            var sig = new ZipRepeatSignal<TOutput, TOther>(otherSignal);
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

    public class ZipRepeatSignal<TInput, TOther> : ChainablePushSignal<TInput, Pair<TInput, TOther>>
    {
        private PushSignalSource<TOther> OtherSignal;

        private TInput cacheInput;
        private TOther cacheOther;

        internal ZipRepeatSignal(PushSignalSource<TOther> otherSignal)
        {
            OtherSignal = otherSignal;

            otherSignal.ForEach(other =>
            {
                cacheOther = other;
                ActualPush();
            });
        }

        internal override void Push(TInput value)
        {
            cacheInput = value;
            ActualPush();
        }

        private void ActualPush()
        {
            if (cacheInput == null || cacheOther == null) return;
            var output = new Pair<TInput, TOther>()
            {
                First = cacheInput,
                Second = cacheOther,
            };
            foreach (var node in next)
            {
                node.Push(output);
            }
        }

        protected override Pair<TInput, TOther> GetNextValue(TInput value) => null;
    }

    public class ForEachSignal<TInput> : PushSignalSink<TInput>
    {
        private Action<TInput> Function;

        internal ForEachSignal(Action<TInput> function)
        {
            Function = function;
        }

        internal override void Push(TInput value) => Function(value);
    }
}
