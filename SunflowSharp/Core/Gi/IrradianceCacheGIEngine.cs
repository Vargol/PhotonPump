using System;
using SunflowSharp.Core;
using SunflowSharp.Image;
using SunflowSharp.Maths;
using SunflowSharp.Systems;

namespace SunflowSharp.Core.Gi
{
    public class IrradianceCacheGIEngine : GIEngine
    {
        private int samples;
        public float tolerance;
        private float invTolerance;
        public float minSpacing;
        public float maxSpacing;
        private Node root;
        //private ReentrantReadWriteLock rwl;
        private GlobalPhotonMapInterface globalPhotonMap;
        private object lockObj = new object();

		public bool init(Options options, Scene scene) 
		{			
			// get settings
            samples = options.getInt("gi.irr-cache.samples", 256);
            tolerance = options.getFloat("gi.irr-cache.tolerance", 0.05f);
            invTolerance = 1.0f / tolerance;
            minSpacing = options.getFloat("gi.irr-cache.min_spacing", 0.05f);
            maxSpacing = options.getFloat("gi.irr-cache.max_spacing", 5.00f);
            root = null;
            //rwl = new ReentrantReadWriteLock();
			globalPhotonMap = PluginRegistry.globalPhotonMapPlugins.createObject(options.getstring("gi.irr-cache.gmap", null));
			// check settings
            samples = Math.Max(0, samples);
            minSpacing = Math.Max(0.001f, minSpacing);
            maxSpacing = Math.Max(0.001f, maxSpacing);
            // display settings
            UI.printInfo(UI.Module.LIGHT, "Irradiance cache settings:");
			UI.printInfo(UI.Module.LIGHT, "  * Samples: {0}", samples);
            if (tolerance <= 0)
                UI.printInfo(UI.Module.LIGHT, "  * Tolerance: off");
            else
				UI.printInfo(UI.Module.LIGHT, "  * Tolerance: {0,3}", tolerance);
			UI.printInfo(UI.Module.LIGHT, "  * Spacing: {0,9:0.00} to {1,9:0.00}", minSpacing, maxSpacing);
            // prepare root node
            Vector3 ext = scene.getBounds().getExtents();
            root = new Node(scene.getBounds().getCenter(), 1.0001f * MathUtils.max(ext.x, ext.y, ext.z), this);
            // init global photon map
            return (globalPhotonMap != null) ? scene.calculatePhotons(globalPhotonMap, "global", 0, options) : true;
        }

        public Color getGlobalRadiance(ShadingState state)
        {
            if (globalPhotonMap == null)
            {
                if (state.getShader() != null)
                    return state.getShader().GetRadiance(state);
                else
                    return Color.BLACK;
            }
            else
                return globalPhotonMap.getRadiance(state.getPoint(), state.getNormal());
        }

        public Color getIrradiance(ShadingState state, Color diffuseReflectance)
        {
            if (samples <= 0)
                return Color.BLACK;
            if (state.getDiffuseDepth() > 0)
            {
                // do simple path tracing for additional bounces (single ray)
                float xi = (float)state.getRandom(0, 0, 1);
                float xj = (float)state.getRandom(0, 1, 1);
                float phi = (float)(xi * 2 * Math.PI);
                float cosPhi = (float)Math.Cos(phi);
                float sinPhi = (float)Math.Sin(phi);
                float sinTheta = (float)Math.Sqrt(xj);
                float cosTheta = (float)Math.Sqrt(1.0f - xj);
                Vector3 w = new Vector3();
                w.x = cosPhi * sinTheta;
                w.y = sinPhi * sinTheta;
                w.z = cosTheta;
                OrthoNormalBasis onb = state.getBasis();
                onb.transform(w);
                Ray r = new Ray(state.getPoint(), w);
                ShadingState temp = state.traceFinalGather(r, 0);
                return temp != null ? getGlobalRadiance(temp).copy().mul((float)Math.PI) : Color.BLACK;
            }
            //rwl.readLock().lockwoot();//fixme
            Color irr;
            lock(lockObj)
                irr = getIrradiance(state.getPoint(), state.getNormal());
            //rwl.readLock().unlock();
            if (irr == null)
            {
                // compute new sample
                irr = Color.black();
                OrthoNormalBasis onb = state.getBasis();
                float invR = 0;
                float minR = float.PositiveInfinity;
                Vector3 w = new Vector3();
                for (int i = 0; i < samples; i++)
                {
                    float xi = (float)state.getRandom(i, 0, samples);
                    float xj = (float)state.getRandom(i, 1, samples);
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
                    ShadingState temp = state.traceFinalGather(r, i);
                    if (temp != null)
                    {
                        minR = Math.Min(r.getMax(), minR);
                        invR += 1.0f / r.getMax();
                        temp.getInstance().prepareShadingState(temp);
                        irr.add(getGlobalRadiance(temp));
                    }
                }
                irr.mul((float)Math.PI / samples);
                invR = samples / invR;
                //rwl.writeLock().lockwoot();//fixme
                lock(lockObj)
                    insert(state.getPoint(), state.getNormal(), invR, irr);
                //rwl.writeLock().unlock();
                // view irr-cache points
                // irr = Color.YELLOW.copy().mul(1e6f);
            }
            return irr;
        }

        private void insert(Point3 p, Vector3 n, float r0, Color irr)
        {
            if (tolerance <= 0)
                return;
            Node node = root;
            r0 = MathUtils.clamp(r0 * tolerance, minSpacing, maxSpacing) * invTolerance;
            if (root.isInside(p))
            {
                while (node.sideLength >= (4.0 * r0 * tolerance))
                {
                    int k = 0;
                    k |= (p.x > node.center.x) ? 1 : 0;
                    k |= (p.y > node.center.y) ? 2 : 0;
                    k |= (p.z > node.center.z) ? 4 : 0;
                    if (node.children[k] == null)
                    {
                        Point3 c = new Point3(node.center);
                        c.x += ((k & 1) == 0) ? -node.quadSideLength : node.quadSideLength;
                        c.y += ((k & 2) == 0) ? -node.quadSideLength : node.quadSideLength;
                        c.z += ((k & 4) == 0) ? -node.quadSideLength : node.quadSideLength;
                        node.children[k] = new Node(c, node.halfSideLength, this);
                    }
                    node = node.children[k];
                }
            }
            Sample s = new Sample(p, n, r0, irr);
            s.next = node.first;
            node.first = s;
        }

        private Color getIrradiance(Point3 p, Vector3 n)
        {
            if (tolerance <= 0)
                return null;
            Sample x = new Sample(p, n);
            float w = root.find(x);
            return (x.irr == null) ? null : x.irr.mul(1.0f / w);
        }

        public class Node
        {
            public Node[] children;
            public Sample first;
            public Point3 center;
            public float sideLength;
            public float halfSideLength;
            public float quadSideLength;
            private IrradianceCacheGIEngine engine;

            public Node(Point3 center, float sideLength, IrradianceCacheGIEngine engine)
            {
                children = new Node[8];
                for (int i = 0; i < 8; i++)
                    children[i] = null;
                this.center = new Point3(center);
                this.sideLength = sideLength;
                halfSideLength = 0.5f * sideLength;
                quadSideLength = 0.5f * halfSideLength;
                first = null;
                this.engine = engine;
            }

            public bool isInside(Point3 p)
            {
                return (Math.Abs(p.x - center.x) < halfSideLength) && (Math.Abs(p.y - center.y) < halfSideLength) && (Math.Abs(p.z - center.z) < halfSideLength);
            }

            public float find(Sample x)
            {
                float weight = 0;
                for (Sample s = first; s != null; s = s.next)
                {
                    float c2 = 1.0f - (x.nix * s.nix + x.niy * s.niy + x.niz * s.niz);
                    float d2 = (x.pix - s.pix) * (x.pix - s.pix) + (x.piy - s.piy) * (x.piy - s.piy) + (x.piz - s.piz) * (x.piz - s.piz);
                    if (c2 > engine.tolerance * engine.tolerance || d2 > engine.maxSpacing * engine.maxSpacing)
                        continue;
                    float invWi = (float)(Math.Sqrt(d2) * s.invR0 + Math.Sqrt(Math.Max(c2, 0)));
                    if (invWi < engine.tolerance || d2 < engine.minSpacing * engine.minSpacing)
                    {
                        float wi = Math.Min(1e10f, 1.0f / invWi);
                        if (x.irr != null)
                            x.irr.madd(wi, s.irr);
                        else
                            x.irr = s.irr.copy().mul(wi);
                        weight += wi;
                    }
                }
                for (int i = 0; i < 8; i++)
                    if ((children[i] != null) && (Math.Abs(children[i].center.x - x.pix) <= halfSideLength) && (Math.Abs(children[i].center.y - x.piy) <= halfSideLength) && (Math.Abs(children[i].center.z - x.piz) <= halfSideLength))
                        weight += children[i].find(x);
                return weight;
            }
        }

        public class Sample
        {
            public float pix, piy, piz;
            public float nix, niy, niz;
            public float invR0;
            public Color irr;
            public Sample next;

            public Sample(Point3 p, Vector3 n)
            {
                pix = p.x;
                piy = p.y;
                piz = p.z;
                Vector3 ni = new Vector3(n).normalize();
                nix = ni.x;
                niy = ni.y;
                niz = ni.z;
                irr = null;
                next = null;
            }

            public Sample(Point3 p, Vector3 n, float r0, Color irr)
            {
                pix = p.x;
                piy = p.y;
                piz = p.z;
                Vector3 ni = new Vector3(n).normalize();
                nix = ni.x;
                niy = ni.y;
                niz = ni.z;
                invR0 = 1.0f / r0;
                this.irr = irr;
                next = null;
            }
        }
    }
}