using System;
using System.Collections;
using System.Collections.Generic;
using SunflowSharp.Core.Primitive;
using SunflowSharp.Image;
using SunflowSharp.Maths;

namespace SunflowSharp.Core
{

    /**
     * Represents a point to be shaded and provides various options for the shading
     * of this point, including spawning of new rays.
     */
    public class ShadingState : IEnumerable<LightSample>//Iterable<LightSample>
    {
        private IntersectionState istate;
        private LightServer server;
        private float rx, ry, time;
        private Color result;
        private Point3 p;
        private Vector3 n;
        private Point2 tex;
        private Vector3 ng;
        private OrthoNormalBasis basis;
		private float cosND;
		private float bias;
		private bool behind;
		private float hitU, hitV, hitW;
        private Instance instance;
        private int primitiveID;
		private Matrix4 o2w, w2o;
        private Ray r;
        private int d; // quasi monte carlo instance variables
        private int i; // quasi monte carlo instance variables
        private double qmcD0I;
        private double qmcD1I;
        private IShader shader;
        private Modifier modifier;
        private int diffuseDepth;
        private int reflectionDepth;
        private int refractionDepth;
        private bool _includeLights;
        private bool _includeSpecular;
        private LightSample lightSample;
        private PhotonStore map;

        public static ShadingState createPhotonState(Ray r, IntersectionState istate, int i, PhotonStore map, LightServer server)
        {
            ShadingState s = new ShadingState(null, istate, r, i, 4);
            s.server = server;
            s.map = map;
            return s;

        }

		public static ShadingState createState(IntersectionState istate, float rx, float ry, float time, Ray r, int i, int d, LightServer server)
        {
            ShadingState s = new ShadingState(null, istate, r, i, d);
            s.server = server;
            s.rx = rx;
            s.ry = ry;
			s.time = time;
            return s;
        }

        public static ShadingState createDiffuseBounceState(ShadingState previous, Ray r, int i)
        {
            ShadingState s = new ShadingState(previous, previous.istate, r, i, 2);
            s.diffuseDepth++;
            return s;
        }

        public static ShadingState createGlossyBounceState(ShadingState previous, Ray r, int i)
        {
            ShadingState s = new ShadingState(previous, previous.istate, r, i, 2);
            s.includeLights = false;
            s.includeSpecular = false;
            s.reflectionDepth++;
            return s;
        }

        public static ShadingState createReflectionBounceState(ShadingState previous, Ray r, int i)
        {
            ShadingState s = new ShadingState(previous, previous.istate, r, i, 2);
            s.reflectionDepth++;
            return s;
        }

        public static ShadingState createRefractionBounceState(ShadingState previous, Ray r, int i)
        {
            ShadingState s = new ShadingState(previous, previous.istate, r, i, 2);
            s.refractionDepth++;
            return s;
        }

        public static ShadingState createFinalGatherState(ShadingState state, Ray r, int i)
        {
            ShadingState finalGatherState = new ShadingState(state, state.istate, r, i, 2);
            finalGatherState.diffuseDepth++;
            finalGatherState.includeLights = false;
            finalGatherState.includeSpecular = false;
            return finalGatherState;
        }

        private ShadingState(ShadingState previous, IntersectionState istate, Ray r, int i, int d)
        {
            this.r = r;
            this.istate = istate;
            this.i = i;
            this.d = d;
			time = istate.time;
            instance = istate.instance; // local copy
            primitiveID = istate.id;
            hitU = istate.u;
            hitV = istate.v;
			hitW = istate.w;
			// get matrices for current time
			o2w = instance.getObjectToWorld(time);
			w2o = instance.getWorldToObject(time);

			if (previous == null)
            {
                diffuseDepth = 0;
                reflectionDepth = 0;
                refractionDepth = 0;
            }
            else
            {
                diffuseDepth = previous.diffuseDepth;
                reflectionDepth = previous.reflectionDepth;
                refractionDepth = previous.refractionDepth;
                server = previous.server;
                map = previous.map;
                rx = previous.rx;
                ry = previous.ry;
                this.i += previous.i;
                this.d += previous.d;
            }
            behind = false;
            cosND = float.NaN;
            includeLights = includeSpecular = true;
            qmcD0I = QMC.halton(this.d, this.i);
            qmcD1I = QMC.halton(this.d + 1, this.i);
            result = null;
			bias = 0.001f;
        }

        public void setRay(Ray r)
        {
            this.r = r;
        }

        /**
         * Create objects needed for surface shading: point, normal, texture
         * coordinates and basis.
         */
        public void init()
        {
            p = new Point3();
            n = new Vector3();
            tex = new Point2();
            ng = new Vector3();
            basis = null;
        }

        /**
         * Run the shader at this surface point.
         * 
         * @return shaded result
         */
        public Color shade()
        {
            return server.shadeHit(this);
        }

        public void correctShadingNormal()
        {
            // correct shading normals pointing the wrong way
            if (Vector3.dot(n, ng) < 0)
            {
                n.negate();
                basis.flipW();
            }
        }

        /**
         * Flip the surface normals to ensure they are facing the current ray. This
         * method also offsets the shading point away from the surface so that new
         * rays will not intersect the same surface again by mistake.
         */
        public void faceforward()
        {
            // make sure we are on the right side of the material
            if (r.dot(ng) < 0)
            {
            }
            else
            {
                // this ensure the ray and the geomtric normal are pointing in the
                // same direction
                ng.negate();
                n.negate();
                basis.flipW();
                behind = true;
            }
            cosND = Math.Max(-r.dot(n), 0); // can't be negative
            // offset the shaded point away from the surface to prevent
            // self-intersection errors
			bias = Math.Max(bias, 25.0f * float.Epsilon);
			p.x += bias * ng.x;
			p.y += bias * ng.y;
			p.z += bias * ng.z;
        }

        /**
         * Get x coordinate of the pixel being shaded.
         * 
         * @return pixel x coordinate
         */
        public float getRasterX()
        {
            return rx;
        }

        /**
         * Get y coordinate of the pixel being shaded.
         * 
         * @return pixel y coordinate
         */
        public float getRasterY()
        {
            return ry;
        }

        /**
         * Cosine between the shading normal and the ray. This is set by
         * {@link #faceforward()}.
         * 
         * @return cosine between shading normal and the ray
         */
        public float getCosND()
        {
            return cosND;
        }

        /**
         * Returns true if the ray hit the surface from behind. This is set by
         * {@link #faceforward()}.
         * 
         * @return <code>true</code> if the surface was hit from behind.
         */
        public bool isBehind()
        {
            return behind;
        }

        public IntersectionState getIntersectionState()
        {
            return istate;
        }

        /**
         * Get u barycentric coordinate of the intersection point.
         * 
         * @return u barycentric coordinate
         */
        public float getU()
        {
            return hitU;
        }

        /**
         * Get v barycentric coordinate of the intersection point.
         * 
         * @return v barycentric coordinate
         */
        public float getV()
        {
            return hitV;
        }

		/**
     	 * Get w barycentric coordinate of the intersection point.
     	 * 
     	 * @return w barycentric coordinate
     	 */
		public float getW() 
		{
			return hitW;
		}
        /**
         * Get the instance which was intersected
         * 
         * @return intersected instance object
         */
        public Instance getInstance()
        {
            return instance;
        }

        /**
         * Get the primitive ID which was intersected
         * 
         * @return intersected primitive ID
         */
        public int getPrimitiveID()
        {
            return primitiveID;
        }


		/**
		 * Transform the given point from object space to world space. A new
 		 * {@link Point3} object is returned.
 		 *
		 * @param p object space position to transform
		 * @return transformed position
		 */
		public Point3 transformObjectToWorld(Point3 p) {
			return o2w == null ? new Point3(p) : o2w.transformP(p);
		}
		
		/**
 	 	 * Transform the given point from world space to object space. A new
 		 * {@link Point3} object is returned.
		 *
		 * @param p world space position to transform
		 * @return transformed position
		 */
		public Point3 transformWorldToObject(Point3 p) {
			return w2o == null ? new Point3(p) : w2o.transformP(p);
		}
		
		/**
		 * Transform the given normal from object space to world space. A new
		 * {@link Vector3} object is returned.
		 *
		 * @param n object space normal to transform
		 * @return transformed normal
		 */
		public Vector3 transformNormalObjectToWorld(Vector3 n) {
			return o2w == null ? new Vector3(n) : w2o.transformTransposeV(n);
		}
		
		/**
		* Transform the given normal from world space to object space. A new
		* {@link Vector3} object is returned.
		*
		* @param n world space normal to transform
		* @return transformed normal
		*/
		public Vector3 transformNormalWorldToObject(Vector3 n) {
			return o2w == null ? new Vector3(n) : o2w.transformTransposeV(n);
		}
		
		/**
		 * Transform the given vector from object space to world space. A new
		 * {@link Vector3} object is returned.
		 *
		 * @param v object space vector to transform
		 * @return transformed vector
		 */
		public Vector3 transformVectorObjectToWorld(Vector3 v) {
			return o2w == null ? new Vector3(v) : o2w.transformV(v);
		}
		
		/**
 		 * Transform the given vector from world space to object space. A new
		 * {@link Vector3} object is returned.
		 *
		 * @param v world space vector to transform
		 * @return transformed vector
		 */
		public Vector3 transformVectorWorldToObject(Vector3 v) {
			return o2w == null ? new Vector3(v) : w2o.transformV(v);
		}


        public void setResult(Color c)
        {
            result = c;
        }

        /**
         * Get the result of shading this point
         * 
         * @return shaded result
         */
        public Color getResult()
        {
            return result;
        }

        LightServer getLightServer()
        {
            return server;
        }

        /**
         * Add the specified light sample to the list of lights to be used
         * 
         * @param sample a valid light sample
         */
        public void addSample(LightSample sample)
        {
            // add to list
            sample.next = lightSample;
            lightSample = sample;
        }

        /**
         * Get a QMC sample from an infinite sequence.
         * 
         * @param j sample number (starts from 0)
         * @param dim dimension to sample
         * @return pseudo-random value in [0,1)
         */
        public double getRandom(int j, int dim)
        {
            switch (dim)
            {
                case 0:
                    return QMC.mod1(qmcD0I + QMC.halton(0, j));
                case 1:
                    return QMC.mod1(qmcD1I + QMC.halton(1, j));
                default:
                    return QMC.mod1(QMC.halton(d + dim, i) + QMC.halton(dim, j));
            }
        }

        /**
         * Get a QMC sample from a finite sequence of n elements. This provides
         * better stratification than the infinite version, but does not allow for
         * adaptive sampling.
         * 
         * @param j sample number (starts from 0)
         * @param dim dimension to sample
         * @param n number of samples
         * @return pseudo-random value in [0,1)
         */
        public double getRandom(int j, int dim, int n)
        {
            switch (dim)
            {
                case 0:
                    return QMC.mod1(qmcD0I + (double)j / (double)n);
                case 1:
                    return QMC.mod1(qmcD1I + QMC.halton(0, j));
                default:
                    return QMC.mod1(QMC.halton(d + dim, i) + QMC.halton(dim - 1, j));
            }
        }

        /**
         * Checks to see if the shader should include emitted light.
         * 
         * @return <code>true</code> if emitted light should be included,
         *         <code>false</code> otherwise
         */
        public bool includeLights
        {
            get
            {
                return _includeLights;
            }
            set
            {
                _includeLights = value;
            }
        }

        /**
         * Checks to see if the shader should include specular terms.
         * 
         * @return <code>true</code> if specular terms should be included,
         *         <code>false</code> otherwise
         */
        public bool includeSpecular
        {
            get
            {
                return _includeSpecular;
            }
            set
            {
                _includeSpecular = value;
            }
        }

        /**
         * Get the shader to be used to shade this surface.
         * 
         * @return shader to be used
         */
        public IShader getShader()
        {
            return shader;
        }

        /**
         * Record which shader should be executed for the intersected surface.
         * 
         * @param shader surface shader to use to shade the current intersection
         *            point
         */
        public void setShader(IShader shader)
        {
            this.shader = shader;
        }

        public Modifier getModifier()
        {
            return modifier;
        }

        /**
         * Record which modifier should be applied to the intersected surface
         * 
         * @param modifier modifier to use the change this shading state
         */
        public void setModifier(Modifier modifier)
        {
            this.modifier = modifier;
        }

        /**
         * Get the current total tracing depth. First generation rays have a depth
         * of 0.
         * 
         * @return current tracing depth
         */
        public int getDepth()
        {
            return diffuseDepth + reflectionDepth + refractionDepth;
        }

        /**
         * Get the current diffuse tracing depth. This is the number of diffuse
         * surfaces reflected from.
         * 
         * @return current diffuse tracing depth
         */
        public int getDiffuseDepth()
        {
            return diffuseDepth;
        }

        /**
         * Get the current reflection tracing depth. This is the number of specular
         * surfaces reflected from.
         * 
         * @return current reflection tracing depth
         */
        public int getReflectionDepth()
        {
            return reflectionDepth;
        }

        /**
         * Get the current refraction tracing depth. This is the number of specular
         * surfaces refracted from.
         * 
         * @return current refraction tracing depth
         */
        public int getRefractionDepth()
        {
            return refractionDepth;
        }

        /**
         * Get hit point.
         * 
         * @return hit point
         */
        public Point3 getPoint()
        {
            return p;
        }

        /**
         * Get shading normal at the hit point. This may differ from the geometric
         * normal
         * 
         * @return shading normal
         */
        public Vector3 getNormal()
        {
            return n;
        }

        /**
         * Get texture coordinates at the hit point.
         * 
         * @return texture coordinate
         */
        public Point2 getUV()
        {
            return tex;
        }

        /**
         * Gets the geometric normal of the current hit point.
         * 
         * @return geometric normal of the current hit point
         */
        public Vector3 getGeoNormal()
        {
            return ng;
        }

        /**
         * Gets the local orthonormal basis for the current hit point.
         * 
         * @return local basis or <code>null</code> if undefined
         */
        public OrthoNormalBasis getBasis()
        {
            return basis;
        }

        /**
         * Define the orthonormal basis for the current hit point.
         * 
         * @param basis
         */
        public void setBasis(OrthoNormalBasis basis)
        {
            this.basis = basis;
        }

        /**
         * Gets the ray that is associated with this state.
         * 
         * @return ray associated with this state.
         */
        public Ray getRay()
        {
            return r;
        }

        /**
         * Get a transformation matrix that will transform camera space points into
         * world space.
         * 
         * @return camera to world transform
         */
        public Matrix4 getCameraToWorld()
        {
            CameraBase c = server.getScene().getCamera();
            return c != null ? c.getCameraToWorld(time) : Matrix4.IDENTITY;
        }

        /**
         * Get a transformation matrix that will transform world space points into
         * camera space.
         * 
         * @return world to camera transform
         */
        public Matrix4 getWorldToCamera()
        {
            CameraBase c = server.getScene().getCamera();
            return c != null ? c.getWorldToCamera(time) : Matrix4.IDENTITY;
        }

        /**
         * Get the three triangle corners in object space if the hit object is a
         * mesh, returns false otherwise.
         * 
         * @param p array of 3 points
         * @return <code>true</code> if the points were read succesfully,
         *         <code>false</code>otherwise
         */
        public bool getTrianglePoints(Point3[] p)
        {
            PrimitiveList prims = instance.getGeometry().getPrimitiveList();
            if (prims is TriangleMesh)
            {
                TriangleMesh m = (TriangleMesh)prims;
                m.getPoint(primitiveID, 0, p[0] = new Point3());
                m.getPoint(primitiveID, 1, p[1] = new Point3());
                m.getPoint(primitiveID, 2, p[2] = new Point3());
                return true;
            }
            return false;
        }

        /**
         * Initialize the use of light samples. Prepares a list of visible lights
         * from the current point.
         */
        public void initLightSamples()
        {
            server.initLightSamples(this);
        }

        /**
         * Add caustic samples to the current light sample set. This method does
         * nothing if caustics are not enabled.
         */
        public void initCausticSamples()
        {
            server.initCausticSamples(this);
        }

        /**
         * Returns the color obtained by recursively tracing the specified ray. The
         * reflection is assumed to be glossy.
         * 
         * @param r ray to trace
         * @param i instance number of this sample
         * @return color observed along specified ray.
         */
        public Color traceGlossy(Ray r, int i)
        {
			return server.traceGlossy(this, r, i);
        }

        /**
         * Returns the color obtained by recursively tracing the specified ray. The
         * reflection is assumed to be specular.
         * 
         * @param r ray to trace
         * @param i instance number of this sample
         * @return color observed along specified ray.
         */
        public Color traceReflection(Ray r, int i)
        {
			return server.traceReflection(this, r, i);
        }

        /**
         * Returns the color obtained by recursively tracing the specified ray.
         * 
         * @param r ray to trace
         * @param i instance number of this sample
         * @return color observed along specified ray.
         */
        public Color traceRefraction(Ray r, int i)
        {
			// this assumes the refraction ray is pointing away from the normal
			r.ox -= 2.0f * bias * ng.x;
			r.oy -= 2.0f * bias * ng.y;
			r.oz -= 2.0f * bias * ng.z;
            return server.traceRefraction(this, r, i);
        }

        /**
         * Trace transparency, this is equivalent to tracing a refraction ray in the
         * incoming ray direction.
         * 
         * @return color observed behind the current shading point
         */
        public Color traceTransparency()
        {
            return traceRefraction(new Ray(p.x, p.y, p.z, r.dx, r.dy, r.dz), 0);
        }

        /**
         * Trace a shadow ray against the scene, and computes the accumulated
         * opacity along the ray.
         * 
         * @param r ray to trace
         * @return opacity along the shadow ray
         */
        public Color traceShadow(Ray r)
        {
            return server.getScene().traceShadow(r, istate);
        }

        /**
         * Records a photon at the specified location.
         * 
         * @param dir incoming direction of the photon
         * @param power photon power
         * @param diffuse diffuse reflectance at the given point
         */
        public void storePhoton(Vector3 dir, Color power, Color diffuse)
        {
            map.store(this, dir, power, diffuse);
        }

        /**
         * Trace a new photon from the current location. This assumes that the
         * photon was reflected by a specular surface.
         * 
         * @param r ray to trace photon along
         * @param power power of the new photon
         */
        public void traceReflectionPhoton(Ray r, Color power)
        {
            if (map.allowReflectionBounced())
                server.traceReflectionPhoton(this, r, power);
        }

        /**
         * Trace a new photon from the current location. This assumes that the
         * photon was refracted by a specular surface.
         * 
         * @param r ray to trace photon along
         * @param power power of the new photon
         */
        public void traceRefractionPhoton(Ray r, Color power)
        {
            if (map.allowRefractionBounced())
            {
                // this assumes the refraction ray is pointing away from the normal
                r.ox -= 0.002f * ng.x;
                r.oy -= 0.002f * ng.y;
                r.oz -= 0.002f * ng.z;
                server.traceRefractionPhoton(this, r, power);
            }
        }

        /**
         * Trace a new photon from the current location. This assumes that the
         * photon was reflected by a diffuse surface.
         * 
         * @param r ray to trace photon along
         * @param power power of the new photon
         */
        public void traceDiffusePhoton(Ray r, Color power)
        {
            if (map.allowDiffuseBounced())
                server.traceDiffusePhoton(this, r, power);
        }

        /**
         * Returns the glboal diffuse radiance estimate given by the current
         * {@link GIEngine} if present.
         * 
         * @return global diffuse radiance estimate
         */
        public Color getGlobalRadiance()
        {
            return server.getGlobalRadiance(this);
        }

        /**
         * Gets the total irradiance reaching the current point from diffuse
         * surfaces.
         * 
         * @param diffuseReflectance diffuse reflectance at the current point, can
         *            be used for importance tracking
         * @return indirect diffuse irradiance reaching the point
         */
        public Color getIrradiance(Color diffuseReflectance)
        {
            return server.getIrradiance(this, diffuseReflectance);
        }

        /**
         * Trace a gather ray and return the intersection result as a new
         * render state
         * 
         * @param r ray to shoot
         * @param i instance of the ray
         * @return new render state object corresponding to the intersection result
         */
        public ShadingState traceFinalGather(Ray r, int i)
        {
            return server.traceFinalGather(this, r, i);
        }

        /**
         * Simple black and white ambient occlusion.
         * 
         * @param samples number of sample rays
         * @param maxDist maximum Length of the rays
         * @return occlusion color
         */
        public Color occlusion(int samples, float maxDist)
        {
            return occlusion(samples, maxDist, Color.WHITE, Color.BLACK);
        }

        /**
         * Ambient occlusion routine, returns a value between bright and dark
         * depending on the amount of geometric occlusion in the scene.
         * 
         * @param samples number of sample rays
         * @param maxDist maximum Length of the rays
         * @param bright color when nothing is occluded
         * @param dark color when fully occluded
         * @return occlusion color
         */
        public Color occlusion(int samples, float maxDist, Color bright, Color dark)
        {
            if (n == null)
            {
                // in case we got called on a geometry without orientation
                return bright;
            }
            // make sure we are on the right side of the material
            faceforward();
            OrthoNormalBasis onb = getBasis();
            Vector3 w = new Vector3();
            Color result = Color.black();
            for (int i = 0; i < samples; i++)
            {
                float xi = (float)getRandom(i, 0, samples);
                float xj = (float)getRandom(i, 1, samples);
                float phi = (float)(2 * Math.PI * xi);
                float cosPhi = (float)Math.Cos(phi);
                float sinPhi = (float)Math.Sin(phi);
                float sinTheta = (float)Math.Sqrt(xj);
                float cosTheta = (float)Math.Sqrt(1.0f - xj);
                w.x = cosPhi * sinTheta;
                w.y = sinPhi * sinTheta;
                w.z = cosTheta;
                onb.transform(w);
                Ray r = new Ray(p, w);
                r.setMax(maxDist);
                result.add(Color.blend(bright, dark, traceShadow(r)));
            }
            return result.mul(1.0f / samples);
        }

        /**
         * Computes a plain diffuse response to the current light samples and global
         * illumination.
         * 
         * @param diff diffuse color
         * @return shaded result
         */
        public Color diffuse(Color diff)
        {
			int i = 0;
			// integrate a diffuse function
            Color lr = Color.black();
            if (diff.isBlack())
                return lr;

            foreach (LightSample sample in this) 
				lr.madd (sample.dot (n), sample.getDiffuseRadiance ());


            lr.add(getIrradiance(diff));
            return lr.mul(diff).mul(1.0f / (float)Math.PI);
        }

        /**
         * Computes a phong specular response to the current light samples and
         * global illumination.
         * 
         * @param spec specular color
         * @param power phong exponent
         * @param numRays number of glossy rays to trace
         * @return shaded color
         */
        public Color specularPhong(Color spec, float power, int numRays)
        {
            // integrate a phong specular function
            Color lr = Color.black();
            if (!includeSpecular || spec.isBlack())
                return lr;
            // reflected direction
            float dn = 2 * cosND;
            Vector3 refDir = new Vector3();
            refDir.x = (dn * n.x) + r.dx;
            refDir.y = (dn * n.y) + r.dy;
            refDir.z = (dn * n.z) + r.dz;
            // direct lighting
            foreach (LightSample sample in this)
            {
                float cosNL = sample.dot(n);
                float cosLR = sample.dot(refDir);
                if (cosLR > 0)
                    lr.madd(cosNL * (float)Math.Pow(cosLR, power), sample.getSpecularRadiance());
            }
            // indirect lighting
            if (numRays > 0)
            {
                int numSamples = getDepth() == 0 ? numRays : 1;
                OrthoNormalBasis onb = OrthoNormalBasis.makeFromW(refDir);
                float mul = (2.0f * (float)Math.PI / (power + 1)) / numSamples;
                for (int i = 0; i < numSamples; i++)
                {
                    // specular indirect lighting
                    double r1 = getRandom(i, 0, numSamples);
                    double r2 = getRandom(i, 1, numSamples);
                    double u = 2 * Math.PI * r1;
                    double s = (float)Math.Pow(r2, 1 / (power + 1));
                    double s1 = (float)Math.Sqrt(1 - s * s);
                    Vector3 w = new Vector3((float)(Math.Cos(u) * s1), (float)(Math.Sin(u) * s1), (float)s);
                    w = onb.transform(w, new Vector3());
                    float wn = Vector3.dot(w, n);
                    if (wn > 0)
                        lr.madd(wn * mul, traceGlossy(new Ray(p, w), i));
                }
            }
            lr.mul(spec).mul((power + 2) / (2.0f * (float)Math.PI));
            return lr;
        }

        /**
         * Allows iteration over current light samples.
         */
        //public Iterator<LightSample> iterator()
        //{
        //    return new LightSampleIterator(lightSample);
        //}

        //private static class LightSampleIterator : Iterator<LightSample>
        //{
        //    private LightSample current;

        //    LightSampleIterator(LightSample head)
        //    {
        //        current = head;
        //    }

        //    public bool hasNext()
        //    {
        //        return current != null;
        //    }

        //    public LightSample next()
        //    {
        //        LightSample c = current;
        //        current = current.next;
        //        return c;
        //    }

        //    public void remove()
        //    {
        //        throw new UnsupportedOperationException();
        //    }
        //}

        #region IEnumerable<LightSample> Members

        IEnumerator<LightSample> IEnumerable<LightSample>.GetEnumerator()
        {
            return new LightSampleIterator(lightSample);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new LightSampleIterator(lightSample);
        }

        #endregion

        public class LightSampleIterator : IEnumerator<LightSample>
        {
            LightSample head, current;

            public LightSampleIterator(LightSample head)
            {
				this.head = new LightSample();
				this.head.next = head;
				current = this.head;
            }

            #region IEnumerator<LightSample> Members

            public LightSample Current
            {
                get
                {
					return current;
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get
                {
					return current;
                }
            }

            public bool MoveNext()
            {
				bool tmp = (current = current != null ? current.next : null) != null;
				return tmp;
            }

            public void Reset()
            {
                current = head;
            }

            #endregion
        }
    }
}