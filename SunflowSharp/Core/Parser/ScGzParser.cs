using System;
using System.IO;
using System.IO.Compression;

namespace SunflowSharp.Core.Parser
{
    public class ScGzParser : SCParser
    {
        #region SceneParser Members
        public override bool parse(Stream stream, SunflowAPI api)
        {
            GZipStream gz = new GZipStream(stream, CompressionMode.Decompress);
            return base.parse(gz, api);
        }
        #endregion
    }
}
