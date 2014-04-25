using System;

namespace SunflowSharp.Maths
{
    public class Vector3
    {
        private static float[] COS_THETA = null;
        private static float[] SIN_THETA = null;
        private static float[] COS_PHI = null;
        private static float[] SIN_PHI = null;

        public float x, y, z;

        public Vector3()
        {
            if (COS_THETA == null)
            {
                COS_THETA = new float[256];
                SIN_THETA = new float[256];
                COS_PHI = new float[256];
                SIN_PHI = new float[256];
                for (int i = 0; i < 256; i++)
                {
                    double angle = (i * Math.PI) / 256.0;
                    COS_THETA[i] = (float)Math.Cos(angle);
                    SIN_THETA[i] = (float)Math.Sin(angle);
                    COS_PHI[i] = (float)Math.Cos(2 * angle);
                    SIN_PHI[i] = (float)Math.Sin(2 * angle);
                }
            }
        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public static Vector3 decode(short n, Vector3 dest) {
        int t = (int)((uint)(n & 0xFF00) >> 8);//>>>
        int p = n & 0xFF;
        dest.x = SIN_THETA[t] * COS_PHI[p];
        dest.y = SIN_THETA[t] * SIN_PHI[p];
        dest.z = COS_THETA[t];
        return dest;
    }

        public static Vector3 decode(short n)
        {
            return decode(n, new Vector3());
        }

        public short encode()
        {
            int theta = (int)(Math.Acos(z) * (256.0 / Math.PI));
            if (theta > 255)
                theta = 255;
            int phi = (int)(Math.Atan2(y, x) * (128.0 / Math.PI));
            if (phi < 0)
                phi += 256;
            else if (phi > 255)
                phi = 255;
            return (short)(((theta & 0xFF) << 8) | (phi & 0xFF));
        }

        public float get(int i)
        {
            switch (i)
            {
                case 0:
                    return x;
                case 1:
                    return y;
                default:
                    return z;
            }
        }

        public float Length()
        {
            return (float)Math.Sqrt((x * x) + (y * y) + (z * z));
        }

        public float LengthSquared()
        {
            return (x * x) + (y * y) + (z * z);
        }

        public Vector3 negate()
        {
            x = -x;
            y = -y;
            z = -z;
            return this;
        }

        public Vector3 negate(Vector3 dest)
        {
            dest.x = -x;
            dest.y = -y;
            dest.z = -z;
            return dest;
        }

        public Vector3 mul(float s)
        {
            x *= s;
            y *= s;
            z *= s;
            return this;
        }

        public Vector3 mul(float s, Vector3 dest)
        {
            dest.x = x * s;
            dest.y = y * s;
            dest.z = z * s;
            return dest;
        }

        public Vector3 div(float d)
        {
            x /= d;
            y /= d;
            z /= d;
            return this;
        }

        public Vector3 div(float d, Vector3 dest)
        {
            dest.x = x / d;
            dest.y = y / d;
            dest.z = z / d;
            return dest;
        }

        public float normalizeLength()
        {
            float n = (float)Math.Sqrt(x * x + y * y + z * z);
            float inf = 1.0f / n;
            x *= inf;
            y *= inf;
            z *= inf;
            return n;
        }

        public Vector3 normalize()
        {
            float inf = 1.0f / (float)Math.Sqrt((x * x) + (y * y) + (z * z));
            x *= inf;
            y *= inf;
            z *= inf;
            return this;
        }

        public Vector3 normalize(Vector3 dest)
        {
            float inf = 1.0f / (float)Math.Sqrt((x * x) + (y * y) + (z * z));
            dest.x = x * inf;
            dest.y = y * inf;
            dest.z = z * inf;
            return dest;
        }

        public Vector3 set(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            return this;
        }

        public Vector3 set(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
            return this;
        }

        public float dot(float vx, float vy, float vz)
        {
            return vx * x + vy * y + vz * z;
        }

        public static float dot(Vector3 v1, Vector3 v2)
        {
            return (v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z);
        }

        public static Vector3 cross(Vector3 v1, Vector3 v2, Vector3 dest)
        {
            dest.x = (v1.y * v2.z) - (v1.z * v2.y);
            dest.y = (v1.z * v2.x) - (v1.x * v2.z);
            dest.z = (v1.x * v2.y) - (v1.y * v2.x);
            return dest;
        }

        public static Vector3 add(Vector3 v1, Vector3 v2, Vector3 dest)
        {
            dest.x = v1.x + v2.x;
            dest.y = v1.y + v2.y;
            dest.z = v1.z + v2.z;
            return dest;
        }

        public static Vector3 sub(Vector3 v1, Vector3 v2, Vector3 dest)
        {
            dest.x = v1.x - v2.x;
            dest.y = v1.y - v2.y;
            dest.z = v1.z - v2.z;
            return dest;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }
    }
}