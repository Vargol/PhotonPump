using System;
using SunflowSharp.Systems.Ui;

namespace SunflowSharp.Systems
{

    /**
     * This class provides a very simple framework for running a BenchmarkTest
     * kernel several times and time the results.
     */
    public class BenchmarkFramework
    {
        private Timer[] timers;
        private int timeLimit; // time limit in seconds

        public BenchmarkFramework(int iterations, int timeLimit)
        {
            this.timeLimit = timeLimit;
            timers = new Timer[iterations];
        }

        public void execute(BenchmarkTest test)
        {
            // clear previous results
            for (int i = 0; i < timers.Length; i++)
                timers[i] = null;
            // loop for the specified number of iterations or until the time limit
            long startTime = NanoTime.Now;
            for (int i = 0; i < timers.Length && ((NanoTime.Now - startTime) / 1000000000) < timeLimit; i++)
            {
                UI.printInfo(UI.Module.BENCH, "Running iteration %d", (i + 1));
                timers[i] = new Timer();
                test.kernelBegin();
                timers[i].start();
                test.kernelMain();
                timers[i].end();
                test.kernelEnd();
            }
            // report stats
            double avg = 0;
            double min = double.PositiveInfinity;
            double max = double.NegativeInfinity;
            int n = 0;
            foreach (Timer t in timers)
            {
                if (t == null)
                    break;
                double s = t.seconds();
                min = Math.Min(min, s);
                max = Math.Max(max, s);
                avg += s;
                n++;
            }
            if (n == 0)
                return;
            avg /= n;
            double stdDev = 0;
            foreach (Timer t in timers)
            {
                if (t == null)
                    break;
                double s = t.seconds();
                stdDev += (s - avg) * (s - avg);
            }
            stdDev = Math.Sqrt(stdDev / n);
            UI.printInfo(UI.Module.BENCH, "Benchmark results:");
            UI.printInfo(UI.Module.BENCH, "  * Iterations: %d", n);
            UI.printInfo(UI.Module.BENCH, "  * Average:    %s", Timer.tostring(avg));
            UI.printInfo(UI.Module.BENCH, "  * Fastest:    %s", Timer.tostring(min));
            UI.printInfo(UI.Module.BENCH, "  * Longest:    %s", Timer.tostring(max));
            UI.printInfo(UI.Module.BENCH, "  * Deviation:  %s", Timer.tostring(stdDev));
            for (int i = 0; i < timers.Length && timers[i] != null; i++)
                UI.printDetailed(UI.Module.BENCH, "  * Iteration %d: %s", i + 1, timers[i]);
        }
    }
}