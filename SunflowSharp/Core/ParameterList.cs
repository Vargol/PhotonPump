using System;
using System.Collections.Generic;
using SunflowSharp.Image;
using SunflowSharp.Maths;
using SunflowSharp.Systems;
using SunflowSharp.Systems.Ui;

namespace SunflowSharp.Core
{

    /**
     * This class holds a list of "parameters". These are defined and then passed
     * onto rendering objects through the API. They can hold arbitrary typed and
     * named variables as a unified way of getting data into user objects.
     */
    public class ParameterList
    {
        public Dictionary<string, Parameter> list;//FastHashMap<string, Parameter> list;
        private int numVerts, numFaces, numFaceVerts;

        public enum ParameterType
        {
            STRING, INT, BOOL, FLOAT, POINT, VECTOR, TEXCOORD, MATRIX, COLOR
        }

        public enum InterpolationType
        {
            NONE, FACE, VERTEX, FACEVARYING
        }

        /**
         * Creates an empty ParameterList.
         */
        public ParameterList()
        {
            list = new Dictionary<string, Parameter>();
            numVerts = numFaces = numFaceVerts = 0;
        }

        /**
         * Clears the list of all its members. If some members were never used, a
         * warning will be printed to remind the user something may be wrong.
         */
        public void clear(bool showUnused)
        {
            if (showUnused)
            {
                foreach (KeyValuePair<string, Parameter> e in list)
                {
                    if (!e.Value.Checked)
                        UI.printWarning(UI.Module.API, "Unused parameter: {0} - {1}", e.Key, e.Value);
                }
            }
            list.Clear();
            numVerts = numFaces = numFaceVerts = 0;
        }

        /**
         * Setup how many faces should be used to check member count on "face"
         * interpolated parameters.
         * 
         * @param numFaces number of faces
         */
        public void setFaceCount(int numFaces)
        {
            this.numFaces = numFaces;
        }

        /**
         * Setup how many vertices should be used to check member count of "vertex"
         * interpolated parameters.
         * 
         * @param numVerts number of vertices
         */
        public void setVertexCount(int numVerts)
        {
            this.numVerts = numVerts;
        }

        /**
         * Setup how many "face-vertices" should be used to check member count of
         * "facevarying" interpolated parameters. This should be equal to the sum of
         * the number of vertices on each face.
         * 
         * @param numFaceVerts number of "face-vertices"
         */
        public void setFaceVertexCount(int numFaceVerts)
        {
            this.numFaceVerts = numFaceVerts;
        }

        /**
         * Add the specified string as a parameter. <code>null</code> values are
         * not permitted.
         * 
         * @param name parameter name
         * @param value parameter value
         */
        public void addstring(string name, string value)
        {
            add(name, new Parameter(value));
        }

        /**
         * Add the specified integer as a parameter. <code>null</code> values are
         * not permitted.
         * 
         * @param name parameter name
         * @param value parameter value
         */
        public void addInteger(string name, int value)
        {
            add(name, new Parameter(value));
        }

        /**
         * Add the specified bool as a parameter. <code>null</code> values are
         * not permitted.
         * 
         * @param name parameter name
         * @param value parameter value
         */
        public void addbool(string name, bool value)
        {
            add(name, new Parameter(value));
        }

        /**
         * Add the specified float as a parameter. <code>null</code> values are
         * not permitted.
         * 
         * @param name parameter name
         * @param value parameter value
         */
        public void addFloat(string name, float value)
        {
            add(name, new Parameter(value));
        }

        /**
         * Add the specified color as a parameter. <code>null</code> values are
         * not permitted.
         * 
         * @param name parameter name
         * @param value parameter value
         */
        public void addColor(string name, Color value)
        {
            if (value == null)
                throw new Exception("Value is null");
            add(name, new Parameter(value));
        }

        /**
         * Add the specified array of integers as a parameter. <code>null</code>
         * values are not permitted.
         * 
         * @param name parameter name
         * @param array parameter value
         */
        public void addIntegerArray(string name, int[] array)
        {
            if (array == null)
                throw new Exception("Value is null");
            add(name, new Parameter(array));
        }

        /**
         * Add the specified array of integers as a parameter. <code>null</code>
         * values are not permitted.
         * 
         * @param name parameter name
         * @param array parameter value
         */
        public void addstringArray(string name, string[] array)
        {
            if (array == null)
                throw new Exception("Value is null");
            add(name, new Parameter(array));
        }

        /**
         * Add the specified floats as a parameter. <code>null</code> values are
         * not permitted.
         * 
         * @param name parameter name
         * @param interp interpolation type
         * @param data parameter value
         */
        public void addFloats(string name, InterpolationType interp, float[] data)
        {
            if (data == null)
            {
                UI.printError(UI.Module.API, "Cannot create float parameter {0} -- invalid data Length", name);
                return;
            }
            add(name, new Parameter(ParameterType.FLOAT, interp, data));
        }

        /**
         * Add the specified points as a parameter. <code>null</code> values are
         * not permitted.
         * 
         * @param name parameter name
         * @param interp interpolation type
         * @param data parameter value
         */
        public void addPoints(string name, InterpolationType interp, float[] data)
        {
            if (data == null || data.Length % 3 != 0)
            {
                UI.printError(UI.Module.API, "Cannot create point parameter {0} -- invalid data Length", name);
                return;
            }
            add(name, new Parameter(ParameterType.POINT, interp, data));
        }

        /**
         * Add the specified vectors as a parameter. <code>null</code> values are
         * not permitted.
         * 
         * @param name parameter name
         * @param interp interpolation type
         * @param data parameter value
         */

        public void addVectors(string name, InterpolationType interp, float[] data)
        {
            if (data == null || data.Length % 3 != 0)
            {
                UI.printError(UI.Module.API, "Cannot create vector parameter {0} -- invalid data Length", name);
                return;
            }
            add(name, new Parameter(ParameterType.VECTOR, interp, data));
        }

        /**
         * Add the specified texture coordinates as a parameter. <code>null</code>
         * values are not permitted.
         * 
         * @param name parameter name
         * @param interp interpolation type
         * @param data parameter value
         */
        public void addTexCoords(string name, InterpolationType interp, float[] data)
        {
            if (data == null || data.Length % 2 != 0)
            {
                UI.printError(UI.Module.API, "Cannot create texcoord parameter {0} -- invalid data Length", name);
                return;
            }
            add(name, new Parameter(ParameterType.TEXCOORD, interp, data));
        }

        /**
         * Add the specified matrices as a parameter. <code>null</code> values are
         * not permitted.
         * 
         * @param name parameter name
         * @param interp interpolation type
         * @param data parameter value
         */
        public void addMatrices(string name, InterpolationType interp, float[] data)
        {
            if (data == null || data.Length % 16 != 0)
            {
                UI.printError(UI.Module.API, "Cannot create matrix parameter{0} -- invalid data Length", name);
                return;
            }
            add(name, new Parameter(ParameterType.MATRIX, interp, data));
        }

        private void add(string name, Parameter param)
        {
            if (name == null)
                UI.printError(UI.Module.API, "Cannot declare parameter with null name");
            if (list.ContainsKey(name))//list.put(name, param) != null)
                UI.printWarning(UI.Module.API, "Parameter {0} was already defined -- overwriting", name);
            list[name] = param;
        }

        /**
         * Get the specified string parameter from this list.
         * 
         * @param name name of the parameter
         * @param defaultValue value to return if not found
         * @return the value of the parameter specified or default value if not
         *         found
         */
        public string getstring(string name, string defaultValue)
        {
            Parameter p;
            if (list.TryGetValue(name, out p))
                if (isValidParameter(name, ParameterType.STRING, InterpolationType.NONE, 1, p))
                    return p.getstringValue();
            return defaultValue;
        }

        /**
         * Get the specified string array parameter from this list.
         * 
         * @param name name of the parameter
         * @param defaultValue value to return if not found
         * @return the value of the parameter specified or default value if not
         *         found
         */
        public string[] getstringArray(string name, string[] defaultValue)
        {
            Parameter p;
            if (list.TryGetValue(name, out p))
                if (isValidParameter(name, ParameterType.STRING, InterpolationType.NONE, -1, p))
                    return p.getstrings();
            return defaultValue;
        }

        /**
         * Get the specified integer parameter from this list.
         * 
         * @param name name of the parameter
         * @param defaultValue value to return if not found
         * @return the value of the parameter specified or default value if not
         *         found
         */
        public int getInt(string name, int defaultValue)
        {
            Parameter p;
            if (list.TryGetValue(name, out p))
                if (isValidParameter(name, ParameterType.INT, InterpolationType.NONE, 1, p))
                    return p.getIntValue();
            return defaultValue;
        }

        /**
         * Get the specified integer array parameter from this list.
         * 
         * @param name name of the parameter
         * @return the value of the parameter specified or <code>null</code> if
         *         not found
         */
        public int[] getIntArray(string name)
        {
            Parameter p;
            if (list.TryGetValue(name, out p))
                if (isValidParameter(name, ParameterType.INT, InterpolationType.NONE, -1, p))
                    return p.getInts();
            return null;
        }

        /**
         * Get the specified bool parameter from this list.
         * 
         * @param name name of the parameter
         * @param defaultValue value to return if not found
         * @return the value of the parameter specified or default value if not
         *         found
         */
        public bool getbool(string name, bool defaultValue)
        {
            Parameter p;
            if (list.TryGetValue(name, out p))
                if (isValidParameter(name, ParameterType.BOOL, InterpolationType.NONE, 1, p))
                    return p.getBoolValue();
            return defaultValue;
        }

        /**
         * Get the specified float parameter from this list.
         * 
         * @param name name of the parameter
         * @param defaultValue value to return if not found
         * @return the value of the parameter specified or default value if not
         *         found
         */
        public float getFloat(string name, float defaultValue)
        {
            Parameter p;
            if (list.TryGetValue(name, out p))
                if (isValidParameter(name, ParameterType.FLOAT, InterpolationType.NONE, 1, p))
                    return p.getFloatValue();
            return defaultValue;
        }

        /**
         * Get the specified color parameter from this list.
         * 
         * @param name name of the parameter
         * @param defaultValue value to return if not found
         * @return the value of the parameter specified or default value if not
         *         found
         */
        public Color getColor(string name, Color defaultValue)
        {
            Parameter p;
            if (list.TryGetValue(name, out p))
                if (isValidParameter(name, ParameterType.COLOR, InterpolationType.NONE, 1, p))
                    return p.getColor();
            return defaultValue;
        }

        /**
         * Get the specified point parameter from this list.
         * 
         * @param name name of the parameter
         * @param defaultValue value to return if not found
         * @return the value of the parameter specified or default value if not
         *         found
         */
        public Point3 getPoint(string name, Point3 defaultValue)
        {
            Parameter p;
            if (list.TryGetValue(name, out p))
                if (isValidParameter(name, ParameterType.POINT, InterpolationType.NONE, 1, p))
                    return p.getPoint();
            return defaultValue;
        }

        /**
         * Get the specified vector parameter from this list.
         * 
         * @param name name of the parameter
         * @param defaultValue value to return if not found
         * @return the value of the parameter specified or default value if not
         *         found
         */
        public Vector3 getVector(string name, Vector3 defaultValue)
        {
            Parameter p;
            if (list.TryGetValue(name, out p))
                if (isValidParameter(name, ParameterType.VECTOR, InterpolationType.NONE, 1, p))
                    return p.getVector();
            return defaultValue;
        }

        /**
         * Get the specified texture coordinate parameter from this list.
         * 
         * @param name name of the parameter
         * @param defaultValue value to return if not found
         * @return the value of the parameter specified or default value if not
         *         found
         */
        public Point2 getTexCoord(string name, Point2 defaultValue)
        {
            Parameter p;
            if (list.TryGetValue(name, out p))
                if (isValidParameter(name, ParameterType.TEXCOORD, InterpolationType.NONE, 1, p))
                    return p.getTexCoord();
            return defaultValue;
        }

        /**
         * Get the specified matrix parameter from this list.
         * 
         * @param name name of the parameter
         * @param defaultValue value to return if not found
         * @return the value of the parameter specified or default value if not
         *         found
         */
        public Matrix4 getMatrix(string name, Matrix4 defaultValue)
        {
            Parameter p;
            if (list.TryGetValue(name, out p))
                if (isValidParameter(name, ParameterType.MATRIX, InterpolationType.NONE, 1, p))
                    return p.getMatrix();
            return defaultValue;
        }

        /**
         * Get the specified float array parameter from this list.
         * 
         * @param name name of the parameter
         * @return the value of the parameter specified or <code>null</code> if
         *         not found
         */
        public FloatParameter getFloatArray(string name)
        {
            return getFloatParameter(name, ParameterType.FLOAT, list.ContainsKey(name) ? list[name] : null);
        }

        /**
         * Get the specified point array parameter from this list.
         * 
         * @param name name of the parameter
         * @return the value of the parameter specified or <code>null</code> if
         *         not found
         */
        public FloatParameter getPointArray(string name)
        {
            return getFloatParameter(name, ParameterType.POINT, list.ContainsKey(name) ? list[name] : null);
        }

        /**
         * Get the specified vector array parameter from this list.
         * 
         * @param name name of the parameter
         * @return the value of the parameter specified or <code>null</code> if
         *         not found
         */
        public FloatParameter getVectorArray(string name)
        {
            return getFloatParameter(name, ParameterType.VECTOR, list.ContainsKey(name) ? list[name] : null);
        }

        /**
         * Get the specified texture coordinate array parameter from this list.
         * 
         * @param name name of the parameter
         * @return the value of the parameter specified or <code>null</code> if
         *         not found
         */
        public FloatParameter getTexCoordArray(string name)
        {
            return getFloatParameter(name, ParameterType.TEXCOORD, list.ContainsKey(name) ? list[name] : null);
        }

        /**
         * Get the specified matrix array parameter from this list.
         * 
         * @param name name of the parameter
         * @return the value of the parameter specified or <code>null</code> if
         *         not found
         */
        public FloatParameter getMatrixArray(string name)
        {
            return getFloatParameter(name, ParameterType.MATRIX, list.ContainsKey(name) ? list[name] : null);
        }

        private bool isValidParameter(string name, ParameterType type, InterpolationType interp, int requestedSize, Parameter p)
        {
            if (p == null)
                return false;
            if (p.type != type)
            {
                UI.printWarning(UI.Module.API, "Parameter {0} requested as a {1} - declared as {2}", name, type.ToString().ToLower(), p.type.ToString().ToLower());
                return false;
            }
            if (p.interp != interp)
            {
                UI.printWarning(UI.Module.API, "Parameter {0} requested as a {1} - declared as {2}", name, interp.ToString().ToLower(), p.interp.ToString().ToLower());
                return false;
            }
            if (requestedSize > 0 && p.size() != requestedSize)
            {
                UI.printWarning(UI.Module.API, "Parameter {0} requires {1} {2} - declared with {3}", name, requestedSize, requestedSize == 1 ? "value" : "values", p.size());
                return false;
            }
            p.Checked = true;
            return true;
        }

        private FloatParameter getFloatParameter(string name, ParameterType type, Parameter p)
        {
            if (p == null)
                return null;
            switch (p.interp)
            {
                case InterpolationType.NONE:
                    if (!isValidParameter(name, type, p.interp, -1, p))
                        return null;
                    break;
                case InterpolationType.VERTEX:
                    if (!isValidParameter(name, type, p.interp, numVerts, p))
                        return null;
                    break;
                case InterpolationType.FACE:
                    if (!isValidParameter(name, type, p.interp, numFaces, p))
                        return null;
                    break;
                case InterpolationType.FACEVARYING:
                    if (!isValidParameter(name, type, p.interp, numFaceVerts, p))
                        return null;
                    break;
                default:
                    return null;
            }
            return p.getFloats();
        }

        /**
         * Represents an array of floating point values. This can store single
         * float, points, vectors, texture coordinates or matrices. The parameter
         * should be interpolated over the surface according to the interp parameter
         * when applicable.
         */
        public class FloatParameter
        {
            public InterpolationType interp;
            public float[] data;

            public FloatParameter()
                : this(InterpolationType.NONE, null)
            {
            }

            public FloatParameter(float f)
                : this(InterpolationType.NONE, new float[] { f })
            {
            }

            public FloatParameter(InterpolationType interp, float[] data)
            {
                this.interp = interp;
                this.data = data;
            }
        }

		public MovingMatrix4 getMovingMatrix(string name, MovingMatrix4 defaultValue) {
			// step 1: check for a non-moving specification:
			Matrix4 m = getMatrix(name, null);
			if (m != null)
				return new MovingMatrix4(m);
			// step 2: check to see if the time range has been updated
			FloatParameter times = getFloatArray(name + ".times");
			if (times != null) {
				if (times.data.Length <= 1)
					defaultValue.updateTimes(0, 0);
				else {
					if (times.data.Length != 2)
						UI.printWarning(UI.Module.API, "Time value specification using only endpoints of {0} values specified", times.data.Length);
					// get endpoint times - we might allow multiple time values
					// later
					float t0 = times.data[0];
					float t1 = times.data[times.data.Length - 1];
					defaultValue.updateTimes(t0, t1);
				}
			} else {
				// time range stays at default
			}
			// step 3: check to see if a number of steps has been specified
			int steps = getInt(name + ".steps", 0);
			if (steps <= 0) {
				// not specified - return default value
			} else {
				// update each element
				defaultValue.setSteps(steps);
				for (int i = 0; i < steps; i++)
					defaultValue.updateData(i, getMatrix(String.Format("{0}[{1}]", name, i), defaultValue.getData(i)));
			}
			return defaultValue;
		}

        public class Parameter
        {
            public ParameterType type;
            public InterpolationType interp;
            public Object obj;
            public bool Checked;

            public Parameter(string value)
            {
                type = ParameterType.STRING;
                interp = InterpolationType.NONE;
                obj = new string[] { value };
                Checked = false;
            }

            public Parameter(int value)
            {
                type = ParameterType.INT;
                interp = InterpolationType.NONE;
                obj = new int[] { value };
                Checked = false;
            }

            public Parameter(bool value)
            {
                type = ParameterType.BOOL;
                interp = InterpolationType.NONE;
                obj = value;
                Checked = false;
            }

            public Parameter(float value)
            {
                type = ParameterType.FLOAT;
                interp = InterpolationType.NONE;
                obj = new float[] { value };
                Checked = false;
            }

            public Parameter(int[] array)
            {
                type = ParameterType.INT;
                interp = InterpolationType.NONE;
                obj = array;
                Checked = false;
            }

            public Parameter(string[] array)
            {
                type = ParameterType.STRING;
                interp = InterpolationType.NONE;
                obj = array;
                Checked = false;
            }

            public Parameter(Color c)
            {
                type = ParameterType.COLOR;
                interp = InterpolationType.NONE;
                obj = c;
                Checked = false;
            }

            public Parameter(ParameterType type, InterpolationType interp, float[] data)
            {
                this.type = type;
                this.interp = interp;
                obj = data;
                Checked = false;
            }

            public int size()
            {
                // number of elements
                switch (type)
                {
                    case ParameterType.STRING:
                        return ((string[])obj).Length;
                    case ParameterType.INT:
                        return ((int[])obj).Length;
                    case ParameterType.BOOL:
                        return 1;
                    case ParameterType.FLOAT:
                        return ((float[])obj).Length;
                    case ParameterType.POINT:
                        return ((float[])obj).Length / 3;
                    case ParameterType.VECTOR:
                        return ((float[])obj).Length / 3;
                    case ParameterType.TEXCOORD:
                        return ((float[])obj).Length / 2;
                    case ParameterType.MATRIX:
                        return ((float[])obj).Length / 16;
                    case ParameterType.COLOR:
                        return 1;
                    default:
                        return -1;
                }
            }

            public void check()
            {
                Checked = true;
            }

            public override string ToString()
            {
                return string.Format("{0}{1}[{2}]", interp == InterpolationType.NONE ? "" : interp.ToString().ToLower() + " ", type.ToString().ToLower(), size());
            }

            public string getstringValue()
            {
                return ((string[])obj)[0];
            }

            public bool getBoolValue()
            {
                return (bool)obj;
            }

            public int getIntValue()
            {
                return ((int[])obj)[0];
            }

            public int[] getInts()
            {
                return (int[])obj;
            }

            public string[] getstrings()
            {
                return (string[])obj;
            }

            public float getFloatValue()
            {
                return ((float[])obj)[0];
            }

            public FloatParameter getFloats()
            {
                return new FloatParameter(interp, (float[])obj);
            }

            public Point3 getPoint()
            {
                float[] floats = (float[])obj;
                return new Point3(floats[0], floats[1], floats[2]);
            }

            public Vector3 getVector()
            {
                float[] floats = (float[])obj;
                return new Vector3(floats[0], floats[1], floats[2]);
            }

            public Point2 getTexCoord()
            {
                float[] floats = (float[])obj;
                return new Point2(floats[0], floats[1]);
            }

            public Matrix4 getMatrix()
            {
                float[] floats = (float[])obj;
                return new Matrix4(floats, true);
            }

            public Color getColor()
            {
                return (Color)obj;
            }
        }
    }
}