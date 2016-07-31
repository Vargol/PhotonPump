using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SunflowSharp.Core;
using SunflowSharp.Core.Shader;

namespace SunflowSharp.Gui
{
    public partial class RenderForm : Form, IDisplay
    {
        GuiApi api;
        Bitmap bitmap;
        public RenderForm()
        {
            InitializeComponent();
            new Thread(new ThreadStart(Render)).Start();
        }

        private void Render()
        {
			api = new GuiApi(@"D:\Trabajo\Unity\LightmapTests\SunflowSharp\PhotonPump\examples\sphereflake.sc");
            api.build();
            api.render(null, this);
        }

        private void RenderForm_Paint(object sender, PaintEventArgs e)
        {
            if (bitmap != null)
                lock(bitmap)
                    e.Graphics.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
        }

        #region IDisplay Members

        public delegate void imageBeginDelegate(int w, int h, int bucketSize);
        public void imageBegin(int w, int h, int bucketSize)
        {
            if (InvokeRequired)
                Invoke(new imageBeginDelegate(imageBegin), w, h, bucketSize);
            else
            {
                bitmap = new Bitmap(w, h);
                Width = w;
                Height = h;
                Show();
            }
        }

        public void imagePrepare(int x, int y, int w, int h, int id)
        {
        }

        public delegate void imageUpdateDelegate(int x, int y, int w, int h, Image.Color[] data, float[] a);
		public void imageUpdate(int x, int y, int w, int h, SunflowSharp.Image.Color[] data, float[] a)
        {
            if (InvokeRequired)
                Invoke(new imageUpdateDelegate(imageUpdate), x, y, w, h, data, a);
            else
            {
                try
                {
                    lock (bitmap)
                    {
                        using (Bitmap flip = new Bitmap(w, h))
                            using (Graphics g = Graphics.FromImage(flip))
                            {
                                for (int i = x, index = 0; i < x + w; i++)
                                    for (int j = y; j < y + h; j++, index++)
                                    {
                                        flip.SetPixel(i % 32, j % 32, Color.FromArgb(data[index].copy().toNonLinear().toRGB()));
                                        //flip.SetPixel(i % 32, j % 32, Color.FromArgb((int)(data[index].r * 255), (int)(data[index].g * 255), (int)(data[index].b * 255)));
                                    }
                                flip.RotateFlip(RotateFlipType.Rotate90FlipX);
                                using (Graphics form = Graphics.FromImage(bitmap))
                                    form.DrawImage(flip, x, y);
                            }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                Invalidate();
            }
        }

        public void imageFill(int x, int y, int w, int h, Image.Color c, float a)
        {
            lock (bitmap)
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                    using (Pen p = new Pen(Color.FromArgb(c.copy().toNonLinear().toRGB())))
                        g.DrawRectangle(p, x, y, w, h);
            }
            Invalidate();
        }

        public delegate void imageEndDelegate();
        public void imageEnd()
        {
            if (InvokeRequired)
                Invoke(new imageEndDelegate(imageEnd));
            else
                Text = "Done";
        }

        #endregion

        private void RenderForm_Click(object sender, EventArgs e)
        {
            SaveFileDialog s = new SaveFileDialog();
            s.Filter = "Bitmap files *.bmp|*.bmp";
            if (s.ShowDialog() == DialogResult.OK)
                bitmap.Save(s.FileName);
        }
    }

    public class GuiApi : SunflowAPI
    {
        private string sc;

        public GuiApi(string sc)
        {
            this.sc = sc;
        }

        public override void build()
        {
            parameter("width", (float)(Math.PI * 0.5 / 8192));
            //shader("ao_wire", new Wire());
            // you can put the path to your own scene here to use this rendering technique
            // just copy this file to the same directory as your main .sc file, and swap
            // the fileanme in the line below
            include(sc != null ? sc : "julia.sc.gz");
            shaderOverride("ao_wire", true);

            // this may need to be tweaked if you want really fine lines
            // this is higher than most scenes need so if you render with ambocc = false, make sure you turn down
            // the sampling rates of dof/lights/gi/reflections accordingly
            parameter("aa.min", 0);
            parameter("aa.max", 1);
            parameter("filter", "catmull-rom");//catmull-rom, blackman-harris
            parameter("sampler", "bucket");//ipr or fast or bucket
            options(DEFAULT_OPTIONS);
        }

        public class Wire : WireframeShader
        {
            public bool ambocc = true;

            public override Image.Color getFillColor(ShadingState state)
            {
                return ambocc ? state.occlusion(16, 6.0f) : state.getShader().GetRadiance(state);
            }
        }
    }
}