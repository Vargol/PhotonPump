using System;
using SunflowSharp.Core;
using SunflowSharp.Image;

namespace SunflowSharp.Core.Shader
{

    public class TexturedAmbientOcclusionShader : AmbientOcclusionShader
    {
        private Texture tex;

        public TexturedAmbientOcclusionShader()
        {
            tex = null;
        }

        public override bool Update(ParameterList pl, SunflowAPI api)
        {
            string filename = pl.getstring("texture", null);
            if (filename != null)
                tex = TextureCache.getTexture(api.resolveTextureFilename(filename), false);
            return tex != null && base.Update(pl, api);
        }

        public override Color getBrightColor(ShadingState state)
        {
            return tex.getPixel(state.getUV().x, state.getUV().y);
        }
    }
}