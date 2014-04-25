using System;
using SunflowSharp.Core;
using SunflowSharp.Image;

namespace SunflowSharp.Core.Shader
{

    public class ViewGlobalPhotonsShader : IShader
    {
        public bool update(ParameterList pl, SunflowAPI api)
        {
            return true;
        }

        public Color getRadiance(ShadingState state)
        {
            state.faceforward();
            return state.getGlobalRadiance();
        }

        public void scatterPhoton(ShadingState state, Color power)
        {
        }
    }
}