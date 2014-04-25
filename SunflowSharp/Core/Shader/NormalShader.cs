using System;
using SunflowSharp.Core;
using SunflowSharp.Image;
using SunflowSharp.Maths;

namespace SunflowSharp.Core.Shader
{

    public class NormalShader : IShader
    {
        public bool update(ParameterList pl, SunflowAPI api)
        {
            return true;
        }

        public Color getRadiance(ShadingState state)
        {
            Vector3 n = state.getNormal();
            if (n == null)
                return Color.BLACK;
            float r = (n.x + 1) * 0.5f;
            float g = (n.y + 1) * 0.5f;
            float b = (n.z + 1) * 0.5f;
            return new Color(r, g, b);
        }

        public void scatterPhoton(ShadingState state, Color power)
        {
        }
    }
}