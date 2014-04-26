using System;
using SunflowSharp.Core;
using SunflowSharp.Maths;

namespace SunflowSharp.Core.Primitive
{

    public class Background : PrimitiveList
    {
        public Background()
        {
        }

        public bool Update(ParameterList pl, SunflowAPI api)
        {
            return true;
        }

        public void prepareShadingState(ShadingState state)
        {
            if (state.getDepth() == 0)
                state.setShader(state.getInstance().getShader(0));
        }

        public int getNumPrimitives()
        {
            return 1;
        }

        public float getPrimitiveBound(int primID, int i)
        {
            return 0;
        }

        public BoundingBox getWorldBounds(Matrix4 o2w)
        {
            return null;
        }

        public void intersectPrimitive(Ray r, int primID, IntersectionState state)
        {
            if (r.getMax () == float.PositiveInfinity) {
				state.setIntersection (0, 0, 0);
			}
        }

        public PrimitiveList getBakingPrimitives()
        {
            return null;
        }
    }
}