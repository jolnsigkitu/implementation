using System;
using ITU.Lang.StandardLib;
using ITU.Lang.SampleProgram.Lib;
using Microsoft.AspNetCore.Components.Web;

class Engine
{
    public int Wpm = 0;
    public double Accuracy = 0;

    public DateTime StartTime;

    public ForwardablePushSignal<CompletedWord> WordSignal;
    public ForwardablePushSignal<AccuracyMetrics> StatsSignal;

    public Engine(ForwardablePushSignal<CompletedWord> wordSignal)
    {
        WordSignal = wordSignal;

        initStatsSignal();
        initWpmSignal();
        initAccuracySignal();
    }
    private void initStatsSignal()
    {
        StatsSignal = WordSignal
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
        }, new AccuracyMetrics());
    }

    private void initWpmSignal()
    {
        StatsSignal.ForEach(stats =>
        {
            double elapsedTime = (DateTime.Now - StartTime).TotalSeconds;
            // https://github.com/Miodec/monkeytype/blob/94d2c7ead9b488230bba7b07181e3a3dfcffd2d2/src/js/test/test-logic.js#L834
            Wpm = (int)Math.Round((stats.Correct * (60d / elapsedTime)) / 5d);
        });
    }


    private void initAccuracySignal()
    {
        StatsSignal
        .Map(stats => ((double)stats.Correct / (double)stats.Total) * 100)
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
