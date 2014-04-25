using System;
using SunflowSharp.Core;
using SunflowSharp.Image;
using SunflowSharp.Maths;

namespace SunflowSharp.Core.Gi
{

    /**
     * This is a quick way to get a bit of ambient lighting into your scene with
     * hardly any overhead. It's based on the formula found here:
     * 
     * @link http://www.cs.utah.edu/~shirley/papers/rtrt/node7.html#SECTION00031100000000000000
     */
    public class FakeGIEngine : GIEngine
    {
        private Vector3 up;
        private Color sky;
        private Color ground;

        public Color getIrradiance(ShadingState state, Color diffuseReflectance)
        {
            float cosTheta = Vector3.dot(up, state.getNormal());
            float sin2 = (1 - cosTheta * cosTheta);
            float sine = sin2 > 0 ? (float)Math.Sqrt(sin2) * 0.5f : 0;
            if (cosTheta > 0)
                return Color.blend(sky, ground, sine);
            else
                return Color.blend(ground, sky, sine);
        }

        public Color getGlobalRadiance(ShadingState state)
        {
            return Color.BLACK;
        }

		public bool init(Options options, Scene scene) {
			up = options.getVector("gi.fake.up", new Vector3(0, 1, 0)).normalize();
			sky = options.getColor("gi.fake.sky", Color.WHITE).copy();
			ground = options.getColor("gi.fake.ground", Color.BLACK).copy();
			sky.mul((float) Math.PI);
			ground.mul((float) Math.PI);
			return true;
        }
    }
}