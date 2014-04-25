using System;
using System.IO;
using SunflowSharp.Core;
using SunflowSharp.Core.Primitive;
using SunflowSharp.Core.Shader;
using SunflowSharp.Systems;

namespace SunflowSharp.Core.Parser
{
    public class TriParser : SceneParserBase
    {
        public override bool parse(Stream stream, SunflowAPI api)
        {
            try
            {
                //UI.printInfo(UI.Module.USER, "TRI - Reading geometry: \"{0}\" ...", filename);
                //Systems.Parser p = new Systems.Parser(stream);
                //float[] verts = new float[3 * p.getNextInt()];
                //for (int v = 0; v < verts.Length; v += 3)
                //{
                //    verts[v + 0] = p.getNextFloat();
                //    verts[v + 1] = p.getNextFloat();
                //    verts[v + 2] = p.getNextFloat();
                //    p.getNextToken();
                //    p.getNextToken();
                //}
                //int[] triangles = new int[p.getNextInt() * 3];
                //for (int t = 0; t < triangles.Length; t += 3)
                //{
                //    triangles[t + 0] = p.getNextInt();
                //    triangles[t + 1] = p.getNextInt();
                //    triangles[t + 2] = p.getNextInt();
                //}

                //// create geometry
                //api.parameter("triangles", triangles);
                //api.parameter("points", "point", "vertex", verts);
                //api.geometry(filename, new TriangleMesh());

                //// create shader
                //api.shader(filename + ".shader", new SimpleShader());
                //api.parameter("shaders", filename + ".shader");

                //// create instance
                //api.instance(filename + ".instance", filename);

                //p.close();
                // output to ra3 format
                Console.WriteLine("Not supported");
                //RandomAccessFile stream = new RandomAccessFile(filename.replace(".tri", ".ra3"), "rw");
                //MappedByteBuffer map = stream.getChannel().map(MapMode.READ_WRITE, 0, 8 + 4 * (verts.Length + triangles.Length));
                //map.order(ByteOrder.LITTLE_ENDIAN);
                //IntBuffer ints = map.asIntBuffer();
                //FloatBuffer floats = map.asFloatBuffer();
                //ints.put(0, verts.Length / 3);
                //ints.put(1, triangles.Length / 3);
                //for (int i = 0; i < verts.Length; i++)
                //    floats.put(2 + i, verts[i]);
                //for (int i = 0; i < triangles.Length; i++)
                //    ints.put(2 + verts.Length + i, triangles[i]);
                //stream.close();
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