using System;
using SunflowSharp.Core;
using SunflowSharp.Maths;

namespace SunflowSharp.Core.Camera
{
    public class PinholeLens : CameraLens
    {
        private float au, av;
        private float aspect, fov;
		private float shiftX, shiftY;

        public PinholeLens()
        {
            fov = 90;
            aspect = 1;
			shiftX = shiftY = 0;
            Update();
        }

        public bool Update(ParameterList pl, SunflowAPI api)
        {
            // get parameters
            fov = pl.getFloat("fov", fov);
            aspect = pl.getFloat("aspect", aspect);
			shiftX = pl.getFloat("shift.x", shiftX);
			shiftY = pl.getFloat("shift.y", shiftY);
            Update();
            return true;
        }

        private void Update()
        {
            au = (float)Math.Tan(MathUtils.toRadians(fov * 0.5f));
            av = au / aspect;
        }

        public Ray getRay(float x, float y, int imageWidth, int imageHeight, double lensX, double lensY, double time)
        {
			float du = shiftX - au + ((2.0f * au * x) / (imageWidth - 1.0f));
			float dv = shiftY - av + ((2.0f * av * y) / (imageHeight - 1.0f));
			return new Ray(0, 0, 0, du, dv, -1);
        }
    }
}