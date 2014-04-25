using System;
using System.Collections.Generic;
using SunflowSharp.Core;
using SunflowSharp.Image;
using SunflowSharp.Maths;
using SunflowSharp.Systems;
using SunflowSharp.Systems.Ui;

namespace SunflowSharp.Core.Gi
{
    public class InstantGI : GIEngine
    {
        private int numPhotons;
        private int numSets;
        private float c;
        private int numBias;
        private PointLight[][] virtualLights;

        public Color getGlobalRadiance(ShadingState state)
        {
            Point3 p = state.getPoint();
            Vector3 n = state.getNormal();
            int set = (int)(state.getRandom(0, 1, 1) * numSets);
            float maxAvgPow = 0;
            float minDist = 1;
            Color pow = null;
            foreach (PointLight vpl in virtualLights[set])
            {
                maxAvgPow = Math.Max(maxAvgPow, vpl.power.getAverage());
                if (Vector3.dot(n, vpl.n) > 0.9f)
                {
                    float d = vpl.p.distanceToSquared(p);
                    if (d < minDist)
                    {
                        pow = vpl.power;
                        minDist = d;
                    }
                }
            }
            return pow == null ? Color.BLACK : pow.copy().mul(1.0f / maxAvgPow);
        }

        public bool init(Options options, Scene scene)
        {
			numPhotons = options.getInt("gi.igi.samples", 64);
			numSets = options.getInt("gi.igi.sets", 1);
			c = options.getFloat("gi.igi.c", 0.00003f);
			numBias = options.getInt("gi.igi.bias_samples", 0);
			virtualLights = null;
			if (numSets < 1)
                numSets = 1;
            UI.printInfo(UI.Module.LIGHT, "Instant Global Illumination settings:");
            UI.printInfo(UI.Module.LIGHT, "  * Samples:     {0}", numPhotons);
            UI.printInfo(UI.Module.LIGHT, "  * Sets:        {0}", numSets);
            UI.printInfo(UI.Module.LIGHT, "  * Bias bound:  {0}", c);
            UI.printInfo(UI.Module.LIGHT, "  * Bias rays:   {0}", numBias);
            virtualLights = new PointLight[numSets][];
            if (numPhotons > 0)
            {
                for (int i = 0, seed = 0; i < virtualLights.Length; i++, seed += numPhotons)
                {
                    PointLightStore map = new PointLightStore(this);
                    if (!scene.calculatePhotons(map, "virtual", seed, options))
                        return false;
                    virtualLights[i] = map.virtualLights.ToArray();
                    UI.printInfo(UI.Module.LIGHT, "Stored {0} virtual point lights for set {0} of {0}", virtualLights[i].Length, i + 1, numSets);
                }
            }
            else
            {
                // create an empty array
                for (int i = 0; i < virtualLights.Length; i++)
                    virtualLights[i] = new PointLight[0];
            }
            return true;
        }

        public Color getIrradiance(ShadingState state, Color diffuseReflectance)
        {
            float b = (float)Math.PI * c / diffuseReflectance.getMax();
            Color irr = Color.black();
            Point3 p = state.getPoint();
            Vector3 n = state.getNormal();
            int set = (int)(state.getRandom(0, 1, 1) * numSets);
            foreach (PointLight vpl in virtualLights[set])
            {
                Ray r = new Ray(p, vpl.p);
                float dotNlD = -(r.dx * vpl.n.x + r.dy * vpl.n.y + r.dz * vpl.n.z);
                float dotND = r.dx * n.x + r.dy * n.y + r.dz * n.z;
                if (dotNlD > 0 && dotND > 0)
                {
                    float r2 = r.getMax() * r.getMax();
                    Color opacity = state.traceShadow(r);
                    Color power = Color.blend(vpl.power, Color.BLACK, opacity);
                    float g = (dotND * dotNlD) / r2;
                    irr.madd(0.25f * Math.Min(g, b), power);
                }
            }
            // bias compensation
            int nb = (state.getDiffuseDepth() == 0 || numBias <= 0) ? numBias : 1;
            if (nb <= 0)
                return irr;
            OrthoNormalBasis onb = state.getBasis();
            Vector3 w = new Vector3();
            float scale = (float)Math.PI / nb;
            for (int i = 0; i < nb; i++)
            {
                float xi = (float)state.getRandom(i, 0, nb);
                float xj = (float)state.getRandom(i, 1, nb);
                float phi = (float)(xi * 2 * Math.PI);
                float cosPhi = (float)Math.Cos(phi);
                float sinPhi = (float)Math.Sin(phi);
                float sinTheta = (float)Math.Sqrt(xj);
                float cosTheta = (float)Math.Sqrt(1.0f - xj);
                w.x = cosPhi * sinTheta;
                w.y = sinPhi * sinTheta;
                w.z = cosTheta;
                onb.transform(w);
                Ray r = new Ray(state.getPoint(), w);
                r.setMax((float)Math.Sqrt(cosTheta / b));
                ShadingState temp = state.traceFinalGather(r, i);
                if (temp != null)
                {
                    temp.getInstance().prepareShadingState(temp);
                    if (temp.getShader() != null)
                    {
                        float dist = temp.getRay().getMax();
                        float r2 = dist * dist;
                        float cosThetaY = -Vector3.dot(w, temp.getNormal());
                        if (cosThetaY > 0)
                        {
                            float g = (cosTheta * cosThetaY) / r2;
                            // was this path accounted for yet?
                            if (g > b)
                                irr.madd(scale * (g - b) / g, temp.getShader().getRadiance(temp));
                        }
                    }
                }
            }
            return irr;
        }

        public class PointLight
        {
            public Point3 p;
            public Vector3 n;
            public Color power;
        }

        private class PointLightStore : PhotonStore
        {
            public List<PointLight> virtualLights = new List<PointLight>();
            InstantGI gi;
            object lockObj = new object();

            public PointLightStore(InstantGI gi)
            {
                this.gi = gi;
            }

            public int numEmit()
            {
                return gi.numPhotons;
            }

            public void prepare(Options options, BoundingBox sceneBounds)
            {
            }

            public void store(ShadingState state, Vector3 dir, Color power, Color diffuse)
            {
                state.faceforward();
                PointLight vpl = new PointLight();
                vpl.p = state.getPoint();
                vpl.n = state.getNormal();
                vpl.power = power;
                lock (lockObj)
                {
                    virtualLights.Add(vpl);
                }
            }

            public void init()
            {
            }

            public bool allowDiffuseBounced()
            {
                return true;
            }

            public bool allowReflectionBounced()
            {
                return true;
            }

            public bool allowRefractionBounced()
            {
                return true;
            }
        }
    }
}