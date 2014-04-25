using System;
using SunflowSharp.Core;
using SunflowSharp.Image;

namespace SunflowSharp.Core.Shader
{

    public class UVShader : IShader
    {
        public bool update(ParameterList pl, SunflowAPI api)
        {
            return true;
        }

        public Color getRadiance(ShadingState state)
        {
            if (state.getUV() == null)
                return Color.BLACK;
            return new Color(state.getUV().x, state.getUV().y, 0);
        }

        public void scatterPhoton(ShadingState state, Color power)
        {
        }
    }
}