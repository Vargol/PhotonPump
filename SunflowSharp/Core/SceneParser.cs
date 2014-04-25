using System;
using System.IO;
using SunflowSharp;

namespace SunflowSharp.Core
{
    /// <summary>
    /// Simple interface to allow for scene creation from arbitrary file formats.
    /// </summary>
    public interface SceneParser
    {
        /**
         * Parse the specified file to create a scene description into the provided
         * {@link SunflowAPI} object.
         * 
         * @param filename filename to parse
         * @param api scene to parse the file into
         * @return <code>true</code> upon sucess, or <code>false</code> if
         *         errors have occured.
         */
        bool parse(string filename, SunflowAPI api);

        /// <summary>
        /// Parse the specified file to create a scene description into the provided <typeparamref name="SunFlowApi"/> object
        /// </summary>
        /// <param name="stream">Stream to parse</param>
        /// <param name="api">Scene to parse the file into</param>
        /// <returns>true upon sucess, or false if errors have occured.</returns>
        bool parse(Stream stream, SunflowAPI api);

        /// <summary>
        /// Determines if the parser can parse the given input.
        /// </summary>
        /// <param name="stream">Can be null</param>
        /// <param name="filename">Can be null</param>
        /// <returns>True if the parser can parse the Stream OR the filename.</returns>
        bool CanParse(Stream stream, string filename);
    }
}