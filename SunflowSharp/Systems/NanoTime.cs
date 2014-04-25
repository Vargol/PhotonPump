using System;
using System.Diagnostics;

namespace SunflowSharp.Systems
{
    //Provided by IKVM
    public class NanoTime
    {
        public static long Now
        {
            get
            {
                long num = 1000000000L;
                double timestamp = Stopwatch.GetTimestamp();
                double frequency = Stopwatch.Frequency;
                return d2l((timestamp / frequency) * num);
            }
        }

        private static long d2l(double d)
        {
            if (d <= -9.2233720368547758E+18)
                return -9223372036854775808L;
            if (d >= 9.2233720368547758E+18)
                return 9223372036854775807L;
            if (double.IsNaN(d))
                return 0L;
            return (long)d;
        }
    }
}
