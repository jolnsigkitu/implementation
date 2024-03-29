using System;
using Pipe.Lang.StandardLib;

namespace Pipe.Lang.SampleProgram.Util
{
    public static class Numbers
    {
        public static double ToDouble(int val) => (double)val;
        public static int Round(double val) => (int)Math.Round(val);
    }
}
