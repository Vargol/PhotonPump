using System;

namespace SunflowSharp.Core.Filter
{
    public class TriangleFilter : IFilter
    {
        public float getSize()
        {
            return 2.0f;
        }

        public float get(float x, float y)
        {
			return (1.0f - Math.Abs(x)) * (1.0f - Math.Abs(y));
        }
    }
}