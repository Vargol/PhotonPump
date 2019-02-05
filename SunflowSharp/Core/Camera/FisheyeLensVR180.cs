using System;
using SunflowSharp.Core;

namespace SunflowSharp.Core.Camera
{
    public class FisheyeLensVR180 : CameraLens
    {

        float eyegap = 0;

        public bool Update(ParameterList pl, SunflowAPI api)
        {
            eyegap = pl.getFloat("lens.eyegap", eyegap) * 0.5f;
            return true;
        }

        public Ray getRay(float x, float y, int imageWidth, int imageHeight, double lensX, double lensY, double time)
        {

            float eyeOffset;
            float halfWidth = (imageWidth / 2.0f);
            if (x >= halfWidth)
            {
                eyeOffset = -eyegap;
            }
            else
            {
                eyeOffset = eyegap;
            }

            x = (x % halfWidth) * 2.0f;

            float cx = 2.0f * x / imageWidth - 1.0f;
            float cy = 2.0f * y / imageHeight - 1.0f;
            float r2 = cx * cx + cy * cy;
            if (r2 > 1)
                return null; // outside the fisheye
            return new Ray(eyeOffset, 0, 0, cx, cy, (float)-Math.Sqrt(1 - r2));
        }
    }
}