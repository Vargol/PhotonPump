using System;
using SunflowSharp;
using SunflowSharp.Core;
using SunflowSharp.Core.Camera;
using SunflowSharp.Core.Display;
using SunflowSharp.Core.Light;
using SunflowSharp.Core.Shader;
using SunflowSharp.Image;
using SunflowSharp.Maths;
using Jitter;
using Jitter.Collision;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;

namespace PhysicsSim
{
    

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                WorldSim world = new WorldSim();
                world.RunSimulation();
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }
    }

    public class WorldSim 
    {




		public void RunSimulation()
        {

			CollisionSystem collision = new CollisionSystemSAP();
			JitterWorld world = new JitterWorld(collision);

            world.Gravity = new JVector(0, -10, -0);

			Shape shape = new BoxShape(20.0f, 1.0f, 20.0f);
			RigidBody floor = new RigidBody(shape);
            floor.IsStatic = true;
			floor.Position = new JVector(0.0f, -15.0f, 0.0f);

            shape = new SphereShape(0.5f);
			RigidBody body = new RigidBody(shape);
            body.Position = new JVector(0.0f, 3.0f, 0.0f);
            body.Material.Restitution = 0.8f;

			world.AddBody(floor);
			world.AddBody(body);

            for (int i = 0; i < 600; i++)
            {
                world.Step(1.0f / 30.0f, true);

                SunflowAPI sunflow = new SunflowAPI();
                SetupSunflow(sunflow);

                sunflow.geometry("sphere", "sphere");

                //Instancing the big metal sphere.
                JVector v = body.Position;
                sunflow.parameter("transform", Matrix4.translation(v.X, v.Y, v.Z).multiply(Matrix4.scale(1)));
                sunflow.parameter("shaders", "metal");
                sunflow.instance("sphere.instance", "sphere");


                sunflow.render(SunflowAPI.DEFAULT_OPTIONS, new FileDisplay("spherecube" + i + ".png"));


                // do other stuff, like drawing
            }
        }

        public void SetupSunflow(SunflowAPI a) {

            a.parameter("threads", Environment.ProcessorCount);
			//          parameter ("threads", 1);
			a.options(SunflowAPI.DEFAULT_OPTIONS);
			//The render's resolution. 1920 by 1080 is full HD.
			int resolutionX = 3840;
			int resolutionY = 1920;
			a.parameter("resolutionX", resolutionX);
			a.parameter("resolutionY", resolutionY);

			//The anti-aliasing. Negative is subsampling and positive is supersampling.
			a.parameter("aa.min", 1);
			a.parameter("aa.max", 2);

			//Number of samples.
			a.parameter("aa.samples", 1);

			//The contrast needed to increase anti-aliasing.
			a.parameter("aa.contrast", .016f);

			//Subpixel jitter.
			a.parameter("aa.jitter", true);

			//The filter.
			a.parameter("filter", "mitchell");
			a.options(SunflowAPI.DEFAULT_OPTIONS);

			//Aspect Ratio.
			float aspect = ((float)resolutionX) / ((float)resolutionY);

            //Set up the camera.
            Point3 eye = new Point3(0.0f, -10.0f, -15.0f);
            Point3 target = new Point3(0, -10.0f, 0);
            Vector3 up = new Vector3(0, 1, 0);

            //            a.parameter("eye", new Point3(0.0f, 0.0f, 0.0f));
            //			a.parameter("target", new Point3(0, -16, 0));
            //			a.parameter("up", new Vector3(0, 1, 0));

            a.parameter("transform", Matrix4.lookAt(eye, target, up));
            a.parameter("lens.eyegap", 0.5f);
//			a.parameter("fov", 60f);
//			a.parameter("aspect", aspect);
			String name = "Camera";
            a.camera(name, "spherical3d");
			a.parameter("camera", name);
			a.options(SunflowAPI.DEFAULT_OPTIONS);

			//Trace depths. Higher numbers look better.
			a.parameter("depths.diffuse", 1);
			a.parameter("depths.reflection", 2);
			a.parameter("depths.refraction", 2);
			a.options(SunflowAPI.DEFAULT_OPTIONS);

			//Setting up the shader for the ground.
			a.parameter("diffuse", null, 0.4f, 0.4f, 0.4f);
			a.parameter("shiny", .1f);
            a.shader("ground",  "shiny_diffuse");
			a.options(SunflowAPI.DEFAULT_OPTIONS);

			//Setting up the shader for the big metal sphere.
			a.parameter("diffuse",  null, 0.3f, 0.3f, 0.3f);
			a.parameter("shiny", .95f);
			a.shader("metal",  "shiny_diffuse");
			a.options(SunflowAPI.DEFAULT_OPTIONS);


			//Setting up the shader for the cube of spheres.
			a.parameter("diffuse", null, 1.0f , 1.0f, 1.0f);
            a.shader("sps", "diffuse");
			a.options(SunflowAPI.DEFAULT_OPTIONS);

			//Instancing the floor.
			a.parameter("center", new Point3(0, -14.5f, 0));
			a.parameter("normal", new Vector3(0, 1, 0));
			a.geometry("floor", "plane");
			a.parameter("shaders", "ground");
			a.instance("FloorInstance", "floor");
			a.options(SunflowAPI.DEFAULT_OPTIONS);

			//Creating the lighting system with the sun and sky.
			a.parameter("up", new Vector3(0, 1, 0));
			a.parameter("east", new Vector3(1, 0, 0));
//            double sunRad = (Math.PI * 1.05);
//			a.parameter("sundir", new Vector3((float)Math.Cos(sunRad), (float)Math.Sin(sunRad), (float)(.5 * Math.Sin(sunRad))).normalize());
			a.parameter("sundir", new Vector3(0.8f, 0.8f, 0.5f).normalize());
			a.parameter("turbidity", 4f);
			a.parameter("samples", 4);
            a.light("sunsky", "sunsky");
			a.options(SunflowAPI.DEFAULT_OPTIONS);

		}
    }


}