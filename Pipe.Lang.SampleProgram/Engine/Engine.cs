using System;
using Pipe.Lang.StandardLib;

using System.Collections.Generic;
using Pipe.Lang.SampleProgram.Lib;
using Pipe.Lang.SampleProgram.Util;
using Microsoft.AspNetCore.Components.Web;

class AccuracyMetrics  {
public int Total = 0;
public int Correct = 0;
public string ToString () => "(Total: " + Total + ", Correct: " + Correct + ")";
}
class Engine  {
public int Wpm = 0;
public double Accuracy = 0d;
public string ElapsedTime = "00:00:00";
public DateTime StartTime;
public PushSignalSource<CompletedWord> WordSignal;
public PushSignalSource<AccuracyMetrics> StatsSignal;
public PushSignalSource<TimeSpan> TimerSignal;
public Engine (PushSignalSource<CompletedWord> wordSignal) {
WordSignal = wordSignal;
}
public PushSignalSource<AccuracyMetrics> makeStatsSignal () {
return WordSignal.Reduce((AccuracyMetrics metrics, CompletedWord word) => {
IEnumerator<Segment> enumerator = word.Segments.GetEnumerator();
while(enumerator.MoveNext()) {
Segment segment = enumerator.Current;
int segLen = segment.Text.Length;
metrics.Total = metrics.Total + segLen;
if(!segment.Incorrect)
{
metrics.Correct = metrics.Correct + segLen;
}
}
return metrics;
}, new AccuracyMetrics());
}
public PushSignalSource<int> makeWpmSignal () {
return StatsSignal.ZipRepeat(TimerSignal).Map((Pair<AccuracyMetrics, TimeSpan> pair) => {
AccuracyMetrics stats = pair.First;
double elapsedTime = (DateTime.Now.Subtract(StartTime)).TotalSeconds;
return Numbers.Round((stats.Correct * (60d / elapsedTime)) / 5d);
});
}
public PushSignalSource<double> makeAccuracySignal () => StatsSignal.Map((AccuracyMetrics stats) => {
return (Numbers.ToDouble(stats.Correct) / Numbers.ToDouble(stats.Total)) * 100d;
});
public PushSignalSource<TimeSpan> makeTimerSignal () => Signal.Timer(500).Map((int i) => DateTime.Now.Subtract(StartTime).Duration());
public void Start (DateTime start, Action onUpdate) {
StartTime = start;
StatsSignal = makeStatsSignal();
TimerSignal = makeTimerSignal();
TimerSignal.ForEach((TimeSpan elapsedTime) => {
ElapsedTime = elapsedTime.ToString("hh\\:mm\\:ss");
onUpdate();
});
makeWpmSignal().ForEach((int wpm) => {
Wpm = wpm;
});
makeAccuracySignal().ForEach((double accuracy) => {
Accuracy = accuracy;
});
}
}