using System;
using System.Collections.Generic;
using System.Diagnostics;
using SunflowSharp.Core;
using SunflowSharp.Systems;
using SunflowSharp.Systems.Ui;

namespace SunflowSharp
{
    public class RenderObjectMap
    {
        private Dictionary<string, RenderObjectHandle> renderObjects;
        private bool rebuildInstanceList;
        private bool rebuildLightList;

        public enum RenderObjectType
        {
            UNKNOWN, SHADER, MODIFIER, GEOMETRY, INSTANCE, LIGHT, CAMERA, OPTIONS
        }

        public RenderObjectMap()
        {
            renderObjects = new Dictionary<string, RenderObjectHandle>();
            rebuildInstanceList = rebuildLightList = false;
        }

        public bool has(string name)
        {
            return renderObjects.ContainsKey(name);
        }

        public void remove(string name)
        {
            RenderObjectHandle obj = renderObjects[name];
            if (obj == null)
            {
                UI.printWarning(UI.Module.API, "Unable to remove \"%s\" - object was not defined yet");
                return;
            }
            UI.printDetailed(UI.Module.API, "Removing object \"%s\"", name);
            renderObjects.Remove(name);
            // scan through all objects to make sure we don't have any
            // references to the old object still around
            switch (obj.type)
            {
                case RenderObjectType.SHADER:
                    IShader s = obj.getShader();
                    foreach (KeyValuePair<string, RenderObjectHandle> e in renderObjects)
                    {
                        Instance i = e.Value.getInstance();
                        if (i != null)
                        {
                            UI.printWarning(UI.Module.API, "Removing shader \"%s\" from instance \"%s\"", name, e.Key);
                            i.removeShader(s);
                        }
                    }
                    break;
                case RenderObjectType.MODIFIER:
                    Modifier m = obj.getModifier();
                    foreach (KeyValuePair<string, RenderObjectHandle> e in renderObjects)
                    {
                        Instance i = e.Value.getInstance();
                        if (i != null)
                        {
                            UI.printWarning(UI.Module.API, "Removing modifier \"%s\" from instance \"%s\"", name, e.Key);
                            i.removeModifier(m);
                        }
                    }
                    break;
                case RenderObjectType.GEOMETRY:
                    {
                        Geometry g = obj.getGeometry();
                        foreach (KeyValuePair<string, RenderObjectHandle> e in renderObjects)
                        {
                            Instance i = e.Value.getInstance();
                            if (i != null && i.hasGeometry(g))
                            {
                                UI.printWarning(UI.Module.API, "Removing instance \"%s\" because it referenced geometry \"%s\"", e.Key, name);
                                remove(e.Key);
                            }
                        }
                        break;
                    }
                case RenderObjectType.INSTANCE:
                    rebuildInstanceList = true;
                    break;
                case RenderObjectType.LIGHT:
                    rebuildLightList = true;
                    break;
                default:
                    // no dependencies
                    break;
            }
        }

        public bool update(string name, ParameterList pl, SunflowAPI api)
        {
            RenderObjectHandle obj = renderObjects[name];
            bool success;
            if (obj == null)
            {
                UI.printError(UI.Module.API, "Unable to update \"{0}\" - object was not defined yet", name);
                success = false;
            }
            else
            {
                UI.printDetailed(UI.Module.API, "Updating {0} object \"{1}\"", obj.typeName(), name);
                success = obj.update(pl, api);
                if (!success)
                {
                    UI.printError(UI.Module.API, "Unable to update \"{0}\" - removing", name);
                    remove(name);
                }
                else
                {
                    switch (obj.type)
                    {
                        case RenderObjectType.GEOMETRY:
                        case RenderObjectType.INSTANCE:
                            rebuildInstanceList = true;
                            break;
                        case RenderObjectType.LIGHT:
                            rebuildLightList = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            return success;
        }

        public void updateScene(Scene scene)
        {
            if (rebuildInstanceList)
            {
                UI.printInfo(UI.Module.API, "Building scene instance list for rendering ...");
                int numInfinite = 0, numInstance = 0;
                foreach (KeyValuePair<string, RenderObjectHandle> e in renderObjects)
                {
                    Instance i = e.Value.getInstance();
                    if (i != null)
                    {
                        i.updateBounds();
                        if (i.getBounds() == null)
                            numInfinite++;
						else if (!i.getBounds().isEmpty())
                            numInstance++;
						else
							UI.printWarning(UI.Module.API, "Ignoring empty instance: \"{0}\"", e.Key);
                    }
                }
                Instance[] infinite = new Instance[numInfinite];
                Instance[] instance = new Instance[numInstance];
                numInfinite = numInstance = 0;
                foreach (KeyValuePair<string, RenderObjectHandle> e in renderObjects)
                {
                    Instance i = e.Value.getInstance();
                    if (i != null)
                    {
                        if (i.getBounds() == null)
                        {
                            infinite[numInfinite] = i;
                            numInfinite++;
                        }
						else if (!i.getBounds().isEmpty()) 
                        {
                            instance[numInstance] = i;
                            numInstance++;
                        }
                    }
                }
                scene.setInstanceLists(instance, infinite);
                rebuildInstanceList = false;
            }
            if (rebuildLightList)
            {
                UI.printInfo(UI.Module.API, "Building scene light list for rendering ...");
                List<LightSource> lightList = new List<LightSource>();
                foreach (KeyValuePair<string, RenderObjectHandle> e in renderObjects)
                {
                    LightSource light = e.Value.getLight();
                    if (light != null)
                        lightList.Add(light);

                }
                scene.setLightList(lightList.ToArray());
                rebuildLightList = false;
            }
        }

        public void put(string name, IShader shader)
        {
            renderObjects[name] = new RenderObjectHandle(shader);
        }

        public void put(string name, Modifier modifier)
        {
            renderObjects[name] = new RenderObjectHandle(modifier);
        }

        public void put(string name, PrimitiveList primitives)
        {
            renderObjects[name] = new RenderObjectHandle(primitives);
        }

        public void put(string name, ITesselatable tesselatable)
        {
            renderObjects[name] = new RenderObjectHandle(tesselatable);
        }

        public void put(string name, Instance instance)
        {
            renderObjects[name] = new RenderObjectHandle(instance);
        }

        public void put(string name, LightSource light)
        {
            renderObjects[name] = new RenderObjectHandle(light);
        }

        public void put(string name, CameraBase camera)
        {
            renderObjects[name] = new RenderObjectHandle(camera);
        }

        public void put(string name, Options options)
        {
            renderObjects[name] = new RenderObjectHandle(options);
        }

        public Geometry lookupGeometry(string name)
        {
            if (name == null)
                return null;
            RenderObjectHandle handle = renderObjects.ContainsKey(name) ? renderObjects[name] : null;
            return (handle == null) ? null : handle.getGeometry();
        }

        public Instance lookupInstance(string name)
        {
            if (name == null)
                return null;
            RenderObjectHandle handle = renderObjects.ContainsKey(name) ? renderObjects[name] : null;
            return (handle == null) ? null : handle.getInstance();
        }

        public CameraBase lookupCamera(string name)
        {
            if (name == null)
                return null;
            RenderObjectHandle handle = renderObjects.ContainsKey(name) ? renderObjects[name] : null;
            return (handle == null) ? null : handle.getCamera();
        }

        public Options lookupOptions(string name)
        {
            if (name == null)
                return null;
            RenderObjectHandle handle = renderObjects.ContainsKey(name) ? renderObjects[name] : null;
            return (handle == null) ? null : handle.getOptions();
        }

        public IShader lookupShader(string name)
        {
            if (name == null)
                return null;
            RenderObjectHandle handle = renderObjects.ContainsKey(name) ? renderObjects[name] : null;
            return (handle == null) ? null : handle.getShader();
        }

        public Modifier lookupModifier(string name)
        {
            if (name == null)
                return null;
            RenderObjectHandle handle = renderObjects.ContainsKey(name) ? renderObjects[name] : null;
            return (handle == null) ? null : handle.getModifier();
        }

        public LightSource lookupLight(string name)
        {
            if (name == null)
                return null;
            RenderObjectHandle handle = renderObjects.ContainsKey(name) ? renderObjects[name] : null;
            return (handle == null) ? null : handle.getLight();
        }

        public class RenderObjectHandle
        {
            public RenderObject obj;
            public RenderObjectType type;

            public RenderObjectHandle(IShader shader)
            {
                obj = shader;
                type = RenderObjectType.SHADER;
            }

            public RenderObjectHandle(Modifier modifier)
            {
                obj = modifier;
                type = RenderObjectType.MODIFIER;
            }

            public RenderObjectHandle(ITesselatable tesselatable)
            {
                obj = new Geometry(tesselatable);
                type = RenderObjectType.GEOMETRY;
            }

            public RenderObjectHandle(PrimitiveList prims)
            {
                obj = new Geometry(prims);
                type = RenderObjectType.GEOMETRY;
            }

            public RenderObjectHandle(Instance instance)
            {
                obj = instance;
                type = RenderObjectType.INSTANCE;
            }

            public RenderObjectHandle(LightSource light)
            {
                obj = light;
                type = RenderObjectType.LIGHT;
            }

            public RenderObjectHandle(CameraBase camera)
            {
                obj = camera;
                type = RenderObjectType.CAMERA;
            }

            public RenderObjectHandle(Options options)
            {
                obj = options;
                type = RenderObjectType.OPTIONS;
            }

            public bool update(ParameterList pl, SunflowAPI api)
            {
                return obj.update(pl, api);
            }

            public string typeName()
            {
                return type.ToString().ToLower();
            }

            public IShader getShader()
            {
                return (type == RenderObjectType.SHADER) ? (IShader)obj : null;
            }

            public Modifier getModifier()
            {
                return (type == RenderObjectType.MODIFIER) ? (Modifier)obj : null;
            }

            public Geometry getGeometry()
            {
                return (type == RenderObjectType.GEOMETRY) ? (Geometry)obj : null;
            }

            public Instance getInstance()
            {
                return (type == RenderObjectType.INSTANCE) ? (Instance)obj : null;
            }

            public LightSource getLight()
            {
                return (type == RenderObjectType.LIGHT) ? (LightSource)obj : null;
            }

            public CameraBase getCamera()
            {
                return (type == RenderObjectType.CAMERA) ? (CameraBase)obj : null;
            }

            public Options getOptions()
            {
                return (type == RenderObjectType.OPTIONS) ? (Options)obj : null;
            }
        }

    }
}