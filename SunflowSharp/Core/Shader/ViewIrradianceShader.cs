using System;
using SunflowSharp.Core;
using SunflowSharp.Image;

namespace SunflowSharp.Core.Shader
{

    public class ViewIrradianceShader : IShader
    {
        public bool update(ParameterList pl, SunflowAPI api)
        {
            return true;
        }

        public Color getRadiance(ShadingState state)
        {
            state.faceforward();
            return new Color().set(state.getIrradiance(Color.WHITE)).mul(1.0f / (float)Math.PI);
        }

        public void scatterPhoton(ShadingState state, Color power)
        {
        }
    }
}