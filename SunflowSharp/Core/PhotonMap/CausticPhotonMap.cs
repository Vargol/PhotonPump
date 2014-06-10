using System;
using System.Collections.Generic;
using SunflowSharp.Core;
using SunflowSharp.Image;
using SunflowSharp.Maths;
using SunflowSharp.Systems;

namespace SunflowSharp.Core.PhotonMap
{

    public class CausticPhotonMap : CausticPhotonMapInterface
    {
        private List<Photon> photonList;
        private Photon[] photons;
        private int storedPhotons;
        private int halfStoredPhotons;
        private int log2n;
        private int gatherNum;
        private float gatherRadius;
        private BoundingBox bounds;
        private float filterValue;
        private float maxPower;
        private float maxRadius;
        private int _numEmit;
        private object lockObj = new object();

		public void prepare(Options options, BoundingBox sceneBounds)
        {
			// get options 
            _numEmit = options.getInt("caustics.emit", 10000);
            gatherNum = options.getInt("caustics.gather", 50);
            gatherRadius = options.getFloat("caustics.radius", 0.5f);
            filterValue = options.getFloat("caustics.filter", 1.1f);
            bounds = new BoundingBox();
			// init
			maxPower = 0;
            maxRadius = 0;
            photonList = new List<Photon>();
            photonList.Add(null);
            photons = null;
            storedPhotons = halfStoredPhotons = 0;
        }

        private void locatePhotons(NearestPhotons np)
        {
            float[] dist1d2 = new float[log2n];
            int[] chosen = new int[log2n];
            int i = 1;
            int level = 0;
            int cameFrom;
            while (true)
            {
                while (i < halfStoredPhotons)
                {
                    float dist1d = photons[i].getDist1(np.px, np.py, np.pz);
                    dist1d2[level] = dist1d * dist1d;
                    i += i;
                    if (dist1d > 0.0f)
                        i++;
                    chosen[level++] = i;
                }
                np.checkAddNearest(photons[i]);
                do
                {
                    cameFrom = i;
                    i >>= 1;
                    level--;
                    if (i == 0)
                        return;
                } while ((dist1d2[level] >= np.dist2[0]) || (cameFrom != chosen[level]));
                np.checkAddNearest(photons[i]);
                i = chosen[level++] ^ 1;
            }
        }

        private void balance()
        {
            if (storedPhotons == 0)
                return;
            photons = photonList.ToArray();
            photonList = null;

//			Photon[] temp = new Photon[storedPhotons + 1];
//            balanceSegment(temp, 1, 1, storedPhotons);
//            photons = temp;

			photons = Photon.BalancePhotons(ref photons);


			halfStoredPhotons = storedPhotons / 2;
			log2n = (int)Math.Ceiling(Math.Log(storedPhotons) / Math.Log(2.0));

        }

        private void balanceSegment(Photon[] temp, int index, int start, int end)
        {
            
			Console.WriteLine(String.Format("index {0}, start {1}, end {2}", index, start, end));
			 
			int median = 1;
            while ((4 * median) <= (end - start + 1))
                median += median;
            if ((3 * median) <= (end - start + 1))
            {
                median += median;
                median += (start - 1);
            }
            else
                median = end - median + 1;

			Console.WriteLine(String.Format("median {0}", median));

            int axis = Photon.SPLIT_Z;
            Vector3 extents = bounds.getExtents();
            if ((extents.x > extents.y) && (extents.x > extents.z))
                axis = Photon.SPLIT_X;
            else if (extents.y > extents.z)
                axis = Photon.SPLIT_Y;
            int left = start;
            int right = end;
            while (right > left)
            {
                double v = photons[right].getCoord(axis);
                int i = left - 1;
                int j = right;
                while (true)
                {
                    while (photons[++i].getCoord(axis) < v)
                    {
                    }
                    while ((photons[--j].getCoord(axis) > v) && (j > left))
                    {
                    }
                    if (i >= j)
                        break;
                    swap(i, j);
                }
                swap(i, right);
                if (i >= median)
                    right = i - 1;
                if (i <= median)
                    left = i + 1;
            }
            temp[index] = photons[median];
            temp[index].setSplitAxis(axis);
            if (median > start)
            {
                if (start < (median - 1))
                {
                    float tmp;
                    switch (axis)
                    {
                        case Photon.SPLIT_X:
                            tmp = bounds.getMaximum().x;
                            bounds.getMaximum().x = temp[index].x;
                            balanceSegment(temp, 2 * index, start, median - 1);
                            bounds.getMaximum().x = tmp;
                            break;
                        case Photon.SPLIT_Y:
                            tmp = bounds.getMaximum().y;
                            bounds.getMaximum().y = temp[index].y;
                            balanceSegment(temp, 2 * index, start, median - 1);
                            bounds.getMaximum().y = tmp;
                            break;
                        default:
                            tmp = bounds.getMaximum().z;
                            bounds.getMaximum().z = temp[index].z;
                            balanceSegment(temp, 2 * index, start, median - 1);
                            bounds.getMaximum().z = tmp;
                            break;
                    }
                }
                else
                    temp[2 * index] = photons[start];
            }
            if (median < end)
            {
                if ((median + 1) < end)
                {
                    float tmp;
                    switch (axis)
                    {
                        case Photon.SPLIT_X:
                            tmp = bounds.getMinimum().x;
                            bounds.getMinimum().x = temp[index].x;
                            balanceSegment(temp, (2 * index) + 1, median + 1, end);
                            bounds.getMinimum().x = tmp;
                            break;
                        case Photon.SPLIT_Y:
                            tmp = bounds.getMinimum().y;
                            bounds.getMinimum().y = temp[index].y;
                            balanceSegment(temp, (2 * index) + 1, median + 1, end);
                            bounds.getMinimum().y = tmp;
                            break;
                        default:
                            tmp = bounds.getMinimum().z;
                            bounds.getMinimum().z = temp[index].z;
                            balanceSegment(temp, (2 * index) + 1, median + 1, end);
                            bounds.getMinimum().z = tmp;
                            break;
                    }
                }
                else
                    temp[(2 * index) + 1] = photons[end];
            }
        }

        private void swap(int i, int j)
        {
            Photon tmp = photons[i];
            photons[i] = photons[j];
            photons[j] = tmp;
        }

        public void store(ShadingState state, Vector3 dir, Color power, Color diffuse)
        {
            if (((state.getDiffuseDepth() == 0) && (state.getReflectionDepth() > 0 || state.getRefractionDepth() > 0)))
            {
                // this is a caustic photon
                Photon p = new Photon(state.getPoint(), dir, power);
                lock (lockObj)
                {
                    storedPhotons++;
                    photonList.Add(p);
                    bounds.include(new Point3(p.x, p.y, p.z));
                    maxPower = Math.Max(maxPower, power.getMax());
                }
            }
        }

        public void init()
        {
            UI.printInfo(UI.Module.LIGHT, "Balancing caustics photon map ...");
            Timer t = new Timer();
            t.start();
            balance();
            t.end();
            UI.printInfo(UI.Module.LIGHT, "Caustic photon map:");
			UI.printInfo(UI.Module.LIGHT, "  * Photons stored:   {0}", storedPhotons);
			UI.printInfo(UI.Module.LIGHT, "  * Photons/estimate: {0}", gatherNum);
            maxRadius = 1.4f * (float)Math.Sqrt(maxPower * gatherNum);
			UI.printInfo(UI.Module.LIGHT, "  * Estimate radius:  {0,6:0.00}", gatherRadius);
			UI.printInfo(UI.Module.LIGHT, "  * Maximum radius:   {0,6:0.00}", maxRadius);
			UI.printInfo(UI.Module.LIGHT, "  * Balancing time:   {0}", t.ToString());
            if (gatherRadius > maxRadius)
                gatherRadius = maxRadius;
        }

        public void getSamples(ShadingState state)
        {
            if (storedPhotons == 0)
                return;
            NearestPhotons np = new NearestPhotons(state.getPoint(), gatherNum, gatherRadius * gatherRadius);
            locatePhotons(np);
            if (np.found < 8)
                return;
            Point3 ppos = new Point3();
            Vector3 pdir = new Vector3();
            Vector3 pvec = new Vector3();
            float invArea = 1.0f / ((float)Math.PI * np.dist2[0]);
            float maxNDist = np.dist2[0] * 0.05f;
            float f2r2 = 1.0f / (filterValue * filterValue * np.dist2[0]);
            float fInv = 1.0f / (1.0f - 2.0f / (3.0f * filterValue));
            for (int i = 1; i <= np.found; i++)
            {
                Photon phot = np.index[i];
                Vector3.decode(phot.dir, pdir);
                float cos = -Vector3.dot(pdir, state.getNormal());
                if (cos > 0.001)
                {
                    ppos.set(phot.x, phot.y, phot.z);
                    Point3.sub(ppos, state.getPoint(), pvec);
                    float pcos = Vector3.dot(pvec, state.getNormal());
                    if ((pcos < maxNDist) && (pcos > -maxNDist))
                    {
                        LightSample sample = new LightSample();
                        sample.setShadowRay(new Ray(state.getPoint(), pdir.negate()));
                        sample.setRadiance(new Color().setRGBE(np.index[i].power).mul(invArea / cos), Color.BLACK);
                        sample.getDiffuseRadiance().mul((1.0f - (float)Math.Sqrt(np.dist2[i] * f2r2)) * fInv);
                        state.addSample(sample);
                    }
                }
            }
        }

        private class NearestPhotons
        {
            public int found;
            public float px, py, pz;
            private int max;
            private bool gotHeap;
            public float[] dist2;
            public Photon[] index;

            public NearestPhotons(Point3 p, int n, float maxDist2)
            {
                max = n;
                found = 0;
                gotHeap = false;
                px = p.x;
                py = p.y;
                pz = p.z;
                dist2 = new float[n + 1];
                index = new Photon[n + 1];
                dist2[0] = maxDist2;
            }

            void reset(Point3 p, float maxDist2)
            {
                found = 0;
                gotHeap = false;
                px = p.x;
                py = p.y;
                pz = p.z;
                dist2[0] = maxDist2;
            }

            public void checkAddNearest(Photon p)
            {
                float fdist2 = p.getDist2(px, py, pz);
                if (fdist2 < dist2[0])
                {
                    if (found < max)
                    {
                        found++;
                        dist2[found] = fdist2;
                        index[found] = p;
                    }
                    else
                    {
                        int j;
                        int parent;
                        if (!gotHeap)
                        {
                            float dst2;
                            Photon phot;
                            int halfFound = found >> 1;
                            for (int k = halfFound; k >= 1; k--)
                            {
                                parent = k;
                                phot = index[k];
                                dst2 = dist2[k];
                                while (parent <= halfFound)
                                {
                                    j = parent + parent;
                                    if ((j < found) && (dist2[j] < dist2[j + 1]))
                                        j++;
                                    if (dst2 >= dist2[j])
                                        break;
                                    dist2[parent] = dist2[j];
                                    index[parent] = index[j];
                                    parent = j;
                                }
                                dist2[parent] = dst2;
                                index[parent] = phot;
                            }
                            gotHeap = true;
                        }
                        parent = 1;
                        j = 2;
                        while (j <= found)
                        {
                            if ((j < found) && (dist2[j] < dist2[j + 1]))
                                j++;
                            if (fdist2 > dist2[j])
                                break;
                            dist2[parent] = dist2[j];
                            index[parent] = index[j];
                            parent = j;
                            j += j;
                        }
                        dist2[parent] = fdist2;
                        index[parent] = p;
                        dist2[0] = dist2[1];
                    }
                }
            }
        }

  

        public bool allowDiffuseBounced()
        {
            return false;
        }

        public bool allowReflectionBounced()
        {
            return true;
        }

        public bool allowRefractionBounced()
        {
            return true;
        }

        public int numEmit()
        {
            return _numEmit;
        }
    }
}