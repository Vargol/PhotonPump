using System;
using SunflowSharp;
using SunflowSharp.Core.Accel;
using SunflowSharp.Maths;
using SunflowSharp.Systems;
using SunflowSharp.Systems.Ui;

namespace SunflowSharp.Core
{

    /**
     * This class represent a geometric object in its native object space. These
     * object are not rendered directly, they must be instanced via {@link Instance}.
     * This class performs all the bookkeeping needed for on-demand tesselation and
     * acceleration structure building.
     */
    public class Geometry : IRenderObject
    {
        private ITesselatable tesselatable;
        private PrimitiveList primitives;
        private AccelerationStructure accel;
        private int builtAccel;
        private int builtTess;
        private string acceltype;
        private object lockObj = new object();
        /**
         * Create a geometry from the specified tesselatable object. The actual
         * renderable primitives will be generated on demand.
         * 
         * @param tesselatable tesselation object
         */
        public Geometry(ITesselatable tesselatable)
        {
            this.tesselatable = tesselatable;
            primitives = null;
            accel = null;
            builtAccel = 0;
            builtTess = 0;
            acceltype = null;
        }

        /**
         * Create a geometry from the specified primitive aggregate. The
         * acceleration structure for this object will be built on demand.
         * 
         * @param primitives primitive list object
         */
        public Geometry(PrimitiveList primitives)
        {
            tesselatable = null;
            this.primitives = primitives;
            accel = null;
            builtAccel = 0;
            builtTess = 1; // already tesselated
        }

        public bool Update(ParameterList pl, SunflowAPI api)
        {
            acceltype = pl.getstring("accel", acceltype);
            // clear up old tesselation if it exists
            if (tesselatable != null)
            {
                primitives = null;
                builtTess = 0;
            }
            // clear acceleration structure so it will be rebuilt
            accel = null;
            builtAccel = 0;
            if (tesselatable != null)
                return tesselatable.Update(pl, api);
            // update primitives
            return primitives.Update(pl, api);
        }

        public int getNumPrimitives()
        {
            return primitives == null ? 0 : primitives.getNumPrimitives();
        }

        public BoundingBox getWorldBounds(Matrix4 o2w)
        {
            if (primitives == null)
            {

                BoundingBox b = tesselatable.GetWorldBounds(o2w);
                if (b != null)
                    return b;
                if (builtTess == 0)
                    tesselate();
                if (primitives == null)
                    return null; // failed tesselation, return infinite bounding
                // box
            }
            return primitives.getWorldBounds(o2w);
        }

        public void intersect(Ray r, IntersectionState state)
        {
            if (builtTess == 0)
                tesselate();
            if (builtAccel == 0)
                build();
            accel.intersect(r, state);
        }

        private void tesselate()
        {
            lock (lockObj)
            {
                // double check flag
                if (builtTess != 0)
                    return;
                if (tesselatable != null && primitives == null)
                {
                    UI.printInfo(UI.Module.GEOM, "Tesselating geometry ...");
                    primitives = tesselatable.Tesselate();
                    if (primitives == null)
                        UI.printError(UI.Module.GEOM, "Tesselation failed - geometry will be discarded");
                    else
                        UI.printDetailed(UI.Module.GEOM, "Tesselation produced {0} primitives", primitives.getNumPrimitives());
                }
                builtTess = 1;
            }
        }

        private void build()
        {
            lock (lockObj)
            {
                // double check flag
                if (builtAccel != 0)
                    return;
                if (primitives != null)
                {
                    int n = primitives.getNumPrimitives();
                    if (n >= 1000)
                        UI.printInfo(UI.Module.GEOM, "Building acceleration structure for {0} primitives ...", n);
                    accel = AccelerationStructureFactory.create(acceltype, n, true);
                    accel.build(primitives);
                }
                else
                {
                    // create an empty accelerator to avoid having to check for null
                    // pointers in the intersect method
                    accel = new NullAccelerator();
                }
                builtAccel = 1;
            }
        }

        public void prepareShadingState(ShadingState state)
        {
            primitives.prepareShadingState(state);
        }

        public PrimitiveList getBakingPrimitives()
        {
            if (builtTess == 0)
                tesselate();
            if (primitives == null)
                return null;
            return primitives.getBakingPrimitives();
        }

        public PrimitiveList getPrimitiveList()
        {
            return primitives;
        }
    }
}