using System;
using SunflowSharp.Core;
using SunflowSharp.Core.Primitive;
using SunflowSharp.Image;
using SunflowSharp.Maths;

namespace SunflowSharp.Core.Light
{

    public class SphereLight : LightSource, IShader
    {
        private Color radiance;
        private int numSamples;
        private Point3 center;
        private double radius;
        private double r2;

        public SphereLight()
        {
            radiance = Color.WHITE;
            numSamples = 4;
            center = new Point3();
            radius = r2 = 1;
        }

        public bool Update(ParameterList pl, SunflowAPI api)
        {
            radiance = pl.getColor("radiance", radiance);
            numSamples = pl.getInt("samples", numSamples);
            radius = pl.getDouble("radius", radius);
            r2 = radius * radius;
            center = pl.getPoint("center", center);
            return true;
        }

        public int getNumSamples()
        {
            return numSamples;
        }

        public int getLowSamples()
        {
            return 1;
        }

        public bool isVisible(ShadingState state)
        {
            return state.getPoint().distanceToSquared(center) > r2;
        }

        public void getSamples(ShadingState state)
        {
            if (getNumSamples() <= 0)
                return;
            Vector3 wc = Point3.sub(center, state.getPoint(), new Vector3());
            float l2 = wc.LengthSquared();
            if (l2 <= r2)
                return; // inside the sphere?
            // top of the sphere as viewed from the current shading point
			float topX = (float)(wc.x + state.getNormal().x * radius);
			float topY = (float)(wc.y + state.getNormal().y * radius);
			float topZ = (float)(wc.z + state.getNormal().z * radius);
            if (state.getNormal().dot(topX, topY, topZ) <= 0)
                return; // top of the sphere is below the horizon
            double cosThetaMax = Math.Sqrt(Math.Max(0, 1 - r2 / Vector3.dot(wc, wc)));
            OrthoNormalBasis basis = OrthoNormalBasis.makeFromW(wc);
            int samples = state.getDiffuseDepth() > 0 ? 1 : getNumSamples();
            float scale = (float)(2 * Math.PI * (1 - cosThetaMax));
            Color c = Color.mul(scale / samples, radiance);
            for (int i = 0; i < samples; i++)
            {
                // random offset on unit square
                double randX = state.getRandom(i, 0, samples);
                double randY = state.getRandom(i, 1, samples);

                // cone sampling
                double cosTheta = (1 - randX) * cosThetaMax + randX;
                double sinTheta = Math.Sqrt(1 - cosTheta * cosTheta);
                double phi = randY * 2 * Math.PI;
                Vector3 dir = new Vector3((float)(Math.Cos(phi) * sinTheta), (float)(Math.Sin(phi) * sinTheta), (float)cosTheta);
                basis.transform(dir);

                // check that the direction of the sample is the same as the
                // normal
                float cosNx = Vector3.dot(dir, state.getNormal());
                if (cosNx <= 0)
                    continue;

				double ocx = state.getPoint().x - center.x;
				double ocy = state.getPoint().y - center.y;
				double ocz = state.getPoint().z - center.z;
                float qa = Vector3.dot(dir, dir);
                float qb = (float)(2.0 * ((dir.x * ocx) + (dir.y * ocy) + (dir.z * ocz)));
				float qc = (float)(((ocx * ocx) + (ocy * ocy) + (ocz * ocz)) - r2);
                double[] t = Solvers.solveQuadric(qa, qb, qc);
                if (t == null)
                    continue;
                LightSample dest = new LightSample();
                // compute shadow ray to the sampled point
                dest.setShadowRay(new Ray(state.getPoint(), dir));
                // FIXME: arbitrary bias, should handle as in other places
                dest.getShadowRay().setMax((float)t[0] - 1e-3f);
                // prepare sample
                dest.setRadiance(c, c);
                dest.traceShadow(state);
                state.addSample(dest);
            }
        }

        public void getPhoton(double randX1, double randY1, double randX2, double randY2, Point3 p, Vector3 dir, Color power)
        {
            double z = (1 - 2 * randX2);
			double r = Math.Sqrt(Math.Max(0, 1 - z * z));
			double phi = (2 * Math.PI * randY2);
			double x = r * Math.Cos(phi);
			double y = r * Math.Sin(phi);
            p.x = (float)(center.x + x * radius);
			p.y = (float)(center.y + y * radius);
			p.z = (float)(center.z + z * radius);
			OrthoNormalBasis basis = OrthoNormalBasis.makeFromW(new Vector3((float)x, (float)y, (float)z));
            phi = (2 * Math.PI * randX1);
			double cosPhi = Math.Cos(phi);
			double sinPhi = Math.Sin(phi);
			double sinTheta = Math.Sqrt(randY1);
			double cosTheta = Math.Sqrt(1 - randY1);
			dir.x = (float)(cosPhi * sinTheta);
			dir.y = (float)(sinPhi * sinTheta);
			dir.z = (float)(cosTheta);
            basis.transform(dir);
            power.set(radiance);
            power.mul((float)(Math.PI * Math.PI * 4 * r2));
        }

        public float getPower()
        {
            return radiance.copy().mul((float)(Math.PI * Math.PI * 4 * r2)).getLuminance();
        }

        public Color GetRadiance(ShadingState state)
        {
            if (!state.includeLights)
                return Color.BLACK;
            state.faceforward();
            // emit constant radiance
            return state.isBehind() ? Color.BLACK : radiance;
        }

        public void ScatterPhoton(ShadingState state, Color power)
        {
            // do not scatter photons
        }

		public Instance createInstance() 
		{
			return Instance.createTemporary(new Sphere(), Matrix4.translation(center.x, center.y, center.z).multiply(Matrix4.scale(radius)), this);
		}

    }
}