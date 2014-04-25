using System;
using System.IO;
using SunflowSharp.Core;
using SunflowSharp.Image;

namespace SunflowSharp.Core.Display
{

    public class ImgPipeDisplay : JPanel, IDisplay
    {
        private int ih;
        private object lockObj = new object();
        /**
         * Render to stdout using the imgpipe protocol used in mental image's
         * imf_disp viewer. http://www.lamrug.org/resources/stubtips.html
         */
        public ImgPipeDisplay()
        {
        }

        public void imageBegin(int w, int h, int bucketSize)
        {
            lock (lockObj)
            {
                ih = h;
                outputPacket(5, w, h, ByteUtil.floatToRawIntBits(1.0f), 0);
                Console.OpenStandardOutput().Flush();
            }
        }

        public void imagePrepare(int x, int y, int w, int h, int id)
        {
        }

		public void imageUpdate(int x, int y, int w, int h, Color[] data, float[] alpha)
        {
            lock (lockObj)
            {
                int xl = x;
                int xh = x + w - 1;
                int yl = ih - 1 - (y + h - 1);
                int yh = ih - 1 - y;
                outputPacket(2, xl, xh, yl, yh);
                byte[] rgba = new byte[4 * (yh - yl + 1) * (xh - xl + 1)];
                for (int j = 0, idx = 0; j < h; j++)
                {
                    for (int i = 0; i < w; i++, idx += 4)
                    {
                        int rgb = data[(h - j - 1) * w + i].toNonLinear().toRGB();
                        int cr = (rgb >> 16) & 0xFF;
                        int cg = (rgb >> 8) & 0xFF;
                        int cb = rgb & 0xFF;
                        rgba[idx + 0] = (byte)(cr & 0xFF);
                        rgba[idx + 1] = (byte)(cg & 0xFF);
                        rgba[idx + 2] = (byte)(cb & 0xFF);
                        rgba[idx + 3] = (byte)(0xFF);
                    }
                }
                try
                {
                    Console.Writerite(rgba);
                }
                catch (IOException e)
                {
                    e.printStackTrace();
                }
            }
        }

		public void imageFill(int x, int y, int w, int h, Color c, float alpha)
        {
            lock (lockObj)
            {
                int xl = x;
                int xh = x + w - 1;
                int yl = ih - 1 - (y + h - 1);
                int yh = ih - 1 - y;
                outputPacket(2, xl, xh, yl, yh);
                int rgb = c.toNonLinear().toRGB();
                int cr = (rgb >> 16) & 0xFF;
                int cg = (rgb >> 8) & 0xFF;
                int cb = rgb & 0xFF;
                byte[] rgba = new byte[4 * (yh - yl + 1) * (xh - xl + 1)];
                for (int j = 0, idx = 0; j < h; j++)
                {
                    for (int i = 0; i < w; i++, idx += 4)
                    {
                        rgba[idx + 0] = (byte)(cr & 0xFF);
                        rgba[idx + 1] = (byte)(cg & 0xFF);
                        rgba[idx + 2] = (byte)(cb & 0xFF);
                        rgba[idx + 3] = (byte)(0xFF);
                    }
                }
                try
                {
                    Console.Write(rgba);
                }
                catch (IOException e)
                {
                    e.printStackTrace();
                }
            }
        }

        public void imageEnd()
        {
            lock (lockObj)
            {
                outputPacket(4, 0, 0, 0, 0);
                Console.OpenStandardOutput().Flush();
            }
        }

        private void outputPacket(int type, int d0, int d1, int d2, int d3)
        {
            outputInt32(type);
            outputInt32(d0);
            outputInt32(d1);
            outputInt32(d2);
            outputInt32(d3);
        }

        private void outputInt32(int i)
        {
            Console.Write((i >> 24) & 0xFF);
            Console.Write((i >> 16) & 0xFF);
            Console.Write((i >> 8) & 0xFF);
            Console.Write(i & 0xFF);
        }
    }
}