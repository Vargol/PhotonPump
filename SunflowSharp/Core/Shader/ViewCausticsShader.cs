using System;
using SunflowSharp.Core;
using SunflowSharp.Image;

namespace SunflowSharp.Core.Shader
{
    public class ViewCausticsShader : IShader
    {
        public bool update(ParameterList pl, SunflowAPI api)
        {
            return true;
        }

        public Color getRadiance(ShadingState state)
        {
            state.faceforward();
            state.initCausticSamples();
            // integrate a diffuse function
            Color lr = Color.black();
            foreach (LightSample sample in state)
                lr.madd(sample.dot(state.getNormal()), sample.getDiffuseRadiance());
            return lr.mul(1.0f / (float)Math.PI);

        }

        public void scatterPhoton(ShadingState state, Color power)
        {
        }
    }
}