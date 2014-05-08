using System;
using SunflowSharp.Core;
using SunflowSharp.Image;
using SunflowSharp.Maths;
using SunflowSharp.Systems;

namespace SunflowSharp.Core.PhotonMap
{

    public class GridPhotonMap : GlobalPhotonMapInterface
    {
        private int numGather;
        private float gatherRadius;
        private int numStoredPhotons;
        private int nx, ny, nz;
        private BoundingBox bounds;
        private PhotonGroup[] cellHash;
        private int hashSize;
        private int hashPrime;
        //private ReentrantReadWriteLock rwl;
        private int _numEmit;

        private static float NORMAL_THRESHOLD = (float)Math.Cos(10.0 * Math.PI / 180.0);
        private static int[] PRIMES = { 11, 19, 37, 109, 163, 251, 367, 557,
            823, 1237, 1861, 2777, 4177, 6247, 9371, 21089, 31627, 47431,
            71143, 106721, 160073, 240101, 360163, 540217, 810343, 1215497,
            1823231, 2734867, 4102283, 6153409, 9230113, 13845163 };
        private object lockObj = new object();

        public GridPhotonMap()
        {
            numStoredPhotons = 0;
            hashSize = 0; // number of unique IDs in the hash
            //rwl = new ReentrantReadWriteLock();
            _numEmit = 100000;
        }

		public void prepare(Options options, BoundingBox sceneBounds)
		{
			// get settings
			_numEmit = options.getInt("gi.irr-cache.gmap.emit", 100000);
			numGather = options.getInt("gi.irr-cache.gmap.gather", 50);
			gatherRadius = options.getFloat("gi.irr-cache.gmap.radius", 0.5f);
			// init
            bounds = new BoundingBox(sceneBounds);
            bounds.enlargeUlps();
            Vector3 w = bounds.getExtents();
            nx = (int)Math.Max(((w.x / gatherRadius) + 0.5f), 1);
            ny = (int)Math.Max(((w.y / gatherRadius) + 0.5f), 1);
            nz = (int)Math.Max(((w.z / gatherRadius) + 0.5f), 1);
            int numCells = nx * ny * nz;
            UI.printInfo(UI.Module.LIGHT, "Initializing grid photon map:");
			UI.printInfo(UI.Module.LIGHT, "  * Resolution:  {0}x{1}x{2}", nx, ny, nz);
			UI.printInfo(UI.Module.LIGHT, "  * Total cells: {0}", numCells);
            for (hashPrime = 0; hashPrime < PRIMES.Length; hashPrime++)
                if (PRIMES[hashPrime] > (numCells / 5))
                    break;
            cellHash = new PhotonGroup[PRIMES[hashPrime]];
			UI.printInfo(UI.Module.LIGHT, "  * Initial hash size: {0}", cellHash.Length);
        }

        public int size()
        {
            return numStoredPhotons;
        }

        public void store(ShadingState state, Vector3 dir, Color power, Color diffuse)
        {
            // don't store on the wrong side of a surface
            if (Vector3.dot(state.getNormal(), dir) > 0)
                return;
            Point3 pt = state.getPoint();
            // outside grid bounds ?
            if (!bounds.contains(pt))
                return;
            Vector3 ext = bounds.getExtents();
            int ix = (int)(((pt.x - bounds.getMinimum().x) * nx) / ext.x);
            int iy = (int)(((pt.y - bounds.getMinimum().y) * ny) / ext.y);
            int iz = (int)(((pt.z - bounds.getMinimum().z) * nz) / ext.z);
            ix = MathUtils.clamp(ix, 0, nx - 1);
            iy = MathUtils.clamp(iy, 0, ny - 1);
            iz = MathUtils.clamp(iz, 0, nz - 1);
            int id = ix + iy * nx + iz * nx * ny;
            lock (lockObj)
            {
                int hid = id % cellHash.Length;
                PhotonGroup g = cellHash[hid];
                PhotonGroup last = null;
                bool hasID = false;
                while (g != null)
                {
                    if (g.id == id)
                    {
                        hasID = true;
                        if (Vector3.dot(state.getNormal(), g.normal) > NORMAL_THRESHOLD)
                            break;
                    }
                    last = g;
                    g = g.next;
                }
                if (g == null)
                {
                    g = new PhotonGroup(id, state.getNormal());
                    if (last == null)
                        cellHash[hid] = g;
                    else
                        last.next = g;
                    if (!hasID)
                    {
                        hashSize++; // we have not seen this ID before
                        // resize hash if we have grown too large
                        if (hashSize > cellHash.Length)
                            growPhotonHash();
                    }
                }
                g.count++;
                g.flux.add(power);
                g.diffuse.add(diffuse);
                numStoredPhotons++;
            }
        }

        public void init()
        {
            UI.printInfo(UI.Module.LIGHT, "Initializing photon grid ...");
			UI.printInfo(UI.Module.LIGHT, "  * Photon hits:      {0}", numStoredPhotons);
			UI.printInfo(UI.Module.LIGHT, "  * hash size:  {0}", cellHash.Length);
            int cells = 0;
            for (int i = 0; i < cellHash.Length; i++)
            {
                for (PhotonGroup g = cellHash[i]; g != null; g = g.next)
                {
                    g.diffuse.mul(1.0f / g.count);
                    cells++;
                }
            }
			UI.printInfo(UI.Module.LIGHT, "  * Num photon cells: {0}", cells);
        }

        public void precomputeRadiance(bool includeDirect, bool includeCaustics)
        {
        }

        private void growPhotonHash()
        {
            // enlarge the hash size:
            if (hashPrime >= PRIMES.Length - 1)
                return;
            PhotonGroup[] temp = new PhotonGroup[PRIMES[++hashPrime]];
            for (int i = 0; i < cellHash.Length; i++)
            {
                PhotonGroup g = cellHash[i];
                while (g != null)
                {
                    // re-hash into the new table
                    int hid = g.id % temp.Length;
                    PhotonGroup last = null;
                    for (PhotonGroup gn = temp[hid]; gn != null; gn = gn.next)
                        last = gn;
                    if (last == null)
                        temp[hid] = g;
                    else
                        last.next = g;
                    PhotonGroup next = g.next;
                    g.next = null;
                    g = next;
                }
            }
            cellHash = temp;
        }

        public Color getRadiance(Point3 p, Vector3 n)
        {
            lock (lockObj)
            {
                if (!bounds.contains(p))
                    return Color.BLACK;
                Vector3 ext = bounds.getExtents();
                int ix = (int)(((p.x - bounds.getMinimum().x) * nx) / ext.x);
                int iy = (int)(((p.y - bounds.getMinimum().y) * ny) / ext.y);
                int iz = (int)(((p.z - bounds.getMinimum().z) * nz) / ext.z);
                ix = MathUtils.clamp(ix, 0, nx - 1);
                iy = MathUtils.clamp(iy, 0, ny - 1);
                iz = MathUtils.clamp(iz, 0, nz - 1);
                int id = ix + iy * nx + iz * nx * ny;
                //rwl.readLock().lockwoot();//fixme:
                PhotonGroup center = null;
                for (PhotonGroup g = get(ix, iy, iz); g != null; g = g.next)
                {
                    if (g.id == id && Vector3.dot(n, g.normal) > NORMAL_THRESHOLD)
                    {
                        if (g.radiance == null)
                        {
                            center = g;
                            break;
                        }
                        Color r = g.radiance.copy();
                        //rwl.readLock().unlock();
                        return r;
                    }
                }
                int vol = 1;
                while (true)
                {
                    int numPhotons = 0;
                    int ndiff = 0;
                    Color irr = Color.black();
                    Color diff = (center == null) ? Color.black() : null;
                    for (int z = iz - (vol - 1); z <= iz + (vol - 1); z++)
                    {
                        for (int y = iy - (vol - 1); y <= iy + (vol - 1); y++)
                        {
                            for (int x = ix - (vol - 1); x <= ix + (vol - 1); x++)
                            {
                                int vid = x + y * nx + z * nx * ny;
                                for (PhotonGroup g = get(x, y, z); g != null; g = g.next)
                                {
                                    if (g.id == vid && Vector3.dot(n, g.normal) > NORMAL_THRESHOLD)
                                    {
                                        numPhotons += g.count;
                                        irr.add(g.flux);
                                        if (diff != null)
                                        {
                                            diff.add(g.diffuse);
                                            ndiff++;
                                        }
                                        break; // only one valid group can be found,
                                        // skip the others
                                    }
                                }
                            }
                        }
                    }
                    if (numPhotons >= numGather || vol >= 3)
                    {
                        // we have found enough photons
                        // cache irradiance and return
                        float area = (2 * vol - 1) / 3.0f * ((ext.x / nx) + (ext.y / ny) + (ext.z / nz));
                        area *= area;
                        area *= (float)Math.PI;
                        irr.mul(1.0f / area);
                        // upgrade lock manually
                        //rwl.readLock().unlock();
                        //rwl.writeLock().lockwoot();//fixme:
                        if (center == null)
                        {
                            if (ndiff > 0)
                                diff.mul(1.0f / ndiff);
                            center = new PhotonGroup(id, n);
                            center.diffuse.set(diff);
                            center.next = cellHash[id % cellHash.Length];
                            cellHash[id % cellHash.Length] = center;
                        }
                        irr.mul(center.diffuse);
                        center.radiance = irr.copy();
                        //rwl.writeLock().unlock(); // unlock write - done
                        return irr;
                    }
                    vol++;
                }
            }
        }
        private PhotonGroup get(int x, int y, int z)
        {
            // returns the list associated with the specified location
            if (x < 0 || x >= nx)
                return null;
            if (y < 0 || y >= ny)
                return null;
            if (z < 0 || z >= nz)
                return null;
            return cellHash[(x + y * nx + z * nx * ny) % cellHash.Length];
        }

        private class PhotonGroup
        {
            public int id;
            public int count;
            public Vector3 normal;
            public Color flux;
            public Color radiance;
            public Color diffuse;
            public PhotonGroup next;

            public PhotonGroup(int id, Vector3 n)
            {
                normal = new Vector3(n);
                flux = Color.black();
                diffuse = Color.black();
                radiance = null;
                count = 0;
                this.id = id;
                next = null;
            }
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

        public int numEmit()
        {
            return _numEmit;
        }
    }
}