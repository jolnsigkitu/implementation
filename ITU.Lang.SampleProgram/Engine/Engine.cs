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
            foreach (var segment in word.Segments)
            {
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
        .ForEach(stats =>
        {
            var accuracy = ((float)stats.Correct / (float)stats.Total) * 100;
            Accuracy = accuracy;
        });
    }
}

class AccuracyMetrics
{
    public int Total = 0;
    public int Correct = 0;

    public override string ToString()
    {
        return $"(Total: {Total}, Correct: {Correct})";
    }
}
