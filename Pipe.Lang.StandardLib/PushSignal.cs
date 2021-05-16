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

    // Methods for the interface are defined in the files for the transformations
    public partial interface PushSignalSource<TOutput> { }

    public abstract partial class ChainablePushSignal<TInput, TOutput> : PushSignalSink<TInput>, PushSignalSource<TOutput>
    {
        protected IList<PushSignalSink<TOutput>> next = new List<PushSignalSink<TOutput>>();

        internal void AddNext(PushSignalSink<TOutput> node) => next.Add(node);

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
        internal ProducerSignal(Action<Action<TInput>> producer) => producer(Push);
        protected override TInput GetNextValue(TInput value) => value;
    }
}
