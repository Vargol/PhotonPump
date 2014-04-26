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

        public bool Update(ParameterList pl, SunflowAPI api)
        {
            c = pl.getColor("color", c);
            return true;
        }

        public Color GetRadiance(ShadingState state)
        {
            return c;
        }

        public void ScatterPhoton(ShadingState state, Color power)
        {
        }
    }
}