using System;

namespace SunflowSharp.Core.Filter
{
    public class MitchellFilter : IFilter
    {
        public float getSize()
        {
            return 4.0f;
        }

        public float get(float x, float y)
        {
            return mitchell(x) * mitchell(y);
        }

        private float mitchell(float x)
        {
            float B = 1 / 3.0f;
            float C = 1 / 3.0f;
            float SIXTH = 1 / 6.0f;
            x = Math.Abs(x);
            float x2 = x * x;
            if (x > 1.0f)
                return ((-B - 6 * C) * x * x2 + (6 * B + 30 * C) * x2 + (-12 * B - 48 * C) * x + (8 * B + 24 * C)) * SIXTH;
            return ((12 - 9 * B - 6 * C) * x * x2 + (-18 + 12 * B + 6 * C) * x2 + (6 - 2 * B)) * SIXTH;
        }
    }
}