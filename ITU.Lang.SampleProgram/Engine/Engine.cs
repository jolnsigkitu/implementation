using System;
using ITU.Lang.StandardLib;
using ITU.Lang.SampleProgram.Lib;
using Microsoft.AspNetCore.Components.Web;

class Engine
{
    public string Wpm { get; private set; } = "0";
    public float Accuracy { get; private set; } = 0;

    private ForwardablePushSignal<CompletedWord> Signal;

    public Engine(ForwardablePushSignal<CompletedWord> signal)
    {
        Signal = signal;

        initAccuracySignal();
    }

    private void initAccuracySignal()
    {
        Signal
        .Map(word =>
        {
            var metrics = new AccuracyMetrics();

            var enumerator = word.Segments.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var segment = enumerator.Current;
                var segLen = segment.Text.Length;
                metrics.Total += segLen;
                if (!segment.Incorrect)
                    metrics.Correct += segLen;
            }

            return metrics;
        })
        .Reduce((acc, stats) =>
        {
            acc.Total += stats.Total;
            acc.Correct += stats.Correct;
            return acc;
        }, new AccuracyMetrics())
        .Map(stats => ((float)stats.Correct / (float)stats.Total) * 100)
        // Assign each new accuracy property in order to reflect it in UI
        .ForEach((accuracy) => Accuracy = accuracy);
    }
}

class AccuracyMetrics
{
    public int Total = 0;
    public int Correct = 0;

    public override string ToString() => $"(Total: {Total}, Correct: {Correct})";
}
