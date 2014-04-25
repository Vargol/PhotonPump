using System;
using System.IO;

namespace SunflowSharp.Core.Parser
{
    public abstract class SceneParserBase : SceneParser
    {
        #region SceneParser Members
        public virtual bool parse(string filename, SunflowAPI api)
        {
            return parse(File.OpenRead(filename), api);
        }

        public abstract bool parse(System.IO.Stream stream, SunflowAPI api);

        public virtual bool CanParse(Stream stream, string filename)
        {
            return true;
        }

        #endregion
    }
}
