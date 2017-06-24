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

            double eyeOffset;
			float halfHeight = (imageHeight / 2.0f);
			if (y > halfHeight)
			{
				eyeOffset = eyegap;
			}
			else
			{
				eyeOffset = -eyegap;
			}
			y = y % halfHeight * 2.0f;

            double theta = 2 * Math.PI * x / imageWidth;

            // generate camera position

			double stc = Math.Sin(theta);
			double ctc = Math.Cos(theta);


			// Generate environment camera ray direction
            theta += (Math.PI / 2.0);
            double phi = Math.PI * (imageHeight - 1 - y) / imageHeight;

            double spe = Math.Sin(phi);

            return new Ray(
                           (float)(eyeOffset * ctc), 
                           0, 
                           (float)(eyeOffset * stc),
						   (float)((Math.Cos(theta) * spe)),
						   (float)(Math.Cos(phi)),
						   (float)((Math.Sin(theta) * spe))
			);
        }
    }
}