using System;

namespace SunflowSharp.Maths
{
    public class Point2
    {
        public float x, y;

        public Point2()
        {
        }

        public Point2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Point2(Point2 p)
        {
            x = p.x;
            y = p.y;
        }

        public Point2 set(float x, float y)
        {
            this.x = x;
            this.y = y;
            return this;
        }

        public Point2 set(Point2 p)
        {
            x = p.x;
            y = p.y;
            return this;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", x, y);
        }
    }
}