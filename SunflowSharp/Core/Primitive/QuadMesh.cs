using System;
using System.IO;
using SunflowSharp.Core;
using SunflowSharp.Maths;
using SunflowSharp.Systems;

namespace SunflowSharp.Core.Primitive
{

    public class QuadMesh : PrimitiveList
    {
        protected float[] points;
        protected int[] quads;
        private SunflowSharp.Core.ParameterList.FloatParameter normals;
        private SunflowSharp.Core.ParameterList.FloatParameter uvs;
        private byte[] faceShaders;

        public QuadMesh()
        {
            quads = null;
            points = null;
            normals = uvs = new ParameterList.FloatParameter();
            faceShaders = null;
        }

        public void writeObj(string filename)
        {
            try
            {
                //FileWriter file = new FileWriter(filename);
                StreamWriter file = new StreamWriter(File.OpenWrite(filename));
                file.WriteLine(string.Format("o object"));
                for (int i = 0; i < points.Length; i += 3)
					file.WriteLine(string.Format("v {0} {1} {2}", points[i], points[i + 1], points[i + 2]));
                file.WriteLine("s off");
                for (int i = 0; i < quads.Length; i += 4)
					file.WriteLine(string.Format("f {0} {1} {2} {3}", quads[i] + 1, quads[i + 1] + 1, quads[i + 2] + 1, quads[i + 3] + 1));
                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public bool Update(ParameterList pl, SunflowAPI api)
        {
            {
                int[] quads1 = pl.getIntArray("quads");
                if (quads != null)
                {
                    this.quads = quads1;
                }
            }
            if (quads == null)
            {
                UI.printError(UI.Module.GEOM, "Unable to update mesh - quad indices are missing");
                return false;
            }
            if (quads.Length % 4 != 0)
                UI.printWarning(UI.Module.GEOM, "Quad index data is not a multiple of 4 - some quads may be missing");
            pl.setFaceCount(quads.Length / 4);
            {
                ParameterList.FloatParameter pointsP = pl.getPointArray("points");
                if (pointsP != null)
                    if (pointsP.interp != ParameterList.InterpolationType.VERTEX)
						UI.printError(UI.Module.GEOM, "Point interpolation type must be set to \"vertex\" - was \"{0}\"", pointsP.interp.ToString().ToLower());
                    else
                    {
                        points = pointsP.data;
                    }
            }
            if (points == null)
            {
                UI.printError(UI.Module.GEOM, "Unabled to update mesh - vertices are missing");
                return false;
            }
            pl.setVertexCount(points.Length / 3);
            pl.setFaceVertexCount(4 * (quads.Length / 4));
            ParameterList.FloatParameter normals = pl.getVectorArray("normals");
            if (normals != null)
                this.normals = normals;
            ParameterList.FloatParameter uvs = pl.getTexCoordArray("uvs");
            if (uvs != null)
                this.uvs = uvs;
            int[] faceShaders = pl.getIntArray("faceshaders");
            if (faceShaders != null && faceShaders.Length == quads.Length / 4)
            {
                this.faceShaders = new byte[faceShaders.Length];
                for (int i = 0; i < faceShaders.Length; i++)
                {
                    int v = faceShaders[i];
                    if (v > 255)
						UI.printWarning(UI.Module.GEOM, "Shader index too large on quad {0}", i);
                    this.faceShaders[i] = (byte)(v & 0xFF);
                }
            }
            return true;
        }

        public float getPrimitiveBound(int primID, int i)
        {
            int quad = 4 * primID;
            int a = 3 * quads[quad + 0];
            int b = 3 * quads[quad + 1];
            int c = 3 * quads[quad + 2];
            int d = 3 * quads[quad + 3];
            int axis = (int)((uint)i >> 1);//>>>
            if ((i & 1) == 0)
                return MathUtils.min(points[a + axis], points[b + axis], points[c + axis], points[d + axis]);
            else
                return MathUtils.max(points[a + axis], points[b + axis], points[c + axis], points[d + axis]);
        }

        public BoundingBox getWorldBounds(Matrix4 o2w)
        {
            BoundingBox bounds = new BoundingBox();
            if (o2w == null)
            {
                for (int i = 0; i < points.Length; i += 3)
                    bounds.include(points[i], points[i + 1], points[i + 2]);
            }
            else
            {
                // transform vertices first
                for (int i = 0; i < points.Length; i += 3)
                {
                    float x = points[i];
                    float y = points[i + 1];
                    float z = points[i + 2];
                    float wx = o2w.transformPX(x, y, z);
                    float wy = o2w.transformPY(x, y, z);
                    float wz = o2w.transformPZ(x, y, z);
                    bounds.include(wx, wy, wz);
                }
            }
            return bounds;
        }

        public void intersectPrimitive(Ray r, int primID, IntersectionState state)
        {
            // ray/bilinear patch intersection adapted from "Production Rendering:
            // Design and Implementation" by Ian Stephenson (Ed.)
            int quad = 4 * primID;
            int p0 = 3 * quads[quad + 0];
            int p1 = 3 * quads[quad + 1];
            int p2 = 3 * quads[quad + 2];
            int p3 = 3 * quads[quad + 3];
            // transform patch into Hilbert space
            float[] A = {
                points[p2 + 0] - points[p3 + 0] - points[p1 + 0] + points[p0 + 0],
                points[p2 + 1] - points[p3 + 1] - points[p1 + 1] + points[p0 + 1],
                points[p2 + 2] - points[p3 + 2] - points[p1 + 2] + points[p0 + 2] };
            float[] B = { points[p1 + 0] - points[p0 + 0],
                points[p1 + 1] - points[p0 + 1],
                points[p1 + 2] - points[p0 + 2] };
            float[] C = { points[p3 + 0] - points[p0 + 0],
                points[p3 + 1] - points[p0 + 1],
                points[p3 + 2] - points[p0 + 2] };
            float[] R = { r.ox - points[p0 + 0], r.oy - points[p0 + 1],
                r.oz - points[p0 + 2] };
            float[] Q = { r.dx, r.dy, r.dz };

            // pick major direction
            float absqx = Math.Abs(r.dx);
            float absqy = Math.Abs(r.dy);
            float absqz = Math.Abs(r.dz);

            int X = 0, Y = 1, Z = 2;
            if (absqx > absqy && absqx > absqz)
            {
                // X = 0, Y = 1, Z = 2
            }
            else if (absqy > absqz)
            {
                // X = 1, Y = 0, Z = 2
                X = 1;
                Y = 0;
            }
            else
            {
                // X = 2, Y = 1, Z = 0
                X = 2;
                Z = 0;
            }

            float Cxz = C[X] * Q[Z] - C[Z] * Q[X];
            float Cyx = C[Y] * Q[X] - C[X] * Q[Y];
            float Czy = C[Z] * Q[Y] - C[Y] * Q[Z];
            float Rxz = R[X] * Q[Z] - R[Z] * Q[X];
            float Ryx = R[Y] * Q[X] - R[X] * Q[Y];
            float Rzy = R[Z] * Q[Y] - R[Y] * Q[Z];
            float Bxy = B[X] * Q[Y] - B[Y] * Q[X];
            float Byz = B[Y] * Q[Z] - B[Z] * Q[Y];
            float Bzx = B[Z] * Q[X] - B[X] * Q[Z];
            float a = A[X] * Byz + A[Y] * Bzx + A[Z] * Bxy;
            if (a == 0)
            {
                // setup for linear equation
                float b = B[X] * Czy + B[Y] * Cxz + B[Z] * Cyx;
                float c = C[X] * Rzy + C[Y] * Rxz + C[Z] * Ryx;
                float u = -c / b;
                if (u >= 0 && u <= 1)
                {
                    float v = (u * Bxy + Ryx) / Cyx;
                    if (v >= 0 && v <= 1)
                    {
                        float t = (B[X] * u + C[X] * v - R[X]) / Q[X];
                        if (r.isInside(t))
                        {
                            r.setMax(t);
                            state.setIntersection(primID, u, v);
                        }
                    }
                }
            }
            else
            {
                // setup for quadratic equation
                float b = A[X] * Rzy + A[Y] * Rxz + A[Z] * Ryx + B[X] * Czy + B[Y] * Cxz + B[Z] * Cyx;
                float c = C[X] * Rzy + C[Y] * Rxz + C[Z] * Ryx;
                float discrim = b * b - 4 * a * c;
                // reject trivial cases
                if (c * (a + b + c) > 0 && (discrim < 0 || a * c < 0 || b / a > 0 || b / a < -2))
                    return;
                // solve quadratic
                float q = b > 0 ? -0.5f * (b + (float)Math.Sqrt(discrim)) : -0.5f * (b - (float)Math.Sqrt(discrim));
                // check first solution
                float Axy = A[X] * Q[Y] - A[Y] * Q[X];
                float u = q / a;
                if (u >= 0 && u <= 1)
                {
                    float d = u * Axy - Cyx;
                    float v = -(u * Bxy + Ryx) / d;
                    if (v >= 0 && v <= 1)
                    {
                        float t = (A[X] * u * v + B[X] * u + C[X] * v - R[X]) / Q[X];
                        if (r.isInside(t))
                        {
                            r.setMax(t);
                            state.setIntersection(primID, u, v);
                        }
                    }
                }
                u = c / q;
                if (u >= 0 && u <= 1)
                {
                    float d = u * Axy - Cyx;
                    float v = -(u * Bxy + Ryx) / d;
                    if (v >= 0 && v <= 1)
                    {
                        float t = (A[X] * u * v + B[X] * u + C[X] * v - R[X]) / Q[X];
                        if (r.isInside(t))
                        {
                            r.setMax(t);
                            state.setIntersection(primID, u, v);
                        }
                    }
                }
            }
        }

        public int getNumPrimitives()
        {
            return quads.Length / 4;
        }

        public void prepareShadingState(ShadingState state)
        {
            state.init();
            Instance parent = state.getInstance();
            int primID = state.getPrimitiveID();
            float u = state.getU();
            float v = state.getV();
            state.getRay().getPoint(state.getPoint());
            int quad = 4 * primID;
            int index0 = quads[quad + 0];
            int index1 = quads[quad + 1];
            int index2 = quads[quad + 2];
            int index3 = quads[quad + 3];
            Point3 v0p = getPoint(index0);
            Point3 v1p = getPoint(index1);
            Point3 v2p = getPoint(index2);
            Point3 v3p = getPoint(index3);
            float tanux = (1 - v) * (v1p.x - v0p.x) + v * (v2p.x - v3p.x);
            float tanuy = (1 - v) * (v1p.y - v0p.y) + v * (v2p.y - v3p.y);
            float tanuz = (1 - v) * (v1p.z - v0p.z) + v * (v2p.z - v3p.z);

            float tanvx = (1 - u) * (v3p.x - v0p.x) + u * (v2p.x - v1p.x);
            float tanvy = (1 - u) * (v3p.y - v0p.y) + u * (v2p.y - v1p.y);
            float tanvz = (1 - u) * (v3p.z - v0p.z) + u * (v2p.z - v1p.z);

            float nx = tanuy * tanvz - tanuz * tanvy;
            float ny = tanuz * tanvx - tanux * tanvz;
            float nz = tanux * tanvy - tanuy * tanvx;

            Vector3 ng = new Vector3(nx, ny, nz);
            ng = state.transformNormalObjectToWorld(ng);
            ng.normalize();
            state.getGeoNormal().set(ng);

            float k00 = (1 - u) * (1 - v);
            float k10 = u * (1 - v);
            float k01 = (1 - u) * v;
            float k11 = u * v;

            switch (normals.interp)
            {
                case ParameterList.InterpolationType.NONE:
                case ParameterList.InterpolationType.FACE:
                    {
                        state.getNormal().set(ng);
                        break;
                    }
                case ParameterList.InterpolationType.VERTEX:
                    {
                        int i30 = 3 * index0;
                        int i31 = 3 * index1;
                        int i32 = 3 * index2;
                        int i33 = 3 * index3;
                        float[] normals1 = this.normals.data;
                        state.getNormal().x = k00 * normals1[i30 + 0] + k10 * normals1[i31 + 0] + k11 * normals1[i32 + 0] + k01 * normals1[i33 + 0];
                        state.getNormal().y = k00 * normals1[i30 + 1] + k10 * normals1[i31 + 1] + k11 * normals1[i32 + 1] + k01 * normals1[i33 + 1];
                        state.getNormal().z = k00 * normals1[i30 + 2] + k10 * normals1[i31 + 2] + k11 * normals1[i32 + 2] + k01 * normals1[i33 + 2];
                        state.getNormal().set(state.transformNormalObjectToWorld(state.getNormal()));
                        state.getNormal().normalize();
                        break;
                    }
                case ParameterList.InterpolationType.FACEVARYING:
                    {
                        int idx = 3 * quad;
                        float[] normals1 = this.normals.data;
                        state.getNormal().x = k00 * normals1[idx + 0] + k10 * normals1[idx + 3] + k11 * normals1[idx + 6] + k01 * normals1[idx + 9];
                        state.getNormal().y = k00 * normals1[idx + 1] + k10 * normals1[idx + 4] + k11 * normals1[idx + 7] + k01 * normals1[idx + 10];
                        state.getNormal().z = k00 * normals1[idx + 2] + k10 * normals1[idx + 5] + k11 * normals1[idx + 8] + k01 * normals1[idx + 11];
                        state.getNormal().set(state.transformNormalObjectToWorld(state.getNormal()));
                        state.getNormal().normalize();
                        break;
                    }
            }
            float uv00 = 0, uv01 = 0, uv10 = 0, uv11 = 0, uv20 = 0, uv21 = 0, uv30 = 0, uv31 = 0;
            switch (uvs.interp)
            {
                case ParameterList.InterpolationType.NONE:
                case ParameterList.InterpolationType.FACE:
                    {
                        state.getUV().x = 0;
                        state.getUV().y = 0;
                        break;
                    }
                case ParameterList.InterpolationType.VERTEX:
                    {
                        int i20 = 2 * index0;
                        int i21 = 2 * index1;
                        int i22 = 2 * index2;
                        int i23 = 2 * index3;
                        float[] uvs1 = this.uvs.data;
                        uv00 = uvs1[i20 + 0];
                        uv01 = uvs1[i20 + 1];
                        uv10 = uvs1[i21 + 0];
                        uv11 = uvs1[i21 + 1];
                        uv20 = uvs1[i22 + 0];
                        uv21 = uvs1[i22 + 1];
                        uv20 = uvs1[i23 + 0];
                        uv21 = uvs1[i23 + 1];
                        break;
                    }
                case ParameterList.InterpolationType.FACEVARYING:
                    {
                        int idx = quad << 1;
                        float[] uvs1 = this.uvs.data;
                        uv00 = uvs1[idx + 0];
                        uv01 = uvs1[idx + 1];
                        uv10 = uvs1[idx + 2];
                        uv11 = uvs1[idx + 3];
                        uv20 = uvs1[idx + 4];
                        uv21 = uvs1[idx + 5];
                        uv30 = uvs1[idx + 6];
                        uv31 = uvs1[idx + 7];
                        break;
                    }
            }
            if (uvs.interp != ParameterList.InterpolationType.NONE)
            {
                // get exact uv coords and compute tangent vectors
                state.getUV().x = k00 * uv00 + k10 * uv10 + k11 * uv20 + k01 * uv30;
                state.getUV().y = k00 * uv01 + k10 * uv11 + k11 * uv21 + k01 * uv31;
                float du1 = uv00 - uv20;
                float du2 = uv10 - uv20;
                float dv1 = uv01 - uv21;
                float dv2 = uv11 - uv21;
                Vector3 dp1 = Point3.sub(v0p, v2p, new Vector3()), dp2 = Point3.sub(v1p, v2p, new Vector3());
                float determinant = du1 * dv2 - dv1 * du2;
                if (determinant == 0.0f)
                {
                    // create basis in world space
                    state.setBasis(OrthoNormalBasis.makeFromW(state.getNormal()));
                }
                else
                {
                    float invdet = 1 / determinant;
                    // Vector3 dpdu = new Vector3();
                    // dpdu.x = (dv2 * dp1.x - dv1 * dp2.x) * invdet;
                    // dpdu.y = (dv2 * dp1.y - dv1 * dp2.y) * invdet;
                    // dpdu.z = (dv2 * dp1.z - dv1 * dp2.z) * invdet;
                    Vector3 dpdv = new Vector3();
                    dpdv.x = (-du2 * dp1.x + du1 * dp2.x) * invdet;
                    dpdv.y = (-du2 * dp1.y + du1 * dp2.y) * invdet;
                    dpdv.z = (-du2 * dp1.z + du1 * dp2.z) * invdet;
                    dpdv = state.transformVectorObjectToWorld(dpdv);
                    // create basis in world space
                    state.setBasis(OrthoNormalBasis.makeFromWV(state.getNormal(), dpdv));
                }
            }
            else
                state.setBasis(OrthoNormalBasis.makeFromW(state.getNormal()));
            int shaderIndex = faceShaders == null ? 0 : (faceShaders[primID] & 0xFF);
            state.setShader(parent.getShader(shaderIndex));
            state.setModifier(parent.getModifier(shaderIndex));
        }

        protected Point3 getPoint(int i)
        {
            i *= 3;
            return new Point3(points[i], points[i + 1], points[i + 2]);
        }

        public PrimitiveList getBakingPrimitives()
        {
            return null;
        }
    }
}