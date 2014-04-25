using System;

namespace SunflowSharp.Core
{

    public interface AccelerationStructure
    {
        /**
         * Construct an acceleration structure for the specified primitive list.
         * 
         * @param primitives
         */
        void build(PrimitiveList primitives);

        /**
         * Intersect the specified ray with the geometry in local space. The ray
         * will be provided in local space.
         * 
         * @param r ray in local space
         * @param istate state to store the intersection into
         */
        void intersect(Ray r, IntersectionState istate);
    }
}