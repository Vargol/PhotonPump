using System;
using SunflowSharp.Core;
using SunflowSharp.Image;
using SunflowSharp.Maths;
using SunflowSharp.Systems;

namespace SunflowSharp
{
    public class Benchmark : BenchmarkTest, UserInterface, IDisplay
    {
        private int resolution;
        private bool showOutput;
        private bool showBenchmarkOutput;
        private bool saveOutput;
        private int threads;
        private int[] referenceImage;
        private int[] validationImage;
        private int errorThreshold;

        public static void main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Benchmark options:");
                Console.WriteLine("  -regen                        Regenerate reference images for a variety of sizes");
                Console.WriteLine("  -bench [threads] [resolution] Run a single iteration of the benchmark using the specified thread count and image resolution");
                Console.WriteLine("                                Default: threads=0 (auto-detect cpus), resolution=256");
            }
            else if (args[0] == "-regen")
            {
                int[] sizes = { 32, 64, 96, 128, 256, 384, 512 };
                foreach (int s in sizes)
                {
                    // run a single iteration to generate the reference image
                    Benchmark b = new Benchmark(s, true, false, true);
                    b.kernelMain();
                }
            }
            else if (args[0] == "-bench")
            {
                int threads = 0, resolution = 256;
                if (args.Length > 1)
                    threads = int.Parse(args[1]);
                if (args.Length > 2)
                    resolution = int.Parse(args[2]);
                Benchmark benchmark = new Benchmark(resolution, false, true, false, threads);
                benchmark.kernelBegin();
                benchmark.kernelMain();
                benchmark.kernelEnd();
            }
        }

        public Benchmark()
            : this(384, false, true, false)
        {
        }

        public Benchmark(int resolution, bool showOutput, bool showBenchmarkOutput, bool saveOutput)
            : this(resolution, showOutput, showBenchmarkOutput, saveOutput, 0)
        {
        }

        public Benchmark(int resolution, bool showOutput, bool showBenchmarkOutput, bool saveOutput, int threads)
        {
            UI.set(this);
            this.resolution = resolution;
            this.showOutput = showOutput;
            this.showBenchmarkOutput = showBenchmarkOutput;
            this.saveOutput = saveOutput;
            this.threads = threads;
            errorThreshold = 6;
            // fetch reference image from resources (jar file or classpath)
            if (saveOutput)
                return;
            URL imageURL = getResource(string.Format("/resources/golden_{0}.png", resolution));//fixme: add padding zeros
            if (imageURL == null)
                UI.printError(UI.Module.BENCH, "Unable to find reference frame!");
            UI.printInfo(UI.Module.BENCH, "Loading reference image from: %s", imageURL);
            try
            {
                BufferedImage bi = ImageIO.read(imageURL);
                if (bi.getWidth() != resolution || bi.getHeight() != resolution)
                    UI.printError(UI.Module.BENCH, "Reference image has invalid resolution! Expected %dx%d found %dx%d", resolution, resolution, bi.getWidth(), bi.getHeight());
                referenceImage = new int[resolution * resolution];
                for (int y = 0, i = 0; y < resolution; y++)
                    for (int x = 0; x < resolution; x++, i++)
                        referenceImage[i] = bi.getRGB(x, resolution - 1 - y); // flip
            }
            catch (Exception e)
            {
                UI.printError(UI.Module.BENCH, "Unable to load reference frame!");
            }
        }

        public void execute()
        {
            // 10 iterations maximum - 10 minute time limit
            BenchmarkFramework framework = new BenchmarkFramework(10, 600);
            framework.execute(this);
        }

        private class BenchmarkScene : SunflowAPI
        {
            Benchmark benchmark;
            public BenchmarkScene(Benchmark benchmark)
            {
                this.benchmark = benchmark;
                build();
                render(SunflowAPI.DEFAULT_OPTIONS, saveOutput ? new FileDisplay(string.Format("resources/golden_{0}.png", resolution)) : benchmark);
                //fixme: add padding zeros
            }

            public void build()
            {
                // settings
                parameter("threads", threads);
                // spawn regular priority threads
                parameter("threads.lowPriority", false);
                parameter("resolutionX", resolution);
                parameter("resolutionY", resolution);
                parameter("aa.min", -1);
                parameter("aa.max", 1);
                parameter("filter", "triangle");
                parameter("depths.diffuse", 2);
                parameter("depths.reflection", 2);
                parameter("depths.refraction", 2);
                parameter("bucket.order", "hilbert");
                parameter("bucket.size", 32);
                // gi options
                parameter("gi.engine", "igi");
                parameter("gi.igi.samples", 90);
                parameter("gi.igi.c", 0.000008f);
                options(SunflowAPI.DEFAULT_OPTIONS);
                buildCornellBox();
            }

            private void buildCornellBox()
            {
                // camera
				parameter("transform", Matrix4.lookAt(new Point3(0, 0, -600), new Point3(0, 0, 0), new Vector3(0, 1, 0)));
				parameter("fov", 45.0f);
                camera("main_camera", "pinhole");
                parameter("camera", "main_camera");
                options(SunflowAPI.DEFAULT_OPTIONS);
                // cornell box
                Color gray = new Color(0.70f, 0.70f, 0.70f);
                Color blue = new Color(0.25f, 0.25f, 0.80f);
                Color red = new Color(0.80f, 0.25f, 0.25f);
                Color emit = new Color(15, 15, 15);

                float minX = -200;
                float maxX = 200;
                float minY = -160;
                float maxY = minY + 400;
                float minZ = -250;
                float maxZ = 200;

                float[] verts = new float[] { minX, minY, minZ, maxX, minY, minZ,
                    maxX, minY, maxZ, minX, minY, maxZ, minX, maxY, minZ, maxX,
                    maxY, minZ, maxX, maxY, maxZ, minX, maxY, maxZ, };
                int[] indices = new int[] { 0, 1, 2, 2, 3, 0, 4, 5, 6, 6, 7, 4, 1,
                    2, 5, 5, 6, 2, 2, 3, 6, 6, 7, 3, 0, 3, 4, 4, 7, 3 };

                parameter("diffuse", gray);
                shader("gray_shader", "diffuse");
                parameter("diffuse", red);
				shader("red_shader", "diffuse");
                parameter("diffuse", blue);
				shader("blue_shader", "diffuse");

                // build walls
                parameter("triangles", indices);
                parameter("points", "point", "vertex", verts);
                parameter("faceshaders", new int[] { 0, 0, 0, 0, 1, 1, 0, 0, 2, 2 });
				geometry("walls", "triangle_mesh");

                // instance walls
                parameter("shaders", new string[] { "gray_shader", "red_shader",
                    "blue_shader" });
                instance("walls.instance", "walls");

                // create mesh light
                parameter("points", "point", "vertex", new float[] { -50, maxY - 1,
                    -50, 50, maxY - 1, -50, 50, maxY - 1, 50, -50, maxY - 1, 50 });
                parameter("triangles", new int[] { 0, 1, 2, 2, 3, 0 });
                parameter("radiance", emit);
                parameter("samples", 8);
				light("light", "triangle_mesh");

                // spheres
                parameter("eta", 1.6f);
                shader("Glass", "glass");
                sphere("glass_sphere", "Glass", -120, minY + 55, -150, 50);
                parameter("color", new Color(0.70f, 0.70f, 0.70f));
                shader("Mirror", "mirror");
                sphere("mirror_sphere", "Mirror", 100, minY + 60, -50, 50);

                // scanned model
                geometry("teapot", "teapot");
                parameter("transform", Matrix4.translation(80, -50, 100).multiply(Matrix4.rotateX((float)-Math.PI / 6)).multiply(Matrix4.rotateY((float)Math.PI / 4)).multiply(Matrix4.rotateX((float)-Math.PI / 2).multiply(Matrix4.scale(1.2f))));
                parameter("shaders", "gray_shader");
                instance("teapot.instance1", "teapot");
                parameter("transform", Matrix4.translation(-80, -160, 50).multiply(Matrix4.rotateY((float)Math.PI / 4)).multiply(Matrix4.rotateX((float)-Math.PI / 2).multiply(Matrix4.scale(1.2f))));
                parameter("shaders", "gray_shader");
                instance("teapot.instance2", "teapot");
            }

            private void sphere(string name, string shaderName, float x, float y, float z, float radius)
            {
                geometry(name, "sphere");
                parameter("transform", Matrix4.translation(x, y, z).multiply(Matrix4.scale(radius)));
                parameter("shaders", shaderName);
                instance(name + ".instance", name);
            }
        }

        public void kernelBegin()
        {
            // allocate a fresh validation target
            validationImage = new int[resolution * resolution];
        }

        public void kernelMain()
        {
            // this builds and renders the scene
            new BenchmarkScene(this);
        }

        public void kernelEnd()
        {
            // make sure the rendered image was correct
            int diff = 0;
            if (referenceImage != null && validationImage.Length == referenceImage.Length)
            {
                for (int i = 0; i < validationImage.Length; i++)
                {
                    // count absolute RGB differences
                    diff += Math.Abs((validationImage[i] & 0xFF) - (referenceImage[i] & 0xFF));
                    diff += Math.Abs(((validationImage[i] >> 8) & 0xFF) - ((referenceImage[i] >> 8) & 0xFF));
                    diff += Math.Abs(((validationImage[i] >> 16) & 0xFF) - ((referenceImage[i] >> 16) & 0xFF));
                }
                if (diff > errorThreshold)
                    UI.printError(UI.Module.BENCH, "Image check failed! - #errors: %d", diff);
                else
                    UI.printInfo(UI.Module.BENCH, "Image check passed!");
            }
            else
                UI.printError(UI.Module.BENCH, "Image check failed! - reference is not comparable");

        }

        public void print(UI.Module m, UI.PrintLevel level, string s)
        {
            if (showOutput || (showBenchmarkOutput && m == Module.BENCH))
                Console.WriteLine(UI.formatOutput(m, level, s));
            if (level == PrintLevel.ERROR)
                throw new RuntimeException(s);
        }

        public void taskStart(string s, int min, int max)
        {
            // render progress display not needed
        }

        public void taskStop()
        {
            // render progress display not needed
        }

        public void taskUpdate(int current)
        {
            // render progress display not needed
        }

        public void imageBegin(int w, int h, int bucketSize)
        {
            // we can assume w == h == resolution
        }

        public void imageEnd()
        {
            // nothing needs to be done - image verification is done externally
        }

		public void imageFill(int x, int y, int w, int h, Color c, float alpha)
        {
            // this is not used
        }

        public void imagePrepare(int x, int y, int w, int h, int id)
        {
            // this is not needed
        }

		public void imageUpdate(int x, int y, int w, int h, Color[] data, float[] alpha)
        {
            // copy bucket data to validation image
            for (int j = 0, index = 0; j < h; j++, y++)
                for (int i = 0, offset = x + resolution * (resolution - 1 - y); i < w; i++, index++, offset++)
                    validationImage[offset] = data[index].copy().toNonLinear().toRGB();
        }
    }
}