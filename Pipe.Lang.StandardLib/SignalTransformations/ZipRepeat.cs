namespace Pipe.Lang.StandardLib
{
    public partial interface PushSignalSource<TOutput>
    {
        ZipRepeatSignal<TOutput, TOther> ZipRepeat<TOther>(PushSignalSource<TOther> otherSignal);
    }

    public abstract partial class ChainablePushSignal<TInput, TOutput>
    {
        public ZipRepeatSignal<TOutput, TOther> ZipRepeat<TOther>(PushSignalSource<TOther> otherSignal)
        {
            var sig = new ZipRepeatSignal<TOutput, TOther>(otherSignal);
            AddNext(sig);
            return sig;
        }
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
}
