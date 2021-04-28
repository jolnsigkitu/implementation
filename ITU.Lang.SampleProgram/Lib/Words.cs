using System;
using System.Linq;
using System.Collections.Generic;

namespace ITU.Lang.SampleProgram.Lib
{
    public class Words
    {
        public List<Word> Future { get; private set; }
        public Word Current { get; private set; }
        public List<Word> Previous { get; private set; }

        public List<Word> Attempted
        {
            get
            {
                var lst = new List<Word>();
                lst.AddRange(Previous);
                lst.Add(Current);
                return lst;
            }
        }

        public Words()
        {
            Future = MakeFutureWords();
            Previous = new List<Word>();
            UpdateCurrent();
        }

        public Word NextWord()
        {
            var completed = Current;
            Previous.Add(completed);
            UpdateCurrent();
            if (Future.Count <= 6)
            {
                Future.AddRange(MakeFutureWords());
            }
            return completed;
        }

        private void UpdateCurrent()
        {
            // See if there is a method like shift
            Current = Future[0];
            Future.RemoveAt(0);
        }

        private List<Word> MakeFutureWords()
        {
            return WordDictionary.GetRandomWords(20).Select(c => new Word(c)).ToList();
        }
    }

    public class Word
    {
        public string FullContent { get; private set; }
        public IList<Segment> Segments { get; private set; } = new List<Segment>();

        public Word(string fullContent)
        {
            FullContent = fullContent;
            ComputeSegments("");
        }

        public void ComputeSegments(string value)
        {
            var newSegments = new List<Segment>();
            var buf = "";
            var isIncorrect = false;

            int i;
            for (i = 0; i < Math.Min(FullContent.Length, value.Length); i++)
            {
                var correctCh = FullContent[i];
                var actualCh = value[i];
                if (correctCh == actualCh && isIncorrect)
                {
                    newSegments.Add(new Segment()
                    {
                        Text = buf,
                        Incorrect = true,
                        Attempted = true,
                    });
                    buf = "";
                    isIncorrect = false;
                }
                else if (correctCh != actualCh && !isIncorrect)
                {
                    newSegments.Add(new Segment()
                    {
                        Text = buf,
                        Incorrect = false,
                        Attempted = true,
                    });
                    buf = "";
                    isIncorrect = true;
                }

                buf += correctCh;
            }

            if (!string.IsNullOrEmpty(buf))
            {
                newSegments.Add(new Segment()
                {
                    Text = buf,
                    Incorrect = isIncorrect,
                    Attempted = true,
                });
            }

            if (i < FullContent.Length)
            {
                newSegments.Add(new Segment()
                {
                    Text = FullContent.Substring(i),
                    Incorrect = false,
                    Attempted = false,
                });
            }

            if (i < value.Length)
            {
                newSegments.Add(new Segment()
                {
                    Text = value.Substring(i),
                    Incorrect = true,
                    Attempted = true,
                });
            }

            Segments = newSegments;
        }
    }

    public class Segment
    {
        public string Text { get; set; }
        public bool Incorrect { get; set; }
        public bool Attempted { get; set; }

        public override string ToString() => $"Text: {Text}, Incorrect: {Incorrect}, Attempted: {Attempted}";
    }
}
