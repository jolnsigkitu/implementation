using System;
using System.Timers;


namespace ITU.Lang.StandardLib
{
    public class Signal
    {
        public static PushSignal timer(double interval)
        {
            return new ProducerSignal((next) =>
            {
                var timer = new Timer(interval);
                timer.Elapsed += () => next();
                timer.Start();
            });
        }
    }
}
