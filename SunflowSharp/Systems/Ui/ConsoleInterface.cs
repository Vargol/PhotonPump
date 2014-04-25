using System;
using System.IO;

namespace SunflowSharp.Systems.Ui
{

    /**
     * Basic console implementation of a user interface.
     */
    public class ConsoleInterface : UserInterface
    {
        private int min;
        private int max;
        private float invP;
        private string task;
        private int lastP;

        public ConsoleInterface()
        {
        }

        public void print(UI.Module m, UI.PrintLevel level, string s)
        {
            using (StreamWriter writer = new StreamWriter(Console.OpenStandardError()))
                writer.WriteLine(UI.formatOutput(m, level, s));
        }

        public void taskStart(string s, int min, int max)
        {
            task = s;
            this.min = min;
            this.max = max;
            lastP = -1;
            invP = 100.0f / (max - min);
        }

        public void taskUpdate(int current)
        {
            int p = (min == max) ? 0 : (int)((current - min) * invP);
            if (p != lastP)
                using (StreamWriter writer = new StreamWriter(Console.OpenStandardError()))
                    writer.Write(task + " [" + (lastP = p) + "%]\r");
        }

        public void taskStop()
        {
            using (StreamWriter writer = new StreamWriter(Console.OpenStandardError()))
                writer.Write("                                                                      \r");
        }
    }
}