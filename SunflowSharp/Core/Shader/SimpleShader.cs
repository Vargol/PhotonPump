using System;
using SunflowSharp.Core;
using SunflowSharp.Image;

namespace SunflowSharp.Core.Shader
{

    public class SimpleShader : IShader
    {
        public bool Update(ParameterList pl, SunflowAPI api)
        {
            return true;
        }

        public Color GetRadiance(ShadingState state)
        {
            return new Color(Math.Abs(state.getRay().dot(state.getNormal())));
        }

        public void ScatterPhoton(ShadingState state, Color power)
        {
        }
    }
}