using System;
using SunflowSharp.Core;
using SunflowSharp.Image;
using SunflowSharp.Maths;

namespace SunflowSharp.Core.Shader
{

    public class PrimIDShader : IShader
    {
        private static Color[] BORDERS = { Color.RED, Color.GREEN,
            Color.BLUE, Color.YELLOW, Color.CYAN, Color.MAGENTA };

        public bool Update(ParameterList pl, SunflowAPI api)
        {
            return true;
        }

        public Color GetRadiance(ShadingState state)
        {
            Vector3 n = state.getNormal();
            float f = n == null ? 1.0f : Math.Abs(state.getRay().dot(n));
            return BORDERS[state.getPrimitiveID() % BORDERS.Length].copy().mul(f);
        }

        public void ScatterPhoton(ShadingState state, Color power)
        {
        }
    }
}