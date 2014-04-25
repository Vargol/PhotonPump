using System;
using SunflowSharp.Core;
using SunflowSharp.Maths;

namespace SunflowSharp.Core.Modifiers
{

    public class BumpMappingModifier : Modifier
    {
        private Texture bumpTexture;
        private float scale;

        public BumpMappingModifier()
        {
            bumpTexture = null;
            scale = 1;
        }

        public bool update(ParameterList pl, SunflowAPI api)
        {
            string filename = pl.getstring("texture", null);
            if (filename != null)
                bumpTexture = TextureCache.getTexture(api.resolveTextureFilename(filename), true);
            scale = pl.getFloat("scale", scale);
            return bumpTexture != null;
        }

        public void modify(ShadingState state)
        {
            // apply bump
            state.getNormal().set(bumpTexture.getBump(state.getUV().x, state.getUV().y, state.getBasis(), scale));
            state.setBasis(OrthoNormalBasis.makeFromW(state.getNormal()));
        }
    }
}