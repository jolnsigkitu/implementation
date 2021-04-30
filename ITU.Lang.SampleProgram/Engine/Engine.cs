using System;
using ITU.Lang.StandardLib;
using ITU.Lang.SampleProgram.Lib;
using Microsoft.AspNetCore.Components.Web;

class Engine
{
    public int Wpm = 0;
    public double Accuracy = 0;
    public string ElapsedTime = "00:00:00";

    public DateTime StartTime;

    public PushSignalSource<CompletedWord> WordSignal;
    public PushSignalSource<AccuracyMetrics> StatsSignal;
    public PushSignalSource<TimeSpan> TimerSignal;

    public Engine(PushSignalSource<CompletedWord> wordSignal)
    {
        WordSignal = wordSignal;
    }

    public void Start(DateTime start, Action onUpdate)
    {
        StartTime = start;

        StatsSignal = makeStatsSignal();
        TimerSignal = makeTimerSignal();

        TimerSignal.ForEach((elapsedTime) =>
        {
            ElapsedTime = elapsedTime.ToString("hh\\:mm\\:ss");
            onUpdate();
        });

        makeWpmSignal()
            .ForEach((wpm) => Wpm = wpm);

        makeAccuracySignal()
            .ForEach((accuracy) => Accuracy = accuracy); ;

    }

    private PushSignalSource<AccuracyMetrics> makeStatsSignal()
    {
        return WordSignal
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

    private PushSignalSource<int> makeWpmSignal()
    {
        return StatsSignal
        .ZipRepeat(TimerSignal)
        .Map(pair =>
        {
            var stats = pair.First;
            if (stats == null)
                return 0;

            double elapsedTime = (DateTime.Now - StartTime).TotalSeconds;
            // https://github.com/Miodec/monkeytype/blob/94d2c7ead9b488230bba7b07181e3a3dfcffd2d2/src/js/test/test-logic.js#L834
            return (int)Math.Round((stats.Correct * (60d / elapsedTime)) / 5d);
        });
    }

    private PushSignalSource<double> makeAccuracySignal()
    {
        return StatsSignal
            .Map(stats => ((double)stats.Correct / (double)stats.Total) * 100);
    }

    private PushSignalSource<TimeSpan> makeTimerSignal()
    {
        return Signal.Timer(500)
            .Map((i) => (DateTime.Now - StartTime).Duration());
    }
}

class AccuracyMetrics
{
    public int Total = 0;
    public int Correct = 0;

    public override string ToString() => $"(Total: {Total}, Correct: {Correct})";
}
