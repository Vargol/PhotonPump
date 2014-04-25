using System;
using SunflowSharp.Core;

namespace SunflowSharp.Core.Camera
{
    public class FisheyeLens : CameraLens
    {
        public bool update(ParameterList pl, SunflowAPI api)
        {
            return true;
        }

        public Ray getRay(float x, float y, int imageWidth, int imageHeight, double lensX, double lensY, double time)
        {
            float cx = 2.0f * x / imageWidth - 1.0f;
            float cy = 2.0f * y / imageHeight - 1.0f;
            float r2 = cx * cx + cy * cy;
            if (r2 > 1)
                return null; // outside the fisheye
            return new Ray(0, 0, 0, cx, cy, (float)-Math.Sqrt(1 - r2));
        }
    }
}