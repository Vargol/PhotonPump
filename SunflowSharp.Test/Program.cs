using System;
using System.Collections.Generic;
using System.Text;
using SunflowSharp;
using SunflowSharp.Core;
using SunflowSharp.Core.Shader;
using SunflowSharp.Core.Display;
using SunflowSharp.Image;

namespace SunflowSharp.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                test test = new test(args.Length > 0 ? args[0] : null);
                test.build();
				test.render("::options", new FileDisplay(string.Format("output{0:yyyy-MM-dd_hh-mm-ss-tt}.exr", DateTime.Now)));//new FileDisplay("output.tga"));
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }
    }

    public class test : SunflowAPI
    {
        private string sc;

        public test(string sc)
        {
            this.sc = sc;
        }

        public override void build()
        {
//            parameter("width", (float)(Math.PI * 0.5 / 8192));
            //shader("ao_wire", new wireframetest());
            // you can put the path to your own scene here to use this rendering technique
            // just copy this file to the same directory as your main .sc file, and swap
            // the fileanme in the line below
            include(sc != null ? sc : "bump_demo.sc.gz");
			//            shaderOverride("ao_wire", true);

			// this may need to be tweaked if you want really fine lines
			// this is higher than most scenes need so if you render with ambocc = false, make sure you turn down
			// the sampling rates of dof/lights/gi/reflections accordingly
			//            parameter("aa.min", 1);
			//            parameter("aa.max", 2);
			//            parameter("filter", "catmull-rom");//catmull-rom, blackman-harris
			//			parameter("sampler", "bucket");//ipr or fast or bucket
			parameter("threads", Environment.ProcessorCount);
//			parameter ("threads", 1);
            options(DEFAULT_OPTIONS);
        }
    }

    public class wireframetest : WireframeShader
    {
        public bool ambocc = true;

        public override Color getFillColor(ShadingState state)
        {
            return ambocc ? state.occlusion(16, 6.0f) : state.getShader().GetRadiance(state);
        }
    }
}
