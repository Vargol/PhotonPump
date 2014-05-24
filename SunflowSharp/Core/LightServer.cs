using System;
using System.Threading;
using SunflowSharp.Core.Gi;
using SunflowSharp.Core.PhotonMap;
using SunflowSharp.Image;
using SunflowSharp.Maths;
using SunflowSharp.Systems;
using SunflowSharp.Systems.Ui;

namespace SunflowSharp.Core
{
    public class LightServer
    {
        // parent
        private Scene scene;

        // lighting
        public LightSource[] lights;

        // shading override
        private IShader shaderOverride;
        private bool shaderOverridePhotons;

        // direct illumination
        private int maxDiffuseDepth;
        private int maxReflectionDepth;
        private int maxRefractionDepth;

        // indirect illumination
        private CausticPhotonMapInterface causticPhotonMap;
        private GIEngine giEngine;
        private int photonCounter;

		private object lockObj = new object();

        public LightServer(Scene scene)
        {
            this.scene = scene;
            lights = new LightSource[0];
            causticPhotonMap = null;

            shaderOverride = null;
            shaderOverridePhotons = false;

            maxDiffuseDepth = 1;
            maxReflectionDepth = 4;
            maxRefractionDepth = 4;

            causticPhotonMap = null;
            giEngine = null;

        }

        public void setLights(LightSource[] lights)
        {
            this.lights = lights;
        }

        public Scene getScene()
        {
            return scene;
        }

        public void setShaderOverride(IShader shader, bool photonOverride)
        {
            shaderOverride = shader;
            shaderOverridePhotons = photonOverride;
        }

        public bool build(Options options)
        {
            // read options
            maxDiffuseDepth = options.getInt("depths.diffuse", maxDiffuseDepth);
            maxReflectionDepth = options.getInt("depths.reflection", maxReflectionDepth);
            maxRefractionDepth = options.getInt("depths.refraction", maxRefractionDepth);
			string giEngineType = options.getstring("gi.engine", null);
			giEngine = PluginRegistry.giEnginePlugins.createObject(giEngineType);
			string caustics = options.getstring("caustics", null);
			causticPhotonMap = PluginRegistry.causticPhotonMapPlugins.createObject(caustics);

            // validate options
            maxDiffuseDepth = Math.Max(0, maxDiffuseDepth);
            maxReflectionDepth = Math.Max(0, maxReflectionDepth);
            maxRefractionDepth = Math.Max(0, maxRefractionDepth);

            SunflowSharp.Systems.Timer t = new SunflowSharp.Systems.Timer();
            t.start();
            // count total number of light samples
            int numLightSamples = 0;
            for (int i = 0; i < lights.Length; i++)
                numLightSamples += lights[i].getNumSamples();
            // initialize gi engine
            if (giEngine != null)
            {
                if (!giEngine.init(options, scene))
                    return false;
            }

            if (!calculatePhotons(causticPhotonMap, "caustic", 0, options))
                return false;
            t.end();

            UI.printInfo(UI.Module.LIGHT, "Light Server stats:");
            UI.printInfo(UI.Module.LIGHT, "  * Light sources found: {0}", lights.Length);
            UI.printInfo(UI.Module.LIGHT, "  * Light samples:       {0}", numLightSamples);
            UI.printInfo(UI.Module.LIGHT, "  * Max raytrace depth:");
            UI.printInfo(UI.Module.LIGHT, "      - Diffuse          {0}", maxDiffuseDepth);
            UI.printInfo(UI.Module.LIGHT, "      - Reflection       {0}", maxReflectionDepth);
            UI.printInfo(UI.Module.LIGHT, "      - Refraction       {0}", maxRefractionDepth);
			UI.printInfo(UI.Module.LIGHT, "  * GI engine            {0}", giEngineType == null ? "none" : giEngineType);
            UI.printInfo(UI.Module.LIGHT, "  * Caustics:            {0}", caustics == null ? "none" : caustics);
            UI.printInfo(UI.Module.LIGHT, "  * Shader override:     {0}", shaderOverride);
            UI.printInfo(UI.Module.LIGHT, "  * Photon override:     {0}", shaderOverridePhotons);
            UI.printInfo(UI.Module.LIGHT, "  * Build time:          {0}", t.ToString());
            return true;
        }

        public void showStats() {}

        public class CalculatePhotons
        {
            int start, end, seed;
            object lockObj;
            LightServer server;
            float[] histogram;
            float scale;
            PhotonStore map;
            public CalculatePhotons(int start, int end, LightServer server, int seed, float[] histogram, float scale, PhotonStore map, object lockObj)
            {
                this.start = start;
                this.end = end;
                this.server = server;
                this.seed = seed;
                this.histogram = histogram;
                this.lockObj = lockObj;
                this.scale = scale;
                this.map = map;
                this.lockObj = lockObj;
            }

            public void Run()
            {
				ByteUtil.InitByteUtil();
				IntersectionState istate = new IntersectionState();
                for (int i = start; i < end; i++)
                {
                    lock (lockObj)
                    {
                        UI.taskUpdate(server.photonCounter);
                        server.photonCounter++;
                        if (UI.taskCanceled())
                            return;
                    }

                    int qmcI = i + seed;

                    double rand = QMC.halton(0, qmcI) * histogram[histogram.Length - 1];
                    int j = 0;
                    while (rand >= histogram[j] && j < histogram.Length)
                        j++;
                    // make sure we didn't pick a zero-probability light
                    if (j == histogram.Length)
                        continue;

                    double randX1 = (j == 0) ? rand / histogram[0] : (rand - histogram[j]) / (histogram[j] - histogram[j - 1]);
                    double randY1 = QMC.halton(1, qmcI);
                    double randX2 = QMC.halton(2, qmcI);
                    double randY2 = QMC.halton(3, qmcI);
                    Point3 pt = new Point3();
                    Vector3 dir = new Vector3();
                    Color power = new Color();
                    server.lights[j].getPhoton(randX1, randY1, randX2, randY2, pt, dir, power);
                    power.mul(scale);
                    Ray r = new Ray(pt, dir);
                    server.scene.trace(r, istate);
                    if (istate.hit())
                        server.shadePhoton(ShadingState.createPhotonState(r, istate, qmcI, map, server), power);
                }
            }
        }

        public bool calculatePhotons(PhotonStore map, string type, int seed, Options options)
        {
            if (map == null)
                return true;
            if (lights.Length == 0)
            {
                UI.printError(UI.Module.LIGHT, "Unable to trace {0} photons, no lights in scene", type);
                return false;
            }
            float[] histogram = new float[lights.Length];
            histogram[0] = lights[0].getPower();
            for (int i = 1; i < lights.Length; i++)
                histogram[i] = histogram[i - 1] + lights[i].getPower();
			UI.printInfo(UI.Module.LIGHT, "Tracing {0} photons ...", type);
			map.prepare(options, scene.getBounds());
            int numEmittedPhotons = map.numEmit();
            if (numEmittedPhotons <= 0 || histogram[histogram.Length - 1] <= 0)
            {
                UI.printError(UI.Module.LIGHT, "Photon mapping enabled, but no {0} photons to emit", type);
                return false;
            }
            UI.taskStart("Tracing " + type + " photons", 0, numEmittedPhotons);
            Thread[] photonThreads = new Thread[scene.getThreads()];
            float scale = 1.0f / numEmittedPhotons;
            int delta = numEmittedPhotons / photonThreads.Length;
            photonCounter = 0;
            SunflowSharp.Systems.Timer photonTimer = new SunflowSharp.Systems.Timer();
            photonTimer.start();
            for (int i = 0; i < photonThreads.Length; i++)
            {
                int threadID = i;
                int start = threadID * delta;
                int end = (threadID == (photonThreads.Length - 1)) ? numEmittedPhotons : (threadID + 1) * delta;
                photonThreads[i] = new Thread(new ThreadStart(new CalculatePhotons(start, end, this, seed, histogram, scale, map, lockObj).Run));
                photonThreads[i].Priority = scene.getThreadPriority();
                photonThreads[i].Start();
            }
            for (int i = 0; i < photonThreads.Length; i++)
            {
                try
                {
                    photonThreads[i].Join();
                }
                catch (Exception e)
                {
                    UI.printError(UI.Module.LIGHT, "Photon thread {0} of {1} was interrupted", i + 1, photonThreads.Length);
                    return false;
                }
            }
            if (UI.taskCanceled())
            {
                UI.taskStop(); // shut down task cleanly
                return false;
            }
            photonTimer.end();
            UI.taskStop();
            UI.printInfo(UI.Module.LIGHT, "Tracing time for {0} photons: {1}", type, photonTimer.ToString());
            map.init();
            return true;
        }

        void shadePhoton(ShadingState state, Color power)
        {
            state.getInstance().prepareShadingState(state);
            IShader shader = getPhotonShader(state);
            // scatter photon
            if (shader != null)
                shader.ScatterPhoton(state, power);
        }

        public void traceDiffusePhoton(ShadingState previous, Ray r, Color power)
        {
            if (previous.getDiffuseDepth() >= maxDiffuseDepth)
                return;
            IntersectionState istate = previous.getIntersectionState();
            scene.trace(r, istate);
            if (previous.getIntersectionState().hit())
            {
                // create a new shading context
                ShadingState state = ShadingState.createDiffuseBounceState(previous, r, 0);
                shadePhoton(state, power);
            }
        }

        public void traceReflectionPhoton(ShadingState previous, Ray r, Color power)
        {
            if (previous.getReflectionDepth() >= maxReflectionDepth)
                return;
            IntersectionState istate = previous.getIntersectionState();
            scene.trace(r, istate);
            if (previous.getIntersectionState().hit())
            {
                // create a new shading context
                ShadingState state = ShadingState.createReflectionBounceState(previous, r, 0);
                shadePhoton(state, power);
            }
        }

        public void traceRefractionPhoton(ShadingState previous, Ray r, Color power)
        {
            if (previous.getRefractionDepth() >= maxRefractionDepth)
                return;
            IntersectionState istate = previous.getIntersectionState();
            scene.trace(r, istate);
            if (previous.getIntersectionState().hit())
            {
                // create a new shading context
                ShadingState state = ShadingState.createRefractionBounceState(previous, r, 0);
                shadePhoton(state, power);
            }
        }

        private IShader getShader(ShadingState state)
        {
            return shaderOverride != null ? shaderOverride : state.getShader();
        }

        private IShader getPhotonShader(ShadingState state)
        {
            return (shaderOverride != null && shaderOverridePhotons) ? shaderOverride : state.getShader();

        }

		public ShadingState getRadiance(float rx, float ry, float time, int i, int d, Ray r, IntersectionState istate, ShadingCache cache) {
			istate.time = time;
			scene.trace(r, istate);
			if (istate.hit()) {
				ShadingState state = ShadingState.createState(istate, rx, ry, time, r, i, d, this);
				state.getInstance().prepareShadingState(state);
				IShader shader = getShader(state);
				if (shader == null) {
					state.setResult(Color.BLACK);
					return state;
				}
				if (cache != null) {
					Color c = cache.lookup(state, shader);
					if (c != null) {
						state.setResult(c);
						return state;
					}
				}
				state.setResult(shader.GetRadiance(state));
				if (cache != null)
					cache.add(state, shader, state.getResult());
				checkNanInf(state.getResult());
				return state;
			} else
				return null;
		}

		private static void checkNanInf(Color c) {
			if (c.isNan())
				UI.printWarning(UI.Module.LIGHT, "NaN shading sample!");
			else if (c.isInf())
				UI.printWarning(UI.Module.LIGHT, "Inf shading sample!");
		}

        public void shadeBakeResult(ShadingState state)
        {
            IShader shader = getShader(state);
            if (shader != null)
                state.setResult(shader.GetRadiance(state));
            else
                state.setResult(Color.BLACK);
        }

        public Color shadeHit(ShadingState state)
        {
            state.getInstance().prepareShadingState(state);
            IShader shader = getShader(state);
            return (shader != null) ? shader.GetRadiance(state) : Color.BLACK;
        }
	
        public Color traceGlossy(ShadingState previous, Ray r, int i)
        {
            // limit path depth and disable caustic paths
            if (previous.getReflectionDepth() >= maxReflectionDepth || previous.getDiffuseDepth() > 0)
                return Color.BLACK;
            IntersectionState istate = previous.getIntersectionState();
			istate.numGlossyRays++;
            scene.trace(r, istate);
            return istate.hit() ? shadeHit(ShadingState.createGlossyBounceState(previous, r, i)) : Color.BLACK;
        }

        public Color traceReflection(ShadingState previous, Ray r, int i)
        {
            // limit path depth and disable caustic paths
            if (previous.getReflectionDepth() >= maxReflectionDepth || previous.getDiffuseDepth() > 0)
                return Color.BLACK;
            IntersectionState istate = previous.getIntersectionState();
			istate.numReflectionRays++;
            scene.trace(r, istate);
            return istate.hit() ? shadeHit(ShadingState.createReflectionBounceState(previous, r, i)) : Color.BLACK;
        }

        public Color traceRefraction(ShadingState previous, Ray r, int i)
        {
            // limit path depth and disable caustic paths
            if (previous.getRefractionDepth() >= maxRefractionDepth || previous.getDiffuseDepth() > 0)
                return Color.BLACK;
            IntersectionState istate = previous.getIntersectionState();
			istate.numRefractionRays++;
            scene.trace(r, istate);
            return istate.hit() ? shadeHit(ShadingState.createRefractionBounceState(previous, r, i)) : Color.BLACK;
        }

        public ShadingState traceFinalGather(ShadingState previous, Ray r, int i)
        {
            if (previous.getDiffuseDepth() >= maxDiffuseDepth)
                return null;
            IntersectionState istate = previous.getIntersectionState();
            scene.trace(r, istate);
            return istate.hit() ? ShadingState.createFinalGatherState(previous, r, i) : null;
        }

        public Color getGlobalRadiance(ShadingState state)
        {
            if (giEngine == null)
                return Color.BLACK;
            return giEngine.getGlobalRadiance(state);
        }

        public Color getIrradiance(ShadingState state, Color diffuseReflectance)
        {
            // no gi engine, or we have already exceeded number of available bounces
            if (giEngine == null || state.getDiffuseDepth() >= maxDiffuseDepth)
                return Color.BLACK;
            return giEngine.getIrradiance(state, diffuseReflectance);
        }

        public void initLightSamples(ShadingState state)
        {
            foreach (LightSource l in lights)
                l.getSamples(state);
        }

        public void initCausticSamples(ShadingState state)
        {
            if (causticPhotonMap != null)
                causticPhotonMap.getSamples(state);
        }
    }
}