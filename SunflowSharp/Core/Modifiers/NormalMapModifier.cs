using System;
using SunflowSharp.Core;
using SunflowSharp.Maths;

namespace SunflowSharp.Core.Modifiers
{
    public class NormalMapModifier : Modifier
    {
        private Texture normalMap;

        public NormalMapModifier()
        {
            normalMap = null;
        }
        public bool update(ParameterList pl, SunflowAPI api)
        {
            string filename = pl.getstring("texture", null);
            if (filename != null)
                normalMap = TextureCache.getTexture(api.resolveTextureFilename(filename), true);
            return normalMap != null;
        }

        public void modify(ShadingState state)
        {
            // apply normal map
            state.getNormal().set(normalMap.getNormal(state.getUV().x, state.getUV().y, state.getBasis()));
            state.setBasis(OrthoNormalBasis.makeFromW(state.getNormal()));
        }
    }
}