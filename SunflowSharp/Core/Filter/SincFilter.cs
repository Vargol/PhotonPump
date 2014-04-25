using System;

namespace SunflowSharp.Core.Filter
{
    public class SincFilter : IFilter
    {
        public float getSize()
        {
            return 4;
        }

        public float get(float x, float y)
        {
            return sinc1d(x) * sinc1d(y);
        }

        private float sinc1d(float x)
        {
            x = Math.Abs(x);
            if (x < 0.0001f)
                return 1.0f;
            x *= (float)Math.PI;
            return (float)Math.Sin(x) / x;
        }
    }
}