using System;
using SunflowSharp.Core;
using SunflowSharp.Image;
using SunflowSharp.Maths;

namespace SunflowSharp.Core.Gi
{
    public class AmbientOcclusionGIEngine : GIEngine
    {
        private Color bright;
        private Color dark;
        private int samples;
        private float maxDist;

		public Color getGlobalRadiance(ShadingState state) 
		{
			return Color.BLACK;
		}
			
		public bool init(Options options, Scene scene) 
		{
            bright = options.getColor("gi.ambocc.bright", Color.WHITE);
            dark = options.getColor("gi.ambocc.dark", Color.BLACK);
            samples = options.getInt("gi.ambocc.samples", 32);
            maxDist = options.getFloat("gi.ambocc.maxdist", 0);
            maxDist = (maxDist <= 0) ? float.PositiveInfinity : maxDist;
            return true;
        }

        public Color getIrradiance(ShadingState state, Color diffuseReflectance)
        {
            OrthoNormalBasis onb = state.getBasis();
            Vector3 w = new Vector3();
            Color result = Color.black();
            for (int i = 0; i < samples; i++)
            {
                float xi = (float)state.getRandom(i, 0, samples);
                float xj = (float)state.getRandom(i, 1, samples);
                float phi = (float)(2 * Math.PI * xi);
                float cosPhi = (float)Math.Cos(phi);
                float sinPhi = (float)Math.Sin(phi);
                float sinTheta = (float)Math.Sqrt(xj);
                float cosTheta = (float)Math.Sqrt(1.0f - xj);
                w.x = cosPhi * sinTheta;
                w.y = sinPhi * sinTheta;
                w.z = cosTheta;
                onb.transform(w);
                Ray r = new Ray(state.getPoint(), w);
                r.setMax(maxDist);
                result.add(Color.blend(bright, dark, state.traceShadow(r)));
            }
            return result.mul((float)Math.PI / samples);
        }
    }
}