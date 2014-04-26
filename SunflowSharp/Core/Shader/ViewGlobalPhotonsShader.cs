using System;
using SunflowSharp.Core;
using SunflowSharp.Image;

namespace SunflowSharp.Core.Shader
{

    public class ViewGlobalPhotonsShader : IShader
    {
        public bool Update(ParameterList pl, SunflowAPI api)
        {
            return true;
        }

        public Color GetRadiance(ShadingState state)
        {
            state.faceforward();
            return state.getGlobalRadiance();
        }

        public void ScatterPhoton(ShadingState state, Color power)
        {
        }
    }
}