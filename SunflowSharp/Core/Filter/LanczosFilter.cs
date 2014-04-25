using System;

namespace SunflowSharp.Core.Filter
{
    public class LanczosFilter : IFilter
    {
        public float getSize()
        {
            return 4.0f;
        }

        public float get(float x, float y)
        {
            return sinc1d(x * 0.5f) * sinc1d(y * 0.5f);
        }

        private float sinc1d(float x)
        {
            x = Math.Abs(x);
            if (x < 1e-5f)
                return 1;
            if (x > 1.0f)
                return 0;
            x *= (float)Math.PI;
            float sinc = (float)Math.Sin(3 * x) / (3 * x);
            float lanczos = (float)Math.Sin(x) / x;
            return sinc * lanczos;
        }

    }
}