using System;

namespace SunflowSharp.Core.Filter
{
    public class BoxFilter : IFilter
    {
        public float getSize()
        {
            return 1.0f;
        }

        public float get(float x, float y)
        {
            return 1.0f;
        }
    }
}