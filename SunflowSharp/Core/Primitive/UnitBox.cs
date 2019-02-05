using System;
using SunflowSharp.Core;
using SunflowSharp.Maths;

namespace SunflowSharp.Core.Primitive
{

    public class UnitBox : Box
    {
        public UnitBox()
        {
            minX = minY = minZ = 0;
            maxX = maxY = maxZ = +1;
        }

    }
}