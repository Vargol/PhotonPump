using System;
using SunflowSharp;
using SunflowSharp.Maths;

namespace SunflowSharp.Core
{

    public class InstanceList : PrimitiveList
    {
        private Instance[] instances;
		private Instance[] lights;

        public InstanceList()
        {
            instances = new Instance[0];
			clearLightSources();
        }

        public InstanceList(Instance[] instances)
        {
            this.instances = instances;
			clearLightSources();
		}

		public void addLightSourceInstances(Instance[] lights) {
			this.lights = lights;
		}
		
		public void clearLightSources() {
			lights = new Instance[0];
		}
		

        public float getPrimitiveBound(int primID, int i)
        {
			if (primID < instances.Length)
				return instances[primID].getBounds().getBound(i);
			else
				return lights[primID - instances.Length].getBounds().getBound(i);
		}

        public BoundingBox getWorldBounds(Matrix4 o2w)
        {
            BoundingBox bounds = new BoundingBox();
            foreach (Instance i in instances)
                bounds.include(i.getBounds());
			foreach (Instance i in lights)
				bounds.include(i.getBounds());
			return bounds;
        }

        public void intersectPrimitive(Ray r, int primID, IntersectionState state)
        {
			if (primID < instances.Length)
				instances[primID].intersect(r, state);
			else
				lights[primID - instances.Length].intersect(r, state);
		}

        public int getNumPrimitives()
        {
            return instances.Length+lights.Length;
        }

        public int getNumPrimitives(int primID)
        {
			return primID < instances.Length ? instances[primID].getNumPrimitives() : lights[primID - instances.Length].getNumPrimitives();
        }

        public void prepareShadingState(ShadingState state)
        {
            state.getInstance().prepareShadingState(state);
        }

        public bool Update(ParameterList pl, SunflowAPI api)
        {
            return true;
        }

        public PrimitiveList getBakingPrimitives()
        {
            return null;
        }
    }
}