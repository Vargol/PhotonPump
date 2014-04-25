using System;
using SunflowSharp.Core;
using SunflowSharp.Image;

namespace SunflowSharp.Core.Shader
{

    public class SimpleShader : IShader
    {
        public bool update(ParameterList pl, SunflowAPI api)
        {
            return true;
        }

        public Color getRadiance(ShadingState state)
        {
            return new Color(Math.Abs(state.getRay().dot(state.getNormal())));
        }

        public void scatterPhoton(ShadingState state, Color power)
        {
        }
    }
}