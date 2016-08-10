using System;
using System.Threading;
using SunflowSharp.Core;
using SunflowSharp.Image;
using SunflowSharp.Systems;

namespace SunflowSharp.Core.Renderer
{
    public class SimpleRenderer : ImageSampler
    {
        private Scene scene;
        private IDisplay display;
        private int imageWidth, imageHeight;
        private uint numBucketsX, numBucketsY;//was int
        private uint bucketCounter, numBuckets;

        public bool prepare(Options options, Scene scene, int w, int h)
        {
            this.scene = scene;
            imageWidth = w;
            imageHeight = h;
            numBucketsX = ((uint)imageWidth + 31) >> 5;//>>>
            numBucketsY = ((uint)imageHeight + 31) >> 5;//>>>
            numBuckets = numBucketsX * numBucketsY;
            return true;
        }

        public void render(IDisplay display)
        {
            this.display = display;
            display.imageBegin(imageWidth, imageHeight, 32);
            // set members variables
            bucketCounter = 0;
            // start task
            SunflowSharp.Systems.Timer timer = new SunflowSharp.Systems.Timer();
            timer.start();
            BucketThread[] renderThreads = new BucketThread[scene.getThreads()];
            for (int i = 0; i < renderThreads.Length; i++)
            {
                renderThreads[i] = new BucketThread(this);
                renderThreads[i].setPriority(scene.getThreadPriority());
                renderThreads[i].start();
            }
            for (int i = 0; i < renderThreads.Length; i++)
            {
                try
                {
                    renderThreads[i].join();
                }
                catch (Exception e)
                {
                    UI.printError(UI.Module.BCKT, "Bucket processing thread {0} of {1} was interrupted", i + 1, renderThreads.Length);
                }
				finally
				{
					renderThreads[i].updateStats();
				}
            }
            timer.end();
            UI.printInfo(UI.Module.BCKT, "Render time: {0}", timer.ToString());
            display.imageEnd();
        }

        public class BucketThread// : Thread {
        {
            SimpleRenderer renderer;
            Thread thread;
			private IntersectionState istate = new IntersectionState();

            public BucketThread(SimpleRenderer renderer)
            {
                this.renderer = renderer;
                thread = new Thread(new ThreadStart(run));
                thread.IsBackground = true;
            }

            public void run()
            {
				//ByteUtil.InitByteUtil();
				while (true)
                {
                    uint bx, by;
                    lock (renderer)//synchronized (SimpleRenderer.this) {
                    {
                        if (renderer.bucketCounter >= renderer.numBuckets)
                            return;
                        by = renderer.bucketCounter / renderer.numBucketsX;
                        bx = renderer.bucketCounter % renderer.numBucketsX;
                        renderer.bucketCounter++;
                    }
                    renderer.renderBucket(bx, by, istate);
                }
            }

            public void setPriority(ThreadPriority prior)
            {
                thread.Priority = prior;
            }

            public void start()
            {
                thread.Start();
            }

            public void join()
            {
                thread.Join();
            }

			public void updateStats() 
			{
				renderer.scene.accumulateStats(istate);
			}
        }

        public void renderBucket(uint bx, uint by, IntersectionState istate)
        {
            // pixel sized extents
            int x0 = (int)(bx * 32);
            int y0 = (int)(by * 32);
            int bw = Math.Min(32, imageWidth - x0);
            int bh = Math.Min(32, imageHeight - y0);

            Color[] bucketRGB = new Color[bw * bh];
			float[] bucketAlpha = new float[bw * bh];

            for (int y = 0, i = 0; y < bh; y++)
            {
                for (int x = 0; x < bw; x++, i++)
                {
                    ShadingState state = scene.getRadiance(istate, x0 + x, imageHeight - 1 - (y0 + y), 0.0, 0.0, 0.0, 0, 0, null);
                    bucketRGB[i] = (state != null) ? state.getResult() : Color.BLACK;
					bucketAlpha[i] = (state != null) ? 1 : 0;
				}
            }
            // update pixels
            display.imageUpdate(x0, y0, bw, bh, bucketRGB, bucketAlpha);
        }
    }
}