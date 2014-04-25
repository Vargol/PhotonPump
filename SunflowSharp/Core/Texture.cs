using System;
using SunflowSharp.Image;
using SunflowSharp.Image.Formats;
using SunflowSharp.Maths;
using SunflowSharp.Systems;

namespace SunflowSharp.Core
{

    /**
     * Represents a 2D texture, typically used by {@link Shader shaders}.
     */
    public class Texture
    {
        private string filename;
        private bool isLinear;
        private Bitmap bitmap;
        private int loaded;
        private object lockObj = new object();
        /**
         * Creates a new texture from the specfied file.
         * 
         * @param filename image file to load
         * @param isLinear is the texture gamma corrected already?
         */
        public Texture(string filename, bool isLinear)
        {
            this.filename = filename;
            this.isLinear = isLinear;
            loaded = 0;
        }

        private void load()
        {
            lock (lockObj)
            {
                if (loaded != 0)
                    return;
				string extension = FileUtils.getExtension(filename);
				try
                {
					UI.printInfo(UI.Module.TEX, "Reading texture bitmap from: \"{0}\" ...", filename);
					BitmapReader reader = PluginRegistry.bitmapReaderPlugins.createObject(extension);
					if (reader != null) 
					{
						bitmap = reader.load(filename, isLinear);
	                    if (bitmap.getWidth() == 0 || bitmap.getHeight() == 0)
	                        bitmap = null;
					}
					if (bitmap == null) {
						UI.printError(UI.Module.TEX, "Bitmap reading failed");
						bitmap = new BitmapBlack();
					}

                }
                catch (Exception e)
                {
                    UI.printError(UI.Module.TEX, "{0}", e);
                }
                loaded = 1;
            }
        }

        public Bitmap getBitmap()
        {
            if (loaded == 0)
                load();
            return bitmap;
        }

        /**
         * Gets the color at location (x,y) in the texture. The lookup is performed
         * using the fractional component of the coordinates, treating the texture
         * as a unit square tiled in both directions. Bicubic filtering is performed
         * on the four nearest pixels to the lookup point.
         * 
         * @param x x coordinate into the texture
         * @param y y coordinate into the texture
         * @return filtered color at location (x,y)
         */
        public Color getPixel(float x, float y)
        {
            Bitmap bitmap = getBitmap();
			x = MathUtils.frac(x);
			y = MathUtils.frac(y);
			float dx = x * bitmap.getWidth();
			float dy = y * bitmap.getHeight();
			int ix0 = MathUtils.clamp((int) dx, 0, bitmap.getWidth() - 1);
			int iy0 = MathUtils.clamp((int) dy, 0, bitmap.getHeight() - 1);
			int ix1 = (ix0 + 1) % bitmap.getWidth();
			int iy1 = (iy0 + 1) % bitmap.getHeight();
            float u = dx - ix0;
            float v = dy - iy0;
            u = u * u * (3.0f - (2.0f * u));
            v = v * v * (3.0f - (2.0f * v));
            float k00 = (1.0f - u) * (1.0f - v);
            Color c00 = bitmap.readColor(ix0, iy0);
            float k01 = (1.0f - u) * v;
			Color c01 = bitmap.readColor(ix0, iy1);
            float k10 = u * (1.0f - v);
			Color c10 = bitmap.readColor(ix1, iy0);
            float k11 = u * v;
			Color c11 = bitmap.readColor(ix1, iy1);
            Color c = Color.mul(k00, c00);
            c.madd(k01, c01);
            c.madd(k10, c10);
            c.madd(k11, c11);
            return c;
        }

        public Vector3 getNormal(float x, float y, OrthoNormalBasis basis)
        {
            float[] rgb = getPixel(x, y).getRGB();
            return basis.transform(new Vector3(2 * rgb[0] - 1, 2 * rgb[1] - 1, 2 * rgb[2] - 1)).normalize();
        }

        public Vector3 getBump(float x, float y, OrthoNormalBasis basis, float scale)
        {
            Bitmap bitmap = getBitmap();
            float dx = 1.0f / bitmap.getWidth();
            float dy = 1.0f / bitmap.getHeight();
            float b0 = getPixel(x, y).getLuminance();
            float bx = getPixel(x + dx, y).getLuminance();
            float by = getPixel(x, y + dy).getLuminance();
			return basis.transform(new Vector3(scale * (b0 - bx), scale * (b0 - by), 1)).normalize();
        }
    }
}