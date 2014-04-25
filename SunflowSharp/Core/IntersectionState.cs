using System;

namespace SunflowSharp.Core
{

    /**
     * This class is used to store ray/object intersections. It also provides
     * additional data to assist {@link AccelerationStructure} objects with
     * traversal.
     */
    public class IntersectionState
    {
        private static int MAX_STACK_SIZE = 64;
		public float time;
        public float u, v, w;
        public Instance instance;
        public int id;
		private StackNode[][] stacks = new StackNode[2][];
        public Instance current;
		public Int64 numEyeRays;
		public Int64 numShadowRays;
		public Int64 numReflectionRays;
		public Int64 numGlossyRays;
		public Int64 numRefractionRays;
		public Int64 numRays;


        /**
         * Traversal stack node, helps with tree-based {@link AccelerationStructure}
         * traversal.
         */
        public class StackNode
        {
            public int node;
            public float near;
            public float far;
        }

        /**
         * Initializes all traversal stacks.
         */
        public IntersectionState()
        {
            for (int i = 0; i < stacks.Length; i++) {
				stacks[i] = new StackNode[MAX_STACK_SIZE];
				for (int j = 0; j < stacks[i].Length; j++)
                	stacks[i][j] = new StackNode();
			}
        }

		/**
		* Returns the time at which the intersection should be calculated. This
		* will be constant for a given ray-tree. This value is guarenteed to be
		* between the camera's shutter open and shutter close time.
		*
		* @return time value
		*/
		public float getTime() {
			return time;
		}
		
		/**
         * Get stack object for tree based {@link AccelerationStructure}s.
         * 
         * @return array of stack nodes
         */
        public StackNode[] getStack()
        {
			return current == null ? stacks[0] : stacks[1];
        }

        /**
         * Checks to see if a hit has been recorded.
         * 
         * @return <code>true</code> if a hit has been recorded,
         *         <code>false</code> otherwise
         */
        public bool hit()
        {
            return instance != null;
        }

		/**
		 * Record an intersection with the specified primitive id. The parent object
		 * is assumed to be the current instance. The u and v parameters are used to
		 * pinpoint the location on the surface if needed.
		 *
		 * @param id primitive id of the intersected object
		 * @param u u surface paramater of the intersection point
		 * @param v v surface parameter of the intersection point
		 */
		public void setIntersection(int id) {
			instance = current;
			this.id = id;
		}
		/**
         * Record an intersection with the specified primitive id. The parent object
         * is assumed to be the current instance. The u and v parameters are used to
         * pinpoint the location on the surface if needed.
         * 
         * @param id primitive id of the intersected object
         * @param u u surface paramater of the intersection point
         * @param v v surface parameter of the intersection point
         */
        public void setIntersection(int id, float u, float v)
        {
            instance = current;
            this.id = id;
            this.u = u;
            this.v = v;
        }

		/**
		 * Record an intersection with the specified primitive id. The parent object
		 * is assumed to be the current instance. The u and v parameters are used to
		 * pinpoint the location on the surface if needed.
		 *
		 * @param id primitive id of the intersected object
		 * @param u u surface paramater of the intersection point
		 * @param v v surface parameter of the intersection point
		 */
		public void setIntersection(int id, float u, float v, float w) {
			instance = current;
			this.id = id;
			this.u = u;
			this.v = v;
			this.w = w;
		}
    }
}