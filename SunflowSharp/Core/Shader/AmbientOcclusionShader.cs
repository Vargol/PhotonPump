using System;
using SunflowSharp.Core;
using SunflowSharp.Image;

namespace SunflowSharp.Core.Shader
{
    public class AmbientOcclusionShader : IShader
    {
        private Color bright;
        private Color dark;
        private int samples;
        private float maxDist;

        public AmbientOcclusionShader()
        {
            bright = Color.WHITE;
            dark = Color.BLACK;
            samples = 32;
            maxDist = float.PositiveInfinity;
        }

        public AmbientOcclusionShader(Color c, float d)
            : this()
        {
            bright = c;
            maxDist = d;
        }

        public virtual bool update(ParameterList pl, SunflowAPI api)
        {
            bright = pl.getColor("bright", bright);
            dark = pl.getColor("dark", dark);
            samples = pl.getInt("samples", samples);
            maxDist = pl.getFloat("maxdist", maxDist);
            if (maxDist <= 0)
                maxDist = float.PositiveInfinity;
            return true;
        }

        public virtual Color getBrightColor(ShadingState state)
        {
            return bright;
        }

        public Color getRadiance(ShadingState state)
        {
            return state.occlusion(samples, maxDist, getBrightColor(state), dark);
        }

        public void scatterPhoton(ShadingState state, Color power)
        {
        }
    }
}