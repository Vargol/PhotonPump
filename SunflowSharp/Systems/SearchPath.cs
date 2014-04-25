using System;
using System.Collections.Generic;
using System.IO;
using SunflowSharp.Systems.Ui;

namespace SunflowSharp.Systems
{

    public class SearchPath
    {
        private LinkedList<string> searchPath;
        private string type;

        public SearchPath(string type)
        {
            this.type = type;
            searchPath = new LinkedList<string>();
        }

        public void resetSearchPath()
        {
            searchPath.Clear();
        }

        public void addSearchPath(string path)
        {
            //File f = new File(path);
            //if (f.exists() && f.isDirectory())
            if (File.Exists(path) || Directory.Exists(path))
            {
                try
                {
                    path = Path.GetFullPath(path);//f.getCanonicalPath();
                    foreach (string prefix in searchPath)
                        if (prefix == path)
                            return;
                    UI.printInfo(UI.Module.SYS, "Adding {0} search path: \"{1}\"", type, path);
                    searchPath.AddLast(path);
                }
                catch (Exception e)
                {
                    UI.printError(UI.Module.SYS, "Invalid {0} search path specification: \"{1}\" - {2}", type, path, e);
                }
            }
            else
                UI.printError(UI.Module.SYS, "Invalid {0} search path specification: \"{1}\" - invalid directory", type, path);
        }

        public string resolvePath(string filename)
        {
            // account for relative naming schemes from 3rd party softwares
            if (filename.StartsWith("//"))
                filename = filename.Substring(2);
            UI.printDetailed(UI.Module.SYS, "Resolving {0} path \"{1}\" ...", type, filename);
            return Path.GetFullPath(filename);//fixme: check to see if this is relevant
            //File f = new File(filename);
            //if (!f.isAbsolute())
            //{
            //    foreach (string prefix in searchPath)
            //    {
            //        UI.printDetailed(UI.Module.SYS, "  * searching: \"{0]\" ...", prefix);
            //        if (prefix.EndsWith(Path.DirectorySeparatorChar.ToString()) || filename.StartsWith(Path.DirectorySeparatorChar.ToString()))
            //            f = new File(prefix + filename);
            //        else
            //            f = new File(prefix + File.separator + filename);
            //        if (f.exists())
            //        {
            //            // suggested path exists - try it
            //            return f.getAbsolutePath();
            //        }
            //    }
            //}
            //// file was not found in the search paths - return the filename itself
            //return filename;
        }
    }
}