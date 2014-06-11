using System;
using SunflowSharp.Core;
using SunflowSharp.Maths;

namespace SunflowSharp.Core.Primitive
{

    public class Sphere : PrimitiveList
    {
        public bool Update(ParameterList pl, SunflowAPI api)
        {
            return true;
        }

        public BoundingBox getWorldBounds(Matrix4 o2w)
        {
            BoundingBox bounds = new BoundingBox(1);
            if (o2w != null)
                bounds = o2w.transform(bounds);
            return bounds;
        }

        public float getPrimitiveBound(int primID, int i)
        {
            return (i & 1) == 0 ? -1 : 1;
        }

        public int getNumPrimitives()
        {
            return 1;
        }

        public void prepareShadingState(ShadingState state)
        {
            state.init();
            state.getRay().getPoint(state.getPoint());
            Instance parent = state.getInstance();
            Point3 localPoint = state.transformWorldToObject(state.getPoint());
            state.getNormal().set(localPoint.x, localPoint.y, localPoint.z);
            state.getNormal().normalize();

            float phi = (float)Math.Atan2(state.getNormal().y, state.getNormal().x);
            if (phi < 0)
                phi += (float)(2 * Math.PI);
            float theta = (float)Math.Acos(state.getNormal().z);
            state.getUV().y = theta / (float)Math.PI;
            state.getUV().x = phi / (float)(2 * Math.PI);
            Vector3 v = new Vector3();
            v.x = -2 * (float)Math.PI * state.getNormal().y;
            v.y = 2 * (float)Math.PI * state.getNormal().x;
            v.z = 0;
            state.setShader(parent.getShader(0));
            state.setModifier(parent.getModifier(0));
            // into world space
            Vector3 worldNormal = state.transformNormalObjectToWorld(state.getNormal());
            v = state.transformVectorObjectToWorld(v);
            state.getNormal().set(worldNormal);
            state.getNormal().normalize();
            state.getGeoNormal().set(state.getNormal());
            // compute basis in world space
            state.setBasis(OrthoNormalBasis.makeFromWV(state.getNormal(), v));

        }

        public void intersectPrimitive(Ray r, int primID, IntersectionState state)
        {
            // intersect in local space
            float qa = r.dx * r.dx + r.dy * r.dy + r.dz * r.dz;
            float qb = 2 * ((r.dx * r.ox) + (r.dy * r.oy) + (r.dz * r.oz));
            float qc = ((r.ox * r.ox) + (r.oy * r.oy) + (r.oz * r.oz)) - 1.0f;
            double[] t = Solvers.solveQuadric(qa, qb, qc);
            if (t != null)
            {
                // early rejection
                if (t[0] >= r.getMax() || t[1] <= r.getMin())
                    return;
                if (t[0] > r.getMin())
                    r.setMax((float)t[0]);
                else
                    r.setMax((float)t[1]);
                state.setIntersection(0);
            }
        }

        public PrimitiveList getBakingPrimitives()
        {
            return null;
        }
    }
}