using System;
using SunflowSharp.Maths;

namespace SunflowSharp.Image
{
    public class Color
    {
        public float r, g, b;
		public static RGBSpace NATIVE_SPACE = RGBSpace.SRGB;
        public static Color BLACK = new Color(0, 0, 0);
        public static Color WHITE = new Color(1, 1, 1);
        public static Color RED = new Color(1, 0, 0);
        public static Color GREEN = new Color(0, 1, 0);
        public static Color BLUE = new Color(0, 0, 1);
        public static Color YELLOW = new Color(1, 1, 0);
        public static Color CYAN = new Color(0, 1, 1);
        public static Color MAGENTA = new Color(1, 0, 1);
        public static Color GRAY = new Color(0.5f, 0.5f, 0.5f);

        public static Color black()
        {
            return new Color();
        }

        public static Color white()
        {
            return new Color(1, 1, 1);
        }

        private static float[] EXPONENT = null;

        //static {
        //    EXPONENT[0] = 0;
        //    for (int i = 1; i < 256; i++) {
        //        float f = 1.0f;
        //        int e = i - (128 + 8);
        //        if (e > 0)
        //            for (int j = 0; j < e; j++)
        //                f *= 2.0f;
        //        else
        //            for (int j = 0; j < -e; j++)
        //                f *= 0.5f;
        //        EXPONENT[i] = f;
        //    }
        //}

        public Color()
        {
            if (EXPONENT == null)
            {
                EXPONENT = new float[256];
                EXPONENT[0] = 0;
                for (int i = 1; i < 256; i++)
                {
                    float f = 1.0f;
                    int e = i - (128 + 8);
                    if (e > 0)
                        for (int j = 0; j < e; j++)
                            f *= 2.0f;
                    else
                        for (int j = 0; j < -e; j++)
                            f *= 0.5f;
                    EXPONENT[i] = f;
                }
            }
        }

        public Color(float gray)
        {
            r = g = b = gray;
        }

        public Color(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public Color toNonLinear()
        {
			r = NATIVE_SPACE.gammaCorrect(r);
			g = NATIVE_SPACE.gammaCorrect(g);
			b = NATIVE_SPACE.gammaCorrect(b);
            return this;
        }

        public Color toLinear()
        {
			r = NATIVE_SPACE.ungammaCorrect(r);
			g = NATIVE_SPACE.ungammaCorrect(g);
			b = NATIVE_SPACE.ungammaCorrect(b);
            return this;
        }

        public Color(Color c)
        {
            r = c.r;
            g = c.g;
            b = c.b;
        }

        public Color(int rgb)
        {
            r = ((rgb >> 16) & 0xFF) / 255.0f;
            g = ((rgb >> 8) & 0xFF) / 255.0f;
            b = (rgb & 0xFF) / 255.0f;
        }

        public Color copy()
        {
            return new Color(this);
        }

        public Color set(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            return this;
        }

        public Color set(Color c)
        {
            r = c.r;
            g = c.g;
            b = c.b;
            return this;
        }

        public Color setRGB(int rgb)
        {
            r = ((rgb >> 16) & 0xFF) / 255.0f;
            g = ((rgb >> 8) & 0xFF) / 255.0f;
            b = (rgb & 0xFF) / 255.0f;
            return this;
        }

		public int toRGBA(float a) 
		{
			int ir = (int) (r * 255 + 0.5);
			int ig = (int) (g * 255 + 0.5);
			int ib = (int) (b * 255 + 0.5);
			int ia = (int) (a * 255 + 0.5);
			ir = MathUtils.clamp(ir, 0, 255);
			ig = MathUtils.clamp(ig, 0, 255);
			ib = MathUtils.clamp(ib, 0, 255);
			ia = MathUtils.clamp(ia, 0, 255);
			return (ia << 24) | (ir << 16) | (ig << 8) | ib;
		}

        public Color setRGBE(int rgbe)
        {
            float f = EXPONENT[rgbe & 0xFF];
            r = f * (((uint)rgbe >> 24) + 0.5f);//>>>
            g = f * (((rgbe >> 16) & 0xFF) + 0.5f);
            b = f * (((rgbe >> 8) & 0xFF) + 0.5f);
            return this;
        }

        public bool isBlack()
        {
            return r <= 0f && g <= 0f && b <= 0f;
        }

        public float getLuminance()
        {
            return (0.2989f * r) + (0.5866f * g) + (0.1145f * b);
        }

        public float getMin()
        {
            return MathUtils.min(r, g, b);
        }

        public float getMax()
        {
            return MathUtils.max(r, g, b);
        }

        public float getAverage()
        {
            return (r + g + b) / 3.0f;
        }

        public float[] getRGB()
        {
            return new float[] { r, g, b };
        }

        public int toRGB()
        {
            int ir = (int)(r * 255 + 0.5);
            int ig = (int)(g * 255 + 0.5);
            int ib = (int)(b * 255 + 0.5);
            ir = MathUtils.clamp(ir, 0, 255);
            ig = MathUtils.clamp(ig, 0, 255);
            ib = MathUtils.clamp(ib, 0, 255);
            return (0xFF << 24) | (ir << 16) | (ig << 8) | ib;
        }

        public int toRGBE()
        {
            // encode the color into 32bits while preserving HDR using Ward's RGBE
            // technique
            float v = MathUtils.max(r, g, b);
            if (v < 1e-32f)
                return 0;

            // get mantissa and exponent
            float m = v;
            int e = 0;
            if (v > 1.0f)
            {
                while (m > 1.0f)
                {
                    m *= 0.5f;
                    e++;
                }
            }
            else if (v <= 0.5f)
            {
                while (m <= 0.5f)
                {
                    m *= 2.0f;
                    e--;
                }
            }
            v = (m * 255.0f) / v;
            int c = (e + 128);
            c |= ((int)(r * v) << 24);
            c |= ((int)(g * v) << 16);
            c |= ((int)(b * v) << 8);
            return c;
        }

        public Color constrainRGB()
        {
            // clamp the RGB value to a representable value
            float w = -MathUtils.min(0, r, g, b);
            if (w > 0)
            {
                r += w;
                g += w;
                b += w;
            }
            return this;
        }

        public bool isNan()
        {
            return float.IsNaN(r) || float.IsNaN(g) || float.IsNaN(b);
        }

        public bool isInf()
        {
            return float.IsInfinity(r) || float.IsInfinity(g) || float.IsInfinity(b);
        }

        public Color add(Color c)
        {
            r += c.r;
            g += c.g;
            b += c.b;
            return this;
        }

        public static Color add(Color c1, Color c2)
        {
            return Color.add(c1, c2, new Color());
        }

        public static Color add(Color c1, Color c2, Color dest)
        {
            dest.r = c1.r + c2.r;
            dest.g = c1.g + c2.g;
            dest.b = c1.b + c2.b;
            return dest;
        }

        public Color madd(float s, Color c)
        {
            r += (s * c.r);
            g += (s * c.g);
            b += (s * c.b);
            return this;
        }

        public Color madd(Color s, Color c)
        {
            r += s.r * c.r;
            g += s.g * c.g;
            b += s.b * c.b;
            return this;
        }

        public Color sub(Color c)
        {
            r -= c.r;
            g -= c.g;
            b -= c.b;
            return this;
        }

        public static Color sub(Color c1, Color c2)
        {
            return Color.sub(c1, c2, new Color());
        }

        public static Color sub(Color c1, Color c2, Color dest)
        {
            dest.r = c1.r - c2.r;
            dest.g = c1.g - c2.g;
            dest.b = c1.b - c2.b;
            return dest;
        }

        public Color mul(Color c)
        {
            r *= c.r;
            g *= c.g;
            b *= c.b;
            return this;
        }

        public static Color mul(Color c1, Color c2)
        {
            return Color.mul(c1, c2, new Color());
        }

        public static Color mul(Color c1, Color c2, Color dest)
        {
            dest.r = c1.r * c2.r;
            dest.g = c1.g * c2.g;
            dest.b = c1.b * c2.b;
            return dest;
        }

        public Color mul(float s)
        {
            r *= s;
            g *= s;
            b *= s;
            return this;
        }

        public static Color mul(float s, Color c)
        {
            return Color.mul(s, c, new Color());
        }

        public static Color mul(float s, Color c, Color dest)
        {
            dest.r = s * c.r;
            dest.g = s * c.g;
            dest.b = s * c.b;
            return dest;
        }

        public Color div(Color c)
        {
            r /= c.r;
            g /= c.g;
            b /= c.b;
            return this;
        }

        public static Color div(Color c1, Color c2)
        {
            return Color.div(c1, c2, new Color());
        }

        public static Color div(Color c1, Color c2, Color dest)
        {
            dest.r = c1.r / c2.r;
            dest.g = c1.g / c2.g;
            dest.b = c1.b / c2.b;
            return dest;
        }

        public Color exp()
        {
            r = (float)Math.Exp(r);
            g = (float)Math.Exp(g);
            b = (float)Math.Exp(b);
            return this;
        }

        public Color opposite()
        {
            r = 1 - r;
            g = 1 - g;
            b = 1 - b;
            return this;
        }

        public Color clamp(float min, float max)
        {
            r = MathUtils.clamp(r, min, max);
            g = MathUtils.clamp(g, min, max);
            b = MathUtils.clamp(b, min, max);
            return this;
        }

        public static Color blend(Color c1, Color c2, float b)
        {
            return blend(c1, c2, b, new Color());
        }

        public static Color blend(Color c1, Color c2, float b, Color dest)
        {
            dest.r = (1.0f - b) * c1.r + b * c2.r;
            dest.g = (1.0f - b) * c1.g + b * c2.g;
            dest.b = (1.0f - b) * c1.b + b * c2.b;
            return dest;
        }

        public static Color blend(Color c1, Color c2, Color b)
        {
            return blend(c1, c2, b, new Color());
        }

        public static Color blend(Color c1, Color c2, Color b, Color dest)
        {
            dest.r = (1.0f - b.r) * c1.r + b.r * c2.r;
            dest.g = (1.0f - b.g) * c1.g + b.g * c2.g;
            dest.b = (1.0f - b.b) * c1.b + b.b * c2.b;
            return dest;
        }

        public static bool hasContrast(Color c1, Color c2, float thresh)
        {
            if (Math.Abs(c1.r - c2.r) / (c1.r + c2.r) > thresh)
                return true;
            if (Math.Abs(c1.g - c2.g) / (c1.g + c2.g) > thresh)
                return true;
            if (Math.Abs(c1.b - c2.b) / (c1.b + c2.b) > thresh)
                return true;
            return false;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", r, g, b);
        }
    }
}