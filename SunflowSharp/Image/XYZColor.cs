using System;

namespace SunflowSharp.Image
{

    public class XYZColor
    {
        private float X, Y, Z;

        public XYZColor()
        {
        }

        public XYZColor(float X, float Y, float Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public float getX()
        {
            return X;
        }

        public float getY()
        {
            return Y;
        }

        public float getZ()
        {
            return Z;
        }

        public XYZColor mul(float s)
        {
            X *= s;
            Y *= s;
            Z *= s;
            return this;
        }

        public void normalize()
        {
            float XYZ = X + Y + Z;
            if (XYZ < 1e-6f)
                return;
            float s = 1 / XYZ;
            X *= s;
            Y *= s;
            Z *= s;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", X, Y, Z);
        }
    }
}