using System;
using SunflowSharp.Core;
using SunflowSharp.Image;
using SunflowSharp.Maths;
using SunflowSharp.Systems;
using SunflowSharp.Systems.Ui;

namespace SunflowSharp.Core.Gi
{

    public class PathTracingGIEngine : GIEngine
    {
        private int samples;

		public bool init(Options options, Scene scene)
        {
            samples = options.getInt("gi.path.samples", 16);
            samples = Math.Max(0, samples);
            UI.printInfo(UI.Module.LIGHT, "Path tracer settings:");
            UI.printInfo(UI.Module.LIGHT, "  * Samples: %d", samples);
            return true;
        }

        public Color getIrradiance(ShadingState state, Color diffuseReflectance)
        {
            if (samples <= 0)
                return Color.BLACK;
            // compute new sample
            Color irr = Color.black();
            OrthoNormalBasis onb = state.getBasis();
            Vector3 w = new Vector3();
            int n = state.getDiffuseDepth() == 0 ? samples : 1;
            for (int i = 0; i < n; i++)
            {
                float xi = (float)state.getRandom(i, 0, n);
                float xj = (float)state.getRandom(i, 1, n);
                float phi = (float)(xi * 2 * Math.PI);
                float cosPhi = (float)Math.Cos(phi);
                float sinPhi = (float)Math.Sin(phi);
                float sinTheta = (float)Math.Sqrt(xj);
                float cosTheta = (float)Math.Sqrt(1.0f - xj);
                w.x = cosPhi * sinTheta;
                w.y = sinPhi * sinTheta;
                w.z = cosTheta;
                onb.transform(w);
                ShadingState temp = state.traceFinalGather(new Ray(state.getPoint(), w), i);
                if (temp != null)
                {
                    temp.getInstance().prepareShadingState(temp);
                    if (temp.getShader() != null)
                        irr.add(temp.getShader().GetRadiance(temp));
                }
            }
            irr.mul((float)Math.PI / n);
            return irr;
        }

        public Color getGlobalRadiance(ShadingState state)
        {
            return Color.BLACK;
        }
    }
}