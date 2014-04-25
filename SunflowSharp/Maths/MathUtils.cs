using System;
using SunflowSharp.Systems;

namespace SunflowSharp.Maths
{
    public class MathUtils
    {
        private MathUtils()
        {
        }

        public static int clamp(int x, int min, int max)
        {
            if (x > max)
                return max;
            if (x > min)
                return x;
            return min;
        }

        public static float clamp(float x, float min, float max)
        {
            if (x > max)
                return max;
            if (x > min)
                return x;
            return min;
        }

        public static double clamp(double x, double min, double max)
        {
            if (x > max)
                return max;
            if (x > min)
                return x;
            return min;
        }

        public static int min(int a, int b, int c)
        {
            if (a > b)
                a = b;
            if (a > c)
                a = c;
            return a;
        }

        public static float min(float a, float b, float c)
        {
            if (a > b)
                a = b;
            if (a > c)
                a = c;
            return a;
        }

        public static double min(double a, double b, double c)
        {
            if (a > b)
                a = b;
            if (a > c)
                a = c;
            return a;
        }

        public static float min(float a, float b, float c, float d)
        {
            if (a > b)
                a = b;
            if (a > c)
                a = c;
            if (a > d)
                a = d;
            return a;
        }

        public static int max(int a, int b, int c)
        {
            if (a < b)
                a = b;
            if (a < c)
                a = c;
            return a;
        }

        public static float max(float a, float b, float c)
        {
            if (a < b)
                a = b;
            if (a < c)
                a = c;
            return a;
        }

        public static double max(double a, double b, double c)
        {
            if (a < b)
                a = b;
            if (a < c)
                a = c;
            return a;
        }

        public static float max(float a, float b, float c, float d)
        {
            if (a < b)
                a = b;
            if (a < c)
                a = c;
            if (a < d)
                a = d;
            return a;
        }

        public static float smoothStep(float a, float b, float x)
        {
            if (x <= a)
                return 0;
            if (x >= b)
                return 1;
            float t = clamp((x - a) / (b - a), 0.0f, 1.0f);
            return t * t * (3 - 2 * t);
        }

		public static float frac(float x) {
			return x < 0 ? x - (int) x + 1 : x - (int) x;
		}


        /**
         * Computes a fast approximation to <code>Math.Pow(a, b)</code>. Adapted
         * from <url>http://www.dctsystems.co.uk/Software/power.html</url>.
         * 
         * @param a a positive number
         * @param b a number
         * @return a^b
         */
        public static float fastPow(float a, float b)
        {
            // adapted from: http://www.dctsystems.co.uk/Software/power.html
            float x = ByteUtil.floatToRawIntBits(a);
            x *= 1.0f / (1 << 23);
            x = x - 127;
            float y = x - (int)Math.Floor(x);
            b *= x + (y - y * y) * 0.346607f;
            y = b - (int)Math.Floor(b);
            y = (y - y * y) * 0.33971f;
            return ByteUtil.intBitsToFloat((int)((b + 127 - y) * (1 << 23)));
        }

        public static double toRadians(double d)
        {
            return (d / 180) * 3.1415926535897931;//fixme: switch to Pi when the conflicting types are sorted out
        }
    }
}