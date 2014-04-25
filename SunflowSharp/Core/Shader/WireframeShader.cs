using System;
using SunflowSharp.Core;
using SunflowSharp.Image;
using SunflowSharp.Maths;

namespace SunflowSharp.Core.Shader
{

    public class WireframeShader : IShader
    {
        private Color lineColor;
        private Color fillColor;
        private float width;
        private float cosWidth;

        public WireframeShader()
        {
            lineColor = Color.BLACK;
            fillColor = Color.WHITE;
            // pick a very small angle - should be roughly the half the angular width of a
            // pixel
            width = (float)(Math.PI * 0.5 / 4096);
            cosWidth = (float)Math.Cos(width);
        }

        public bool update(ParameterList pl, SunflowAPI api)
        {
            lineColor = pl.getColor("line", lineColor);
            fillColor = pl.getColor("fill", fillColor);
            width = pl.getFloat("width", width);
            cosWidth = (float)Math.Cos(width);
            return true;
        }

        public virtual Color getFillColor(ShadingState state)
        {
            return fillColor;
        }

        public Color getLineColor(ShadingState state)
        {
            return lineColor;
        }

        public Color getRadiance(ShadingState state)
        {
            Point3[] p = new Point3[3];
            if (!state.getTrianglePoints(p))
                return getFillColor(state);
            // transform points into camera space
            Point3 center = state.getPoint();
            Matrix4 w2c = state.getWorldToCamera();
            center = w2c.transformP(center);
            for (int i = 0; i < 3; i++)
                p[i] = w2c.transformP(state.transformObjectToWorld(p[i]));
            float cn = 1.0f / (float)Math.Sqrt(center.x * center.x + center.y * center.y + center.z * center.z);
            for (int i = 0, i2 = 2; i < 3; i2 = i, i++)
            {
                // compute orthogonal projection of the shading point onto each
                // triangle edge as in:
                // http://mathworld.wolfram.com/Point-LineDistance3-Dimensional.html
                float t = (center.x - p[i].x) * (p[i2].x - p[i].x);
                t += (center.y - p[i].y) * (p[i2].y - p[i].y);
                t += (center.z - p[i].z) * (p[i2].z - p[i].z);
                t /= p[i].distanceToSquared(p[i2]);
                float projx = (1 - t) * p[i].x + t * p[i2].x;
                float projy = (1 - t) * p[i].y + t * p[i2].y;
                float projz = (1 - t) * p[i].z + t * p[i2].z;
                float n = 1.0f / (float)Math.Sqrt(projx * projx + projy * projy + projz * projz);
                // check angular width
                float dot = projx * center.x + projy * center.y + projz * center.z;
                if (dot * n * cn >= cosWidth)
                    return getLineColor(state);
            }
            return getFillColor(state);
        }

        public void scatterPhoton(ShadingState state, Color power)
        {
        }
    }
}