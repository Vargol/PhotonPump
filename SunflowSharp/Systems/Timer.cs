using System;

namespace SunflowSharp.Systems
{

    public class Timer
    {
        private long startTime, endTime;

        public Timer()
        {
            startTime = endTime = 0;
        }

        public void start()
        {
            startTime = endTime = NanoTime.Now;
        }

        public void end()
        {
            endTime = NanoTime.Now;
        }

        public long nanos()
        {
            return endTime - startTime;
        }

        public double seconds()
        {
            return (endTime - startTime) * 1e-9;
        }

        public static string tostring(long nanos)
        {
            Timer t = new Timer();
            t.endTime = nanos;
            return t.ToString();
        }

        public static string tostring(double seconds)
        {
            Timer t = new Timer();
            t.endTime = (long)(seconds * 1e9);
            return t.ToString();
        }

        public override string ToString()
        {
            long millis = nanos() / (1000 * 1000);
            if (millis < 10000)
				return string.Format("{0}ms", millis);
            long hours = millis / (60 * 60 * 1000);
            millis -= hours * 60 * 60 * 1000;
            long minutes = millis / (60 * 1000);
            millis -= minutes * 60 * 1000;
            long seconds = millis / 1000;
            millis -= seconds * 1000;
			return string.Format("{0}:{1:0#}:{2:0#}.{3:#}", hours, minutes, seconds, millis / 100);
        }
    }
}