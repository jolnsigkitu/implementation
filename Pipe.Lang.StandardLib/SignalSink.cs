using System;

namespace Pipe.Lang.StandardLib
{
    public class SignalSink<TInput>
    {
        public ProducerSignal<TInput> Signal { get; private set; }
        private Action<TInput> Pusher;
        public SignalSink()
        {
            Signal = PushSignal.Produce<TInput>(push => Pusher = push);
        }

        public void Push(TInput val) => Pusher(val);
    }
}
