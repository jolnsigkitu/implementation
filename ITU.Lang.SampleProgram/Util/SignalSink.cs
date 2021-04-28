using System;
using ITU.Lang.StandardLib;

namespace ITU.Lang.SampleProgram.Util
{
    class SignalSink<TInput>
    {
        public ProducerSignal<TInput> Signal { get; private set; }
        private Action<TInput> Pusher;
        public SignalSink()
        {
            Signal = PushSignal<TInput>.Produce(push => Pusher = push);
        }

        public void Push(TInput val) => Pusher(val);
    }
}
