using System;
using System.Collections.Generic;

namespace ITU.Lang.StandardLib
{
    public interface ITerminalPushSignal<TInput>
    {
        // TODO: Extract into internal interface
        void onNext(TInput item);
    }
    public interface IPushSignal<TInput> : ITerminalPushSignal<TInput>
    {
        PushSignal<TResult> map<TResult>(Func<TInput, TResult> mapper);

        PushSignal<TResult> reduce<TResult>(Func<TResult, TInput, TResult> reducer, TResult defaultAggregate);

        PushSignal<TInput> filter(Predicate<TInput> filter);

        ITerminalPushSignal<TInput> forEach(Action<TInput> func);
    }

    public abstract class PushSignal<TInput, TOutput> : IPushSignal<TInput>
    {
        private IList<IPushSignal<TOutput>> next = new List<IPushSignal<TOutput>>();
        public PushSignal<TResult> map<TResult>(Func<TInput, TResult> mapper)
        {
            var sig = new MapSignal(mapper);
            addNext(sig);
            return sig;
        }

        public PushSignal<TResult> reduce<TResult>(Func<TResult, TInput, TResult> reducer, TResult defaultResult = default(TResult))
        {
            var sig = new ReduceSignal(reducer, defaultResult);
            addNext(sig);
            return sig;
        }

        public PushSignal<TResult> filter(Predicate<TInput> filter)
        {
            var sig = new FilterSignal(filter);
            addNext(sig);
            return sig;
        }

        // public PushSignal<TOther> pullFrom(PullSignal<TOther> otherSignal) {
        //     var sig = new PullFromSignal(otherSignal);
        //     addNext(sig);
        //     return sig;
        // }

        public void forEach(Action<TInput> func)
        {
            addNext(new ForEachSignal(func));
        }

        private void addNext(PushSignal<TOutput, object> nextSignal)
        {
            next.Add(nextSignal);
        }
    }

    public class ProducerSignal<TInput, TOutput> : PushSignal<TInput, TOutput>
    {
        public ProducerSignal(Action<Func<TInput>> producer)
        {
            producer(onNext);
        }

        internal override void onNext(TInput item)
        {
            foreach (var sig in next)
            {
                sig.onNext(item);
            }
        }
    }

    public class MapSignal<TInput, TOutput> : PushSignal<TInput, TOutput>
    {
        private Func<TInput, TOutput> mapper;

        internal MapSignal(Func<TInput, TOutput> mapper)
        {
            this.mapper = mapper;
        }

        internal override void onNext(TInput item)
        {
            TOutput newVal = mapper(item);
            foreach (var sig in next)
            {
                sig.onNext(newVal);
            }
        }
    }

    public class ReduceSignal<TInput, TOutput> : PushSignal<TInput, TOutput>
    {
        private Func<TResult, TInput, TResult> reducer;
        private TOutput state;

        internal ReduceSignal(Func<TOutput, TInput, TOutput> reducer, TOutput defaultValue)
        {
            this.reducer = reducer;
            this.state = defaultValue;
        }

        internal override void onNext(TInput item)
        {
            state = reducer(item, state);
            foreach (var sig in next)
            {
                sig.onNext(state);
            }
        }
    }

    public class FilterSignal<TInput> : PushSignal<TInput, TInput>
    {
        private Predicate<TInput> predicate;

        internal MapSignal(Predicate<TInput> predicate)
        {
            this.predicate = predicate;
        }

        internal override void onNext(TInput item)
        {
            if (!predicate(item)) return;

            foreach (var sig in next)
            {
                sig.onNext(item);
            }
        }
    }

    public class ForEachSignal<TInput> : ITerminalPushSignal<TInput>
    {
        private Action<TInput> function;

        internal ForEachSignal(Action<TInput> function)
        {
            this.function = function;
        }

        internal void onNext(TInput item)
        {
            function(item);
        }
    }
}
