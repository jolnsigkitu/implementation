using System;

namespace Pipe.Lang.StandardLib
{
    public partial interface PushSignalSource<TOutput>
    {
        ForEachSignal<TOutput> ForEach(Action<TOutput> func);
    }

    public abstract partial class ChainablePushSignal<TInput, TOutput>
    {
        public ForEachSignal<TOutput> ForEach(Action<TOutput> func)
        {
            var sig = new ForEachSignal<TOutput>(func);
            AddNext(sig);
            return sig;
        }
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
