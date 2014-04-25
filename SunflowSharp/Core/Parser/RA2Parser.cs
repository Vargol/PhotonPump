using System;
using System.IO;
using SunflowSharp.Core;
using SunflowSharp.Maths;
using SunflowSharp.Systems;

namespace SunflowSharp.Core.Parser
{

    public class RA2Parser : SceneParserBase
    {
        public override bool parse(Stream stream, SunflowAPI api)
        {
            Console.WriteLine("Unsupported RA2Parser");
            //try
            //{
            //    UI.printInfo(UI.Module.USER, "RA2 - Reading geometry: \"{0}\" ...", filename);
            //    //File file = new File(filename);
            //    //FileInputStream stream = new FileInputStream(filename);
            //    //MappedByteBuffer map = stream.getChannel().map(FileChannel.MapMode.READ_ONLY, 0, file.Length());
            //    //map.order(ByteOrder.LITTLE_ENDIAN);
            //    //FloatBuffer buffer = map.asFloatBuffer();

            //    BinaryReader reader = new BinaryReader(stream);
            //    float[] data = new float[reader.BaseStream.Length / 4];
            //    for (int i = 0; i < data.Length; i++)
            //        data[i] = reader.ReadSingle();
            //    reader.Close();
            //    api.parameter("points", "point", "vertex", data);
            //    int[] triangles = new int[3 * (data.Length / 9)];
            //    for (int i = 0; i < triangles.Length; i++)
            //        triangles[i] = i;
            //    // create geo
            //    api.parameter("triangles", triangles);
            //    api.geometry(filename, new TriangleMesh());
            //    // create shader
            //    api.shader(filename + ".shader", new SimpleShader());
            //    // create instance
            //    api.parameter("shaders", filename + ".shader");
            //    api.instance(filename + ".instance", filename);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    return false;
            //}
            //try
            //{
            //    filename = filename.Replace(".ra2", ".txt");
            //    UI.printInfo(UI.Module.USER, "RA2 - Reading camera  : \"{0}\" ...", filename);
            //    Systems.Parser p = new Systems.Parser(filename);
            //    Point3 eye = new Point3();
            //    eye.x = p.getNextFloat();
            //    eye.y = p.getNextFloat();
            //    eye.z = p.getNextFloat();
            //    Point3 to = new Point3();
            //    to.x = p.getNextFloat();
            //    to.y = p.getNextFloat();
            //    to.z = p.getNextFloat();
            //    Vector3 up = new Vector3();
            //    switch (p.getNextInt())
            //    {
            //        case 0:
            //            up.set(1, 0, 0);
            //            break;
            //        case 1:
            //            up.set(0, 1, 0);
            //            break;
            //        case 2:
            //            up.set(0, 0, 1);
            //            break;
            //        default:
            //            UI.printWarning(UI.Module.USER, "RA2 - Invalid up vector specification - using Z axis");
            //            up.set(0, 0, 1);
            //            break;
            //    }
            //    api.parameter("eye", eye);
            //    api.parameter("target", to);
            //    api.parameter("up", up);
            //    string name = api.getUniqueName("camera");
            //    api.parameter("fov", 80f);
            //    api.camera(name, new PinholeLens());
            //    api.parameter("camera", name);
            //    api.parameter("resolutionX", 1024);
            //    api.parameter("resolutionY", 1024);
            //    api.options(SunflowAPI.DEFAULT_OPTIONS);
            //    p.close();
            //}
            //catch (Exception e)
            //{
            //    UI.printWarning(UI.Module.USER, "RA2 - Camera file not found");
            //}
            return true;
        }
    }
}