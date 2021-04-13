using System;

namespace ITU.Lang.StandardLib
{
    public class MainClass
    {
        public static void Main()
        {

            Console.WriteLine("Hello, world");

            ProducerSignal<int> timer = Signal.Timer(1000);
            timer.ForEach((item) => Console.WriteLine(item));
            Signal.Timer(2000).Map((i) => i * 2).ForEach((item) => Console.WriteLine(item));
            ChainablePushSignal<int, int> a = Signal.Timer(2000);
            ChainablePushSignal<int, int> b = a.Map((i) => i * 2);
            b.ForEach((item) => Console.WriteLine(item));


            for (; ; ) { }
        }
    }
}
