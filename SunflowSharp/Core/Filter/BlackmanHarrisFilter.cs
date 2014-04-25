using System;

namespace SunflowSharp.Core.Filter
{
    public class BlackmanHarrisFilter : IFilter
    {
        public float getSize()
        {
            return 4;
        }

        public float get(float x, float y)
        {
			return bh1d(x * 0.5f) * bh1d(y * 0.5f);
        }

        private float bh1d(float x)
        {
            if (x < -1.0f || x > 1.0f)
                return 0.0f;
            x = (x + 1) * 0.5f;
            double A0 = 0.35875;
            double A1 = -0.48829;
            double A2 = 0.14128;
            double A3 = -0.01168;
            return (float)(A0 + A1 * Math.Cos(2 * Math.PI * x) + A2 * Math.Cos(4 * Math.PI * x) + A3 * Math.Cos(6 * Math.PI * x));
        }
    }
}