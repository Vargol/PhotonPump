using System;
using System.Collections.Generic;
using SunflowSharp.Systems;

namespace SunflowSharp.Core
{

    /**
     * Maintains a cache of all loaded texture maps. This is usefull if the same
     * texture might be used more than once in your scene.
     */
    public class TextureCache
    {
        private static object lockObj = new object();
        private static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

        private TextureCache()
        {
        }

        /**
         * Gets a reference to the texture specified by the given filename. If the
         * texture has already been loaded the previous reference is returned,
         * otherwise, a new texture is created.
         * 
         * @param filename image file to load
         * @param isLinear is the texture gamma corrected?
         * @return texture object
         * @see Texture
         */
        public static Texture getTexture(string filename, bool isLinear)
        {
            lock (lockObj)
            {
                if (textures.ContainsKey(filename))
                {
					UI.printInfo(UI.Module.TEX, "Using cached copy for file \"{0}\" ...", filename);
                    return textures[filename];
                }
				UI.printInfo(UI.Module.TEX, "Using file \"{0}\" ...", filename);
                Texture t = new Texture(filename, isLinear);
                textures.Add(filename, t);
                return t;
            }
        }

        /**
         * Flush all textures from the cache, this will cause them to be reloaded
         * anew the next time they are accessed.
         */
        public static void flush()
        {
            lock (lockObj)
            {
                UI.printInfo(UI.Module.TEX, "Flushing texture cache");
                textures.Clear();
            }
        }
    }
}