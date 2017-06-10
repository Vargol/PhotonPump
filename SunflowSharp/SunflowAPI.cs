using System;
using System.Diagnostics;
using System.IO;
using SunflowSharp.Core;
using SunflowSharp.Core.Camera;
using SunflowSharp.Core.Parser;
using SunflowSharp.Core.Renderer;
using SunflowSharp.Image;
using SunflowSharp.Maths;
using SunflowSharp.Systems;

namespace SunflowSharp
{
    /**
     * This API gives a simple interface for creating scenes procedurally. This is
     * the main entry point to Sunflow. To use this class, extend from it and
     * implement the build method which may execute arbitrary code to create a
     * scene.
     */
    public class SunflowAPI
    {
        public static string VERSION = "0.07.3";
        public static string DEFAULT_OPTIONS = "::options";
		public int currentFrame { get; set;}

        private Scene scene;
        private SearchPath includeSearchPath;
        private SearchPath textureSearchPath;
        private ParameterList parameterList;
        private RenderObjectMap renderObjects;

        /**
         * This is a quick system test which verifies that the user has launched
         * Java properly.
         */
        public static void runSystemCheck()
        {
            long RECOMMENDED_MAX_SIZE = 800;
            long maxMb = maxMemory() / 1048576;
            if (maxMb < RECOMMENDED_MAX_SIZE)
                UI.printError(UI.Module.API, "Available memory is below {0} MB (found {1} MB only).\nPlease make sure you launched the program with the -Xmx command line options.", RECOMMENDED_MAX_SIZE, maxMb);
            string compiler = Environment.Version.ToString();
            //if (compiler == null || !(compiler.contains("HotSpot") && compiler.contains("Server")))
            //    UI.printError(UI.Module.API, "You do not appear to be running Sun's server JVM\nPerformance may suffer");
            UI.printDetailed(UI.Module.API, "Environment settings:");
            UI.printDetailed(UI.Module.API, "  * Max memory available : {0} MB", maxMb);
            UI.printDetailed(UI.Module.API, "  * Virtual machine name : {0}", compiler == null ? "<unknown" : compiler);
            UI.printDetailed(UI.Module.API, "  * Operating system     : {0}", Environment.OSVersion);
            UI.printDetailed(UI.Module.API, "  * CPU architecture     : {0}", "?");
        }
        static long maxMemory()
        {
            using (PerformanceCounter p = new PerformanceCounter("Memory", "Available Bytes"))
                return (long)p.NextValue();
        }
        /**
         * Creates an empty scene.
         */
        public SunflowAPI()
        {
			ByteUtil.InitByteUtil(); // ensure this done in the main thread.
			reset();
        }

        /**
         * Reset the state of the API completely. The object table is cleared, and
         * all search paths areset back to their default values.
         */
        public void reset()
        {
            scene = new Scene();
            includeSearchPath = new SearchPath("include");
            textureSearchPath = new SearchPath("texture");
            parameterList = new ParameterList();
            renderObjects = new RenderObjectMap();
            currentFrame = 1;
        }

		public void plugin(string type, string name, string code) {
			if (type.Equals("primitive"))
				PluginRegistry.primitivePlugins.registerPlugin(name, code);
			else if (type.Equals("tesselatable"))
				PluginRegistry.tesselatablePlugins.registerPlugin(name, code);
			else if (type.Equals("shader"))
				PluginRegistry.shaderPlugins.registerPlugin(name, code);
			else if (type.Equals("modifier"))
				PluginRegistry.modifierPlugins.registerPlugin(name, code);
			else if (type.Equals("camera_lens"))
				PluginRegistry.cameraLensPlugins.registerPlugin(name, code);
			else if (type.Equals("light"))
				PluginRegistry.lightSourcePlugins.registerPlugin(name, code);
			else if (type.Equals("accel"))
				PluginRegistry.accelPlugins.registerPlugin(name, code);
			else if (type.Equals("bucket_order"))
				PluginRegistry.bucketOrderPlugins.registerPlugin(name, code);
			else if (type.Equals("filter"))
				PluginRegistry.filterPlugins.registerPlugin(name, code);
			else if (type.Equals("gi_engine"))
				PluginRegistry.giEnginePlugins.registerPlugin(name, code);
			else if (type.Equals("caustic_photon_map"))
				PluginRegistry.causticPhotonMapPlugins.registerPlugin(name, code);
			else if (type.Equals("global_photon_map"))
				PluginRegistry.globalPhotonMapPlugins.registerPlugin(name, code);
			else if (type.Equals("image_sampler"))
				PluginRegistry.imageSamplerPlugins.registerPlugin(name, code);
			else if (type.Equals("parser"))
				PluginRegistry.parserPlugins.registerPlugin(name, code);
			else if (type.Equals("bitmap_reader"))
				PluginRegistry.bitmapReaderPlugins.registerPlugin(name, code);
			else if (type.Equals("bitmap_writer"))
				PluginRegistry.bitmapWriterPlugins.registerPlugin(name, code);
			else
				UI.printWarning(UI.Module.API, "Unrecognized plugin type: \"{0}\" - ignoring declaration of \"{0}\"", type, name);
		}

        /**
         * Declare a parameter with the specified name and value. This parameter
         * will be added to the currently active parameter list.
         * 
         * @param name parameter name
         * @param value parameter value
         */
        public void parameter(string name, string value)
        {
            parameterList.addstring(name, value);
        }

        /**
         * Declare a parameter with the specified name and value. This parameter
         * will be added to the currently active parameter list.
         * 
         * @param name parameter name
         * @param value parameter value
         */
        public void parameter(string name, bool value)
        {
            parameterList.addbool(name, value);
        }

        /**
         * Declare a parameter with the specified name and value. This parameter
         * will be added to the currently active parameter list.
         * 
         * @param name parameter name
         * @param value parameter value
         */
        public void parameter(string name, int value)
        {
            parameterList.addInteger(name, value);
        }

        /**
         * Declare a parameter with the specified name and value. This parameter
         * will be added to the currently active parameter list.
         * 
         * @param name parameter name
         * @param value parameter value
         */
        public void parameter(string name, float value)
        {
            parameterList.addFloat(name, value);
        }

        /**
         * Declare a parameter with the specified name and value. This parameter
         * will be added to the currently active parameter list.
         * 
         * @param name parameter name
         * @param value parameter value
         */
		public void parameter(string name, string colorspace, params float[] data) {
			try {
				parameterList.addColor(name, ColorFactory.createColor(colorspace, data));
			} catch (ColorFactory.ColorSpecificationException e) {
				UI.printError(UI.Module.API, "Unable to specify color: {0} - ignoring parameter \"{1}\"", e.Message, name);
			}
		}

        /**
         * Declare a parameter with the specified name and value. This parameter
         * will be added to the currently active parameter list.
         * 
         * @param name parameter name
         * @param value parameter value
         */
        public void parameter(string name, Point3 value)
        {
            parameterList.addPoints(name, ParameterList.InterpolationType.NONE, new float[] {
                value.x, value.y, value.z });
        }

        /**
         * Declare a parameter with the specified name and value. This parameter
         * will be added to the currently active parameter list.
         * 
         * @param name parameter name
         * @param value parameter value
         */
        public void parameter(string name, Vector3 value)
        {
            parameterList.addVectors(name, ParameterList.InterpolationType.NONE, new float[] {
                value.x, value.y, value.z });
        }

        /**
         * Declare a parameter with the specified name and value. This parameter
         * will be added to the currently active parameter list.
         * 
         * @param name parameter name
         * @param value parameter value
         */
		public void parameter(String name, Point2 value) 
		{
			parameterList.addTexCoords(name, ParameterList.InterpolationType.NONE, new float[] {
			value.x, value.y });
		}
		
		/**
         * Declare a parameter with the specified name and value. This parameter
         * will be added to the currently active parameter list.
         * 
         * @param name parameter name
         * @param value parameter value
         */
		public void parameter(string name, Matrix4 value)
        {
            parameterList.addMatrices(name, ParameterList.InterpolationType.NONE, value.asRowMajor());
        }

        /**
         * Declare a parameter with the specified name and value. This parameter
         * will be added to the currently active parameter list.
         * 
         * @param name parameter name
         * @param value parameter value
         */
        public void parameter(string name, int[] value)
        {
            parameterList.addIntegerArray(name, value);
        }

        /**
         * Declare a parameter with the specified name and value. This parameter
         * will be added to the currently active parameter list.
         * 
         * @param name parameter name
         * @param value parameter value
         */
        public void parameter(string name, string[] value)
        {
            parameterList.addstringArray(name, value);
        }

        /**
         * Declare a parameter with the specified name. The type may be one of the
         * follow: "float", "point", "vector", "texcoord", "matrix". The
         * interpolation determines how the parameter is to be interpreted over
         * surface (see {@link InterpolationType}). The data is specified in a
         * flattened float array.
         * 
         * @param name parameter name
         * @param type parameter data type
         * @param interpolation parameter interpolation mode
         * @param data raw floating point data
         */
        public void parameter(string name, string type, string interpolation, float[] data)
        {
            ParameterList.InterpolationType interp;
            try
            {
                interp = (ParameterList.InterpolationType)Enum.Parse(typeof(ParameterList.InterpolationType), interpolation, true);
            }
            catch (Exception e)
            {
                UI.printError(UI.Module.API, "Unknown interpolation type: {0} -- ignoring parameter \"{1}\"", interpolation, name);
                return;
            }
            if (type == "float")
                parameterList.addFloats(name, interp, data);
            else if (type == "point")
                parameterList.addPoints(name, interp, data);
            else if (type == "vector")
                parameterList.addVectors(name, interp, data);
            else if (type == "texcoord")
                parameterList.addTexCoords(name, interp, data);
            else if (type == "matrix")
                parameterList.addMatrices(name, interp, data);
            else
                UI.printError(UI.Module.API, "Unknown parameter type: {0} -- ignoring parameter \"{1}\"", type, name);
        }

        /**
         * Remove the specified render object. Note that this may cause the removal
         * of other objects which depended on it.
         * 
         * @param name name of the object to remove
         */
        public void remove(string name)
        {
            renderObjects.remove(name);
        }

        /**
         * Update the specfied object using the currently active parameter list. The
         * object is removed if the update fails to avoid leaving inconsistently set
         * objects in the list.
         * 
         * @param name name of the object to update
         * @return <code>true</code> if the update was succesfull, or
         *         <code>false</code> if the update failed
         */
        private bool update(string name)
        {
            bool success = renderObjects.update(name, parameterList, this);
            parameterList.clear(success);
            return success;
        }

        /**
         * Add the specified path to the list of directories which are searched
		 * automatically to resolve scene filenames or textures. Currently the
		 * supported searchpath types are: "include" and "texture". All other types
		 * will be ignored.
		 *
		 * @param path
		 */
		public void searchpath(string type, string path) 
		{
			if (type.Equals("include"))
				includeSearchPath.addSearchPath(path);
			else if (type.Equals("texture"))
				textureSearchPath.addSearchPath(path);
			else
				UI.printWarning(UI.Module.API, "Invalid searchpath type: \"{0}\"", type);
		}

        /**
         * Attempts to resolve the specified filename by checking it against the
         * texture search path.
         * 
         * @param filename filename
         * @return a path which matches the filename, or filename if no matches are
         *         found
         */
        public string resolveTextureFilename(string filename)
        {
            return textureSearchPath.resolvePath(filename);
        }

        /**
         * Attempts to resolve the specified filename by checking it against the
         * include search path.
         * 
         * @param filename filename
         * @return a path which matches the filename, or filename if no matches are
         *         found
         */
        public string resolveIncludeFilename(string filename)
        {
            return includeSearchPath.resolvePath(filename);
        }

        /**
         * Defines a shader with a given name. If the shader type name is left
         * <code>null</code>, the shader with the given name will be updated (if
         * it exists).
         * 
         * @param name a unique name given to the shader
         * @param shaderType a shader plugin type
         */
        public void shader(string name, string shaderType)
        {
			if (!isIncremental(shaderType))
            {
                // we are declaring a shader for the first time
                if (renderObjects.has(name))
                {
                    UI.printError(UI.Module.API, "Unable to declare shader \"{0}\", name is already in use", name);
                    parameterList.clear(true);
                    return;
                }
				IShader shader = PluginRegistry.shaderPlugins.createObject(shaderType);
				if (shader == null) 
				{
					UI.printError(UI.Module.API, "Unable to create shader of type \"{0}\"", shaderType);
					return;
				}     
                renderObjects.put(name, shader);
            }
            // update existing shader (only if it is valid)
            if (lookupShader(name) != null)
                update(name);
            else
            {
                UI.printError(UI.Module.API, "Unable to update shader \"{0}\" - shader object was not found", name);
                parameterList.clear(true);
            }
        }

        /**
         * Defines a modifier with a given name. If the modifier type name is left
         * <code>null</code>, the modifier with the given name will be updated
         * (if it exists).
         * 
         * @param name a unique name given to the modifier
         * @param modifierType a modifier plugin type name
         */
        public void modifier(string name, string modifierType)
        {
			if (!isIncremental(modifierType))
            {
                // we are declaring a shader for the first time
                if (renderObjects.has(name))
                {
                    UI.printError(UI.Module.API, "Unable to declare modifier \"{0}\", name is already in use", name);
                    parameterList.clear(true);
                    return;
                }
				Modifier modifier = PluginRegistry.modifierPlugins.createObject(modifierType);
				if (modifier == null) 
				{
					UI.printError(UI.Module.API, "Unable to create modifier of type \"{0}\"", modifierType);
					return;
				}

                renderObjects.put(name, modifier);
            }
            // update existing shader (only if it is valid)
            if (lookupModifier(name) != null)
                update(name);
            else
            {
                UI.printError(UI.Module.API, "Unable to update modifier \"{0}\" - modifier object was not found", name);
                parameterList.clear(true);
            }
        }

		/**
		 * Defines a geometry with a given name. The geometry is built from the
		 * specified type. Note that geometries may be created from
		 * {@link Tesselatable} objects or {@link PrimitiveList} objects. This means
		 * that two seperate plugin lists will be searched for the geometry type.
		 * {@link Tesselatable} objects are search first. If the type name is left
		 * <code>null</code>, the geometry with the given name will be updated
		 * (if it exists).
		 *
		 * @param name a unique name given to the geometry
		 * @param typeName a tesselatable or primitive plugin type name
		 */
		public void geometry(string name, string typeName) {
			if (!isIncremental(typeName)) {
				// we are declaring a geometry for the first time
				if (renderObjects.has(name)) {
					UI.printError(UI.Module.API, "Unable to declare geometry \"{0}\", name is already in use", name);
					parameterList.clear(true);
					return;
				}
				// check tesselatable first
				if (PluginRegistry.tesselatablePlugins.hasType(typeName)) {
					ITesselatable tesselatable = PluginRegistry.tesselatablePlugins.createObject(typeName);
					if (tesselatable == null) {
						UI.printError(UI.Module.API, "Unable to create tesselatable object of type \"{0}\"", typeName);
						return;
					}
					renderObjects.put(name, tesselatable);
				} else {
					PrimitiveList primitives = PluginRegistry.primitivePlugins.createObject(typeName);
					if (primitives == null) {
						UI.printError(UI.Module.API, "Unable to create primitive of type \"{0}\"", typeName);
						return;
					}
					renderObjects.put(name, primitives);
				}
			}
			if (lookupGeometry(name) != null)
				update(name);
			else {
				UI.printError(UI.Module.API, "Unable to update geometry \"{0}\" - geometry object was not found", name);
				parameterList.clear(true);
			}
		}

        /**
         * Instance the specified geometry into the scene. If geoname is
         * <code>null</code>, the specified instance object will be updated (if
         * it exists). In order to change the instancing relationship of an existing
		 * instance, you should use the "geometry" string attribute.
         * 
         * @param name instance name
         * @param geoname name of the geometry to instance
         */
        public void instance(string name, string geoname)
        {
			if (!isIncremental(geoname))
            {
                // we are declaring this instance for the first time
                if (renderObjects.has(name))
                {
                    UI.printError(UI.Module.API, "Unable to declare instance \"{0}\", name is already in use", name);
                    parameterList.clear(true);
                    return;
                }
                parameter("geometry", geoname);
                renderObjects.put(name, new Instance());
            }
            if (lookupInstance(name) != null)
                update(name);
            else
            {
                UI.printError(UI.Module.API, "Unable to update instance \"{0}\" - instance object was not found", name);
                parameterList.clear(true);
            }
        }

		/**
		 * Defines a light source with a given name. If the light type name is left
		 * <code>null</code>, the light source with the given name will be
		 * updated (if it exists).
		 *
		 * @param name a unique name given to the light source
		 * @param lightType a light source plugin type name
		 */
        public void light(string name, string lightType)
        {
			if (!isIncremental(lightType))
            {
                // we are declaring this light for the first time
                if (renderObjects.has(name))
                {
                    UI.printError(UI.Module.API, "Unable to declare light \"{0}\", name is already in use", name);
                    parameterList.clear(true);
                    return;
                }
				LightSource light = PluginRegistry.lightSourcePlugins.createObject(lightType);
				if (light == null) {
					UI.printError(UI.Module.API, "Unable to create light source of type \"{0}\"", lightType);
					return;
				}
                renderObjects.put(name, light);
            }
            if (lookupLight(name) != null)
                update(name);
            else
            {
                UI.printError(UI.Module.API, "Unable to update instance \"{0}\" - instance object was not found", name);
                parameterList.clear(true);
            }
        }

        /**
         * Defines a camera with a given name. The camera is built from the
		 * specified camera lens type plugin. If the lens type name is left
	     * <code>null</code>, the camera with the given name will be updated (if
		 * it exists). It is not currently possible to change the lens of a camera
	     * after it has been created.
         * 
         * @param name camera name
         * @param lensparam lensType a camera lens plugin type name
         */
        public void camera(string name, string lensType)
        {
			if (!isIncremental(lensType))
            {
                // we are declaring this camera for the first time
                if (renderObjects.has(name))
                {
                    UI.printError(UI.Module.API, "Unable to declare camera \"{0}\", name is already in use", name);
                    parameterList.clear(true);
                    return;
                }
				CameraLens lens = PluginRegistry.cameraLensPlugins.createObject(lensType);
				if (lens == null) 
				{
					UI.printError(UI.Module.API, "Unable to create a camera lens of type \"{0}\"", lensType);
					return;
				}
                renderObjects.put(name, new CameraBase(lens));
            }
            // update existing shader (only if it is valid)
            if (lookupCamera(name) != null)
                update(name);
            else
            {
                UI.printError(UI.Module.API, "Unable to update camera \"{0}\" - camera object was not found", name);
                parameterList.clear(true);
            }
        }

        /**
         * Defines an option object to hold the current parameters. If the object
         * already exists, the values will simply override previous ones.
         * 
         * @param name
         */
        public void options(string name)
        {
            if (lookupOptions(name) == null)
            {
                if (renderObjects.has(name))
                {
                    UI.printError(UI.Module.API, "Unable to declare options \"{0}\", name is already in use", name);
                    parameterList.clear(true);
                    return;
                }
                renderObjects.put(name, new Options());
            }
            Debug.Assert(lookupOptions(name) != null);
            update(name);
        }

        /**
         * Retrieve a geometry object by its name, or <code>null</code> if no
         * geometry was found, or if the specified object is not a geometry.
         * 
         * @param name geometry name
         * @return the geometry object associated with that name
         */
        public Geometry lookupGeometry(string name)
        {
            return renderObjects.lookupGeometry(name);
        }

		private bool isIncremental(string typeName) {
			return typeName == null || typeName.Equals("incremental");
		}

        /**
         * Retrieve an instance object by its name, or <code>null</code> if no
         * instance was found, or if the specified object is not an instance.
         * 
         * @param name instance name
         * @return the instance object associated with that name
         */
        private Instance lookupInstance(string name)
        {
            return renderObjects.lookupInstance(name);
        }

        /**
         * Retrieve a shader object by its name, or <code>null</code> if no shader
         * was found, or if the specified object is not a shader.
         * 
         * @param name camera name
         * @return the camera object associate with that name
         */
        private CameraBase lookupCamera(string name)
        {
            return renderObjects.lookupCamera(name);
        }

        private Options lookupOptions(string name)
        {
            return renderObjects.lookupOptions(name);
        }

        /**
         * Retrieve a shader object by its name, or <code>null</code> if no shader
         * was found, or if the specified object is not a shader.
         * 
         * @param name shader name
         * @return the shader object associated with that name
         */
        public IShader lookupShader(string name)
        {
            return renderObjects.lookupShader(name);
        }

        /**
         * Retrieve a modifier object by its name, or <code>null</code> if no
         * modifier was found, or if the specified object is not a modifier.
         * 
         * @param name modifier name
         * @return the modifier object associated with that name
         */
        public Modifier lookupModifier(string name)
        {
            return renderObjects.lookupModifier(name);
        }

		/**
	     * Retrieve a light object by its name, or <code>null</code> if no shader
	     * was found, or if the specified object is not a light.
	     * 
	     * @param name light name
	     * @return the light object associated with that name
	     */
		private LightSource lookupLight(string name) 
		{
			return renderObjects.lookupLight(name);
		}

		/**
         * Sets a global shader override to the specified shader name. If the shader
         * is not found, the overriding is disabled. The second parameter controls
         * whether the override applies to the photon tracing process.
         * 
         * @param name shader name
         * @param photonOverride apply override to photon tracing phase
         */
        public void shaderOverride(string name, bool photonOverride)
        {
            scene.setShaderOverride(lookupShader(name), photonOverride);
        }

        /**
         * Render using the specified options and the specified display. If the
         * specified options do not exist - defaults will be used.
         * 
         * @param optionsName name of the {@link RenderObject} which contains the
         *            options
         * @param display display object
         */
        public void render(string optionsName, IDisplay display)
        {
            if (string.IsNullOrEmpty(optionsName))
                optionsName = "::options";
            renderObjects.updateScene(scene);
            Options opt = lookupOptions(optionsName);
            if (opt == null)
                opt = new Options();
            scene.setCamera(lookupCamera(opt.getstring("camera", null)));

			// shader override
			String shaderOverrideName = opt.getstring("override.shader", "none");
			bool overridePhotons = opt.getbool("override.photons", false);
			
			if (shaderOverrideName.Equals("none"))
				scene.setShaderOverride(null, false);
			else {
				IShader shader = lookupShader(shaderOverrideName);
				if (shader == null)
					UI.printWarning(UI.Module.API, "Unable to find shader \"{0}\" for override, disabling", shaderOverrideName);
				scene.setShaderOverride(shader, overridePhotons);
			}

			// baking
            string bakingInstanceName = opt.getstring("baking.instance", null);
            if (bakingInstanceName != null)
            {
                Instance bakingInstance = lookupInstance(bakingInstanceName);
                if (bakingInstance == null)
                {
                    UI.printError(UI.Module.API, "Unable to bake instance \"{0}\" - not found", bakingInstanceName);
                    return;
                }
                scene.setBakingInstance(bakingInstance);
            }
            else
                scene.setBakingInstance(null);

			ImageSampler sampler = PluginRegistry.imageSamplerPlugins.createObject(opt.getstring("sampler", "bucket"));
            scene.render(opt, sampler, display);
        }

        /**
         * Parse the specified filename. The include paths are searched first. The
         * contents of the file are simply added to the active scene. This allows to
         * break up a scene into parts, even across file formats. The appropriate
         * parser is chosen based on file extension.
         * 
         * @param filename filename to load
         * @return <code>true</code> upon sucess, <code>false</code> if an error
         *         occured.
         */
        public bool include(string filename)
        {
            if (filename == null)
                return false;
            filename = includeSearchPath.resolvePath(filename);
			string extension = FileUtils.getExtension(filename);
			SceneParser parser = PluginRegistry.parserPlugins.createObject(extension);
			if (parser == null) 
			{
				UI.printError(UI.Module.API, "Unable to find a suitable parser for: \"{0}\"(extension: {1})", filename, extension);
				return false;
            }
            string currentFolder = Path.GetDirectoryName(filename);//new File(filename).getAbsoluteFile().getParentFile().getAbsolutePath();
            includeSearchPath.addSearchPath(currentFolder);
            textureSearchPath.addSearchPath(currentFolder);
            return parser.parse(filename, this);
        }

        /**
         * Retrieve the bounding box of the scene. This method will be valid only
         * after a first call to {@link #render(string, Display)} has been made.
         */
        public BoundingBox getBounds()
        {
            return scene.getBounds();
        }

        /**
         * This method does nothing, but may be overriden to create scenes
         * procedurally.
         */
        public virtual void build()
        {
        }

        /**
         * Create an API object from the specified file. Java files are read by
         * Janino and are expected to implement a build method (they implement a
         * derived class of SunflowAPI. The build method is called if the code
         * compiles succesfully. Other files types are handled by the parse method.
         * 
         * @param filename filename to load
         * @return a valid SunflowAPI object or <code>null</code> on failure
         */
        public static SunflowAPI create(string filename, int frameNumber)
        {
            if (filename == null)
                return new SunflowAPI();
            SunflowAPI api = null;
            if (filename.EndsWith(".java"))
            {
                Timer t = new Timer();
                UI.printInfo(UI.Module.API, "Compiling \"" + filename + "\" ...");
                t.start();
                try
                {
                    //FileInputStream stream = new FileInputStream(filename);
                    api = null;//(SunflowAPI) ClassBodyEvaluator.createFastClassBodyEvaluator(new Scanner(filename, stream), SunflowAPI.class, ClassLoader.getSystemClassLoader());
                    //fixme: the dynamic loading
                    //stream.close();
                }
                catch (Exception e)
                {
                    UI.printError(UI.Module.API, "Could not compile: \"{0}\"", filename);
                    UI.printError(UI.Module.API, "{0}", e);
                    return null;
                }
                t.end();
                UI.printInfo(UI.Module.API, "Compile time: " + t.ToString());
                if (api != null)
                {
                    string currentFolder = Path.GetDirectoryName(filename);//new File(filename).getAbsoluteFile().getParentFile().getAbsolutePath();
                    api.includeSearchPath.addSearchPath(currentFolder);
                    api.textureSearchPath.addSearchPath(currentFolder);
                }
                UI.printInfo(UI.Module.API, "Build script running ...");
                t.start();
                api.currentFrame = frameNumber;
                api.build();
                t.end();
                UI.printInfo(UI.Module.API, "Build script time: {0}", t.ToString());
            }
            else
            {
                api = new SunflowAPI();
                api = api.include(filename) ? api : null;
            }
            return api;
        }

        /**
         * Compile the specified code string via Janino. The code must implement a
         * build method as described above. The build method is not called on the
         * output, it is up the caller to do so.
         * 
         * @param code java code string
         * @return a valid SunflowAPI object upon succes, <code>null</code>
         *         otherwise.
         */
        public static SunflowAPI compile(string code)
        {
            try
            {
                Timer t = new Timer();
                t.start();
                SunflowAPI api = null;//(SunflowAPI) ClassBodyEvaluator.createFastClassBodyEvaluator(new Scanner(null, new stringReader(code)), SunflowAPI.class, (ClassLoader) null);
                //fixme: the dynamic loading
                t.end();
                UI.printInfo(UI.Module.API, "Compile time: {0}", t.ToString());
                return api;
            }
            catch (Exception e)
            {
                UI.printError(UI.Module.API, "{0}", e);
                return null;
            }
        }

        /**
         * Read the value of the current frame. This value is intended only for
         * procedural animation creation. It is not used by the Sunflow core in
         * anyway. The default value is 1.
         * 
         * @return current frame number
         *
        public int currentFrame()
        {
            return currentFrame;
        }

        /**
         * Set the value of the current frame. This value is intended only for
         * procedural animation creation. It is not used by the Sunflow core in
         * anyway. The default value is 1.
         * 
         * @param currentFrame current frame number
         *
        public void currentFrame(int currentFrame)
        {
            this.currentFrame = currentFrame;
        }
        */
    }
}