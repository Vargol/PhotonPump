using System;
using SunflowSharp.Core;

namespace SunflowSharp.Core.Camera
{
    public class SphericalODSLens : CameraLens
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
			if (y >= halfHeight)
			{
				eyeOffset = eyegap;
			}
			else
			{
				eyeOffset = -eyegap;
			}

            y = (y % halfHeight) * 2.0f;



       //     imageWidth--;
         //   imageHeight--;

            double theta = (x / imageWidth) * 2.0 *  Math.PI - Math.PI;
            double phi = Math.PI / 2.0 - ((imageHeight - y / imageHeight) * Math.PI);

            // generate camera position


            double odsOffset = Math.Sin((imageHeight - y / imageHeight) * Math.PI);


            double stc = Math.Sin(theta);
            double ctc = Math.Cos(theta);
            double cpc = Math.Cos(phi);

			// Generate environment camera ray direction
			return new Ray(
                (float)(eyeOffset * ctc*odsOffset),
				0,
                (float)(eyeOffset * stc* odsOffset),
                (float)(Math.Sin(theta) * cpc),
                (float)(Math.Sin(phi)),
				(float)((-ctc * cpc))
            );

            /*
            double theta = Math.PI * x / imageWidth + Math.PI;
			double phi = Math.PI * (imageHeight - 1 - y) / imageHeight;
			return new Ray(eyeoffset, 0, 0, (float)(Math.Cos(theta) * Math.Sin(phi)), (float)(Math.Cos(phi)), (float)(Math.Sin(theta) * Math.Sin(phi)));
            */


		}


    }
}