using System;
using System.Collections.Generic;

namespace ITU.Lang.StandardLib
{
    public abstract class PushSignal<TInput>
    {
        internal abstract void push(TInput value);
    }

    public abstract class ChainablePushSignal<TInput, TOutput> : PushSignal<TInput>
    {
        protected IList<PushSignal<TOutput>> next = new List<PushSignal<TOutput>>();

        internal void addNext(PushSignal<TOutput> node)
        {
            next.Add(node);
        }

        public MapSignal<TOutput, TResult> map<TResult>(Func<TOutput, TResult> mapper)
        {
            var sig = new MapSignal<TOutput, TResult>(mapper);
            addNext(sig);
            return sig;
        }

        public ReduceSignal<TOutput, TResult> reduce<TResult>(Func<TResult, TOutput, TResult> reducer, TResult defaultResult = default(TResult))
        {
            var sig = new ReduceSignal<TOutput, TResult>(reducer, defaultResult);
            addNext(sig);
            return sig;
        }

        public FilterSignal<TOutput> filter(Predicate<TOutput> filter)
        {
            var sig = new FilterSignal<TOutput>(filter);
            addNext(sig);
            return sig;
        }

        // public PushSignal<TOther> pullFrom(PullSignal<TOther> otherSignal) {
        //     var sig = new PullFromSignal(otherSignal);
        //     addNext(sig);
        //     return sig;
        // }

        public ForEachSignal<TOutput> forEach(Action<TOutput> func)
        {
            var sig = new ForEachSignal<TOutput>(func);
            addNext(sig);
            return sig;
        }

        internal override void push(TInput value)
        {
            if (!shouldPush(value)) return;
            TOutput output = getNextValue(value);
            foreach (var node in next)
            {
                node.push(output);
            }
        }

        protected virtual bool shouldPush(TInput value)
        {
            return true;
        }

        protected abstract TOutput getNextValue(TInput value);
    }

    public class ProducerSignal<TInput> : ChainablePushSignal<TInput, TInput>
    {
        public ProducerSignal(Action<Action<TInput>> producer)
        {
            producer(push);
        }

        protected override TInput getNextValue(TInput value)
        {
            return value;
        }
    }

    public class MapSignal<TInput, TOutput> : ChainablePushSignal<TInput, TOutput>
    {
        private Func<TInput, TOutput> mapper;

        internal MapSignal(Func<TInput, TOutput> mapper)
        {
            this.mapper = mapper;
        }

        protected override TOutput getNextValue(TInput value)
        {
            return mapper(value);
        }
    }

    public class ReduceSignal<TInput, TOutput> : ChainablePushSignal<TInput, TOutput>
    {
        private Func<TOutput, TInput, TOutput> reducer;
        private TOutput state;

        internal ReduceSignal(Func<TOutput, TInput, TOutput> reducer, TOutput defaultValue)
        {
            this.reducer = reducer;
            this.state = defaultValue;
        }

        protected override TOutput getNextValue(TInput value)
        {
            state = reducer(state, value);

            return state;
        }
    }

    public class FilterSignal<TInput> : ChainablePushSignal<TInput, TInput>
    {
        private Predicate<TInput> predicate;

        internal FilterSignal(Predicate<TInput> predicate)
        {
            this.predicate = predicate;
        }

        protected override bool shouldPush(TInput value)
        {
            return predicate(value);
        }

        protected override TInput getNextValue(TInput value)
        {
            return value;
        }
    }

    public class ForEachSignal<TInput> : PushSignal<TInput>
    {
        private Action<TInput> function;

        internal ForEachSignal(Action<TInput> function)
        {
            this.function = function;
        }

        internal override void push(TInput value)
        {
            function(value);
        }
    }
}
