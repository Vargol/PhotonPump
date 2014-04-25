using System;

namespace SunflowSharp.Systems.Ui
{

    /**
     * Null implementation of a user interface. This is usefull to silence the
     * output.
     */
    public class SilentInterface : UserInterface
    {
        public void print(UI.Module m, UI.PrintLevel level, string s)
        {
        }

        public void taskStart(string s, int min, int max)
        {
        }

        public void taskUpdate(int current)
        {
        }

        public void taskStop()
        {
        }
    }
}