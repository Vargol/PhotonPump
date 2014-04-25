using System;
using SunflowSharp.Core;
using SunflowSharp.Image;

namespace SunflowSharp.Core.Shader
{
    public class ConstantShader : IShader
    {
        private Color c;

        public ConstantShader()
        {
            c = Color.WHITE;
        }

        public bool update(ParameterList pl, SunflowAPI api)
        {
            c = pl.getColor("color", c);
            return true;
        }

        public Color getRadiance(ShadingState state)
        {
            return c;
        }

        public void scatterPhoton(ShadingState state, Color power)
        {
        }
    }
}