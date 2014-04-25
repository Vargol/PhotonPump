using System;

namespace SunflowSharp.Core.Filter
{
    public class CatmullRomFilter : IFilter
    {
        public float getSize()
        {
            return 4.0f;
        }

        public float get(float x, float y)
        {
            return catrom1d(x) * catrom1d(y);
        }

        private float catrom1d(float x)
        {
            x = Math.Abs(x);
            float x2 = x * x;
            float x3 = x * x2;
            if (x >= 2)
                return 0;
            if (x < 1)
                return 3 * x3 - 5 * x2 + 2;
            return -x3 + 5 * x2 - 8 * x + 4;
        }
    }
}