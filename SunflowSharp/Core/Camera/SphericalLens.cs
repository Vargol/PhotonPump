using System;
using SunflowSharp.Core;

namespace SunflowSharp.Core.Camera
{
    public class SphericalLens : CameraLens
    {
        public bool Update(ParameterList pl, SunflowAPI api)
        {
            return true;
        }

        public Ray getRay(float x, float y, int imageWidth, int imageHeight, double lensX, double lensY, double time)
        {
            // Generate environment camera ray direction
            double theta = 2 * Math.PI * x / imageWidth + Math.PI / 2;
            double phi = Math.PI * (imageHeight - 1 - y) / imageHeight;
            return new Ray(0, 0, 0, (float)(Math.Cos(theta) * Math.Sin(phi)), (float)(Math.Cos(phi)), (float)(Math.Sin(theta) * Math.Sin(phi)));
        }
    }
}