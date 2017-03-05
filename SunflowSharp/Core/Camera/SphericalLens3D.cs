using System;
using SunflowSharp.Core;

namespace SunflowSharp.Core.Camera
{
    public class Spherical3DLens : CameraLens
    {
		float eyegap = 0;

        public bool Update(ParameterList pl, SunflowAPI api)
        {
			eyegap = pl.getFloat("lens.eyegap", eyegap) * 0.5f;
			return true;
        }

        public Ray getRay(float x, float y, int imageWidth, int imageHeight, double lensX, double lensY, double time)
        {

			float eyeoffset;
			float halfHeight = (imageHeight / 2.0f);
			if (y > halfHeight)
			{
				eyeoffset = -eyegap;
			}
			else
			{
				eyeoffset = eyegap;
			}
			y = y % halfHeight * 2.0f;

			// Generate environment camera ray direction
            double theta = 2 * Math.PI * x / imageWidth + Math.PI / 2;
            double phi = Math.PI * (imageHeight - 1 - y) / imageHeight;
			return new Ray(eyeoffset, 0, 0, (float)(Math.Cos(theta) * Math.Sin(phi)), (float)(Math.Cos(phi)), (float)(Math.Sin(theta) * Math.Sin(phi)));
        }
    }
}