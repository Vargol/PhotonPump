using System;
using System.IO;
using SunflowSharp.Core;
using SunflowSharp.Maths;

namespace SunflowSharp.Core.Parser
{

    public class RA3Parser : SceneParserBase
    {
        public override bool parse(Stream stream, SunflowAPI api)
        {
            try
            {
                Console.WriteLine("Unsupported RA3Parser");
                return false;
                //UI.printInfo(UI.Module.USER, "RA3 - Reading geometry: \"%s\" ...", filename);
                //File file = new File(filename);
                //FileInputStream stream = new FileInputStream(filename);
                //MappedByteBuffer map = stream.getChannel().map(FileChannel.MapMode.READ_ONLY, 0, file.Length());
                //map.order(ByteOrder.LITTLE_ENDIAN);
                //IntBuffer ints = map.asIntBuffer();
                //FloatBuffer buffer = map.asFloatBuffer();
                //int numVerts = ints.get(0);
                //int numTris = ints.get(1);
                //UI.printInfo(UI.Module.USER, "RA3 -   * Reading %d vertices ...", numVerts);
                //float[] verts = new float[3 * numVerts];
                //for (int i = 0; i < verts.Length; i++)
                //    verts[i] = buffer.get(2 + i);
                //UI.printInfo(UI.Module.USER, "RA3 -   * Reading %d triangles ...", numTris);
                //int[] tris = new int[3 * numTris];
                //for (int i = 0; i < tris.Length; i++)
                //    tris[i] = ints.get(2 + verts.Length + i);
                //stream.close();
                //UI.printInfo(UI.Module.USER, "RA3 -   * Creating mesh ...");

                //// create geometry
                //api.parameter("triangles", tris);
                //api.parameter("points", "point", "vertex", verts);
                //api.geometry(filename, new TriangleMesh());

                //// create shader
                //IShader s = api.lookupShader("ra3shader");
                //if (s == null)
                //{
                //    // create default shader
                //    api.shader(filename + ".shader", new SimpleShader());
                //    api.parameter("shaders", filename + ".shader");
                //}
                //else
                //{
                //    // reuse existing shader
                //    api.parameter("shaders", "ra3shader");
                //}

                //// create instance
                //api.instance(filename + ".instance", filename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }
    }
}