using System;
using SunflowSharp.Systems.Ui;

namespace SunflowSharp.Systems
{

    /**
     * Static singleton interface to a UserInterface object. This is set to a text
     * console by default.
     */
    public class UI
    {
        private static UserInterface ui = new ConsoleInterface();
        private static bool canceled = false;
        private static int _verbosity = 3;
        private static object lockObj = new object();
        public enum Module
        {
            API, GEOM, HAIR, ACCEL, BCKT, IPR, LIGHT, GUI, SCENE, BENCH, TEX, IMG, DISP, QMC, SYS, USER, CAM,
        }

        public enum PrintLevel
        {
            ERROR, WARN, INFO, DETAIL
        }

        private UI()
        {
        }

        /**
         * Sets the active user interface implementation. Passing <code>null</code>
         * silences printing completely.
         * 
         * @param ui object to recieve all user interface calls
         */
        public static void set(UserInterface ui)
        {
            if (ui == null)
                ui = new SilentInterface();
            UI.ui = ui;
        }

        public static void verbosity(int verbosity)
        {
            UI._verbosity = verbosity;
        }

        public static string formatOutput(Module m, PrintLevel level, string s)
        {
            return string.Format("{0}  {1}: {2}", m, level.ToString().ToLower(), s);
        }

        //http://www.ibm.com/developerworks/java/library/j-praxis/pr46.html
        public static void printDetailed(Module m, string s, params object[] args)
        {
            lock (lockObj)
                if (_verbosity > 3)
                    ui.print(m, PrintLevel.DETAIL, string.Format(s, args));
        }

        public static void printInfo(Module m, string s, params object[] args)
        {
            lock (lockObj)
                if (_verbosity > 2)
                    ui.print(m, PrintLevel.INFO, string.Format(s, args));
        }

        public static void printWarning(Module m, string s, params object[] args)
        {
            lock (lockObj)
                if (_verbosity > 1)
                    ui.print(m, PrintLevel.WARN, string.Format(s, args));
        }

        public static void printError(Module m, string s, params object[] args)
        {
            lock (lockObj)
                if (_verbosity > 0)
                    ui.print(m, PrintLevel.ERROR, string.Format(s, args));
        }

        public static void taskStart(string s, int min, int max)
        {
            lock (lockObj)
                ui.taskStart(s, min, max);
        }

        public static void taskUpdate(int current)
        {
            lock (lockObj)
                ui.taskUpdate(current);
        }

        public static void taskStop()
        {
            lock (lockObj)
                ui.taskStop();
            // reset canceled status
            // this assume the parent application will deal with it immediately
            canceled = false;
        }

        /**
         * Cancel the currently active task. This forces the application to abort as
         * soon as possible.
         */
        public static void taskCancel()
        {
            lock (lockObj)
            {
                printInfo(UI.Module.GUI, "Abort requested by the user ...");
                canceled = true;
            }
        }

        /**
         * Check to see if the current task should be aborted.
         * 
         * @return <code>true</code> if the current task should be stopped,
         *         <code>false</code> otherwise
         */
        public static bool taskCanceled()
        {
            lock (lockObj)
            {
                if (canceled)
                    printInfo(UI.Module.GUI, "Abort request noticed by the current task");
                return canceled;
            }
        }
    }
}