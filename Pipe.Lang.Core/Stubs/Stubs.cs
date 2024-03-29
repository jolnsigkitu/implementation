namespace Pipe.Lang.Core
{
    public class Stubs
    {
        private string SignalStub = @"
        extern type PushSignalSink = <TInput> { };

        extern type PushSignalSource = <TOutput> {
            Map: <TResult>((TOutput) => TResult) => PushSignalSource<TResult>;
            Reduce: <TResult>((TResult, TOutput) => TResult, TResult) => PushSignalSource<TResult>;
            Filter: ((TOutput) => boolean) => PushSignalSource<TOutput>;
            ForEach: ((TOutput) => void) => PushSignalSink<TOutput>;
            ZipRepeat: <TOther>(PushSignalSource<TOther>) => PushSignalSource<Pair<TOutput, TOther>>;
        };

        extern type SignalLib = {
            Timer: (int) => PushSignalSource<int>;
        };
        
        extern const Signal = new SignalLib();";

        public override string ToString()
        {
            return SignalStub.Replace('\n', ' ').Replace("  ", "");
        }
    };
}
