using System;
using System.Threading;
using System.Collections.Generic;
using SunflowSharp.Core;
using SunflowSharp.Image;
using SunflowSharp.Maths;
using SunflowSharp.Systems;

namespace SunflowSharp.Core.Renderer
{
    public class ProgressiveRenderer : ImageSampler
    {

		private Scene scene;
        private int imageWidth, imageHeight;
        private Queue<SmallBucket> smallBucketQueue;//PriorityBlockingQueue<SmallBucket> smallBucketQueue;//fixme: just a queue of stuff?
        private IDisplay display;
        private int counter, counterMax;

        public ProgressiveRenderer()
        {
            imageWidth = 640;
            imageHeight = 480;
            smallBucketQueue = null;
        }

        public bool prepare(Options options, Scene scene, int w, int h)
        {
            this.scene = scene;
            imageWidth = w;
            imageHeight = h;
            // prepare table used by deterministic anti-aliasing
            return true;
        }

        public void render(IDisplay display)
        {
            this.display = display;
            display.imageBegin(imageWidth, imageHeight, 0);
            // create first bucket
            SmallBucket b = new SmallBucket();
            b.x = b.y = 0;
            int s = Math.Max(imageWidth, imageHeight);
            b.size = 1;
            while (b.size < s)
                b.size <<= 1;
            smallBucketQueue = new Queue<SmallBucket>();//PriorityBlockingQueue<SmallBucket>();
            smallBucketQueue.Enqueue(b);
            UI.taskStart("Progressive Render", 0, imageWidth * imageHeight);
            SunflowSharp.Systems.Timer t = new SunflowSharp.Systems.Timer();
            t.start();
            counter = 0;
            counterMax = imageWidth * imageHeight;

            SmallBucketThread[] renderThreads = new SmallBucketThread[scene.getThreads()];
            for (int i = 0; i < renderThreads.Length; i++)
            {
                renderThreads[i] = new SmallBucketThread(this);
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
                    UI.printError(UI.Module.IPR, "Thread {0} of {1} was interrupted", i + 1, renderThreads.Length);
                }
				finally 
				{
					renderThreads[i].updateStats();
				}

            }
            UI.taskStop();
            t.end();
            UI.printInfo(UI.Module.IPR, "Rendering time: {0}", t.ToString());
            display.imageEnd();
        }

        public class SmallBucketThread //: Thread {
        {
            ProgressiveRenderer renderer;
            Thread thread;
			private IntersectionState istate = new IntersectionState();

            public SmallBucketThread(ProgressiveRenderer renderer)
            {
                this.renderer = renderer;
                thread = new Thread(new ThreadStart(run));
                thread.IsBackground = true;
            }

            public void run()
            {
				ByteUtil.InitByteUtil();
				while (true)
                {
                    int n = renderer.progressiveRenderNext(istate);
                    lock (renderer)// synchronized (ProgressiveRenderer.this) {
                    {
                        if (renderer.counter >= renderer.counterMax)
                            return;
                        renderer.counter += n;
                        UI.taskUpdate(renderer.counter);
                    }
                    if (UI.taskCanceled())
                        return;
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

			public void updateStats() {
				renderer.scene.accumulateStats(istate);
			}

        }

        private int progressiveRenderNext(IntersectionState istate)
        {
            int TASK_SIZE = 16;
            SmallBucket first = smallBucketQueue.Count > 0 ? smallBucketQueue.Dequeue() : null;
            if (first == null)
                return 0;
            int ds = first.size / TASK_SIZE;
            bool useMask = smallBucketQueue.Count != 0;
            int mask = 2 * first.size / TASK_SIZE - 1;
            int pixels = 0;
            for (int i = 0, y = first.y; i < TASK_SIZE && y < imageHeight; i++, y += ds)
            {
                for (int j = 0, x = first.x; j < TASK_SIZE && x < imageWidth; j++, x += ds)
                {
                    // check to see if this is a pixel from a higher level tile
                    if (useMask && (x & mask) == 0 && (y & mask) == 0)
                        continue;
					int instance = ((x & ((1 << QMC.MAX_SIGMA_ORDER) - 1)) << QMC.MAX_SIGMA_ORDER) + QMC.sigma(y & ((1 << QMC.MAX_SIGMA_ORDER) - 1), QMC.MAX_SIGMA_ORDER);
					double time = QMC.halton(1, instance);
                    double lensU = QMC.halton(2, instance);
                    double lensV = QMC.halton(3, instance);
                    ShadingState state = scene.getRadiance(istate, x, imageHeight - 1 - y, lensU, lensV, time, instance, 4, null);
                    Color c = state != null ? state.getResult() : Color.BLACK;
                    pixels++;
                    // fill region
					display.imageFill(x, y, Math.Min(ds, imageWidth - x), Math.Min(ds, imageHeight - y), c, state == null ? 0 : 1);
                }
            }
            if (first.size >= 2 * TASK_SIZE)
            {
                // generate child buckets
                int size = (int)((uint)first.size >> 1);//>>>
                for (int i = 0; i < 2; i++)
                {
                    if (first.y + i * size < imageHeight)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            if (first.x + j * size < imageWidth)
                            {
                                SmallBucket b = new SmallBucket();
                                b.x = first.x + j * size;
                                b.y = first.y + i * size;
                                b.size = size;
                                b.constrast = 1.0f / size;
                                smallBucketQueue.Enqueue(b);
                            }
                        }
                    }
                }
            }
            return pixels;
        }

        // progressive rendering
        private class SmallBucket : IComparable<SmallBucket>
        {
            public int x, y, size;
            public float constrast;

            public int CompareTo(SmallBucket o)
            {
                if (constrast < o.constrast)
                    return -1;
                if (constrast == o.constrast)
                    return 0;
                return 1;
            }
        }
    }
}