using System;
using SunflowSharp.Core;
using SunflowSharp.Image;

namespace SunflowSharp.Core.Shader
{

    public class UVShader : IShader
    {
        public bool Update(ParameterList pl, SunflowAPI api)
        {
            return true;
        }

        public Color GetRadiance(ShadingState state)
        {
            if (state.getUV() == null)
                return Color.BLACK;
            return new Color(state.getUV().x, state.getUV().y, 0);
        }

        public void ScatterPhoton(ShadingState state, Color power)
        {
        }
    }
}