using System;

namespace SunflowSharp.Core.Filter
{
    public class GaussianFilter : IFilter
    {
        private float es2;

        public GaussianFilter()
        {
            es2 = (float)-Math.Exp(-9.0f);
        }

        public float getSize()
        {
            return 3.0f;
        }

        public float get(float x, float y)
        {
            float gx = (float)Math.Exp(-x * x) + es2;
            float gy = (float)Math.Exp(-y * y) + es2;
            return gx * gy;
        }
    }
}