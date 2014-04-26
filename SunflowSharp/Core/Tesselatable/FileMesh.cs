using System;
using System.IO;
using System.Collections.Generic;
using SunflowSharp.Core;
using SunflowSharp.Core.Primitive;
using SunflowSharp.Maths;
using SunflowSharp.Systems;

namespace SunflowSharp.Core.Tesselatable
{

    public class FileMesh : ITesselatable
    {
        private string filename = null;
        private bool smoothNormals = false;

        public BoundingBox getWorldBounds(Matrix4 o2w)
        {
            // world bounds can't be computed without reading file
            // return null so the mesh will be loaded right away
            return null;
        }

        public PrimitiveList tesselate()
        {
            if (filename.EndsWith(".ra3"))
            {
                Console.WriteLine("RA3 unsupported");
                //try
                //{
                //    UI.printInfo(UI.Module.GEOM, "RA3 - Reading geometry: \"%s\" ...", filename);
                //    File file = new File(filename);
                //    FileInputStream stream = new FileInputStream(filename);
                //    MappedByteBuffer map = stream.getChannel().map(FileChannel.MapMode.READ_ONLY, 0, file.Length());
                //    map.order(ByteOrder.LITTLE_ENDIAN);
                //    IntBuffer ints = map.asIntBuffer();
                //    FloatBuffer buffer = map.asFloatBuffer();
                //    int numVerts = ints.get(0);
                //    int numTris = ints.get(1);
                //    UI.printInfo(UI.Module.GEOM, "RA3 -   * Reading %d vertices ...", numVerts);
                //    float[] verts = new float[3 * numVerts];
                //    for (int i = 0; i < verts.Length; i++)
                //        verts[i] = buffer.get(2 + i);
                //    UI.printInfo(UI.Module.GEOM, "RA3 -   * Reading %d triangles ...", numTris);
                //    int[] tris = new int[3 * numTris];
                //    for (int i = 0; i < tris.Length; i++)
                //        tris[i] = ints.get(2 + verts.Length + i);
                //    stream.close();
                //    UI.printInfo(UI.Module.GEOM, "RA3 -   * Creating mesh ...");
                //    return generate(tris, verts, smoothNormals);
                //}
                //catch (FileNotFoundException e)
                //{
                //    e.printStackTrace();
                //    UI.printError(UI.Module.GEOM, "Unable to read mesh file \"%s\" - file not found", filename);
                //}
                //catch (IOException e)
                //{
                //    e.printStackTrace();
                //    UI.printError(UI.Module.GEOM, "Unable to read mesh file \"%s\" - I/O error occured", filename);
                //}
            }
            else if (filename.EndsWith(".obj"))
            {
                int lineNumber = 1;
                try
                {
                    UI.printInfo(UI.Module.GEOM, "OBJ - Reading geometry: \"%s\" ...", filename);
                    List<float> verts = new List<float>();
                    List<int> tris = new List<int>();
                    //FileReader file = new FileReader(filename);
                    //BufferedReader bf = new BufferedReader(file);
                    StreamReader bf = new StreamReader(filename); 
                    string line;
                    while ((line = bf.ReadLine()) != null)
                    {
                        if (line.StartsWith("v"))
                        {
                            string[] v = line.Split(StringConsts.Whitespace, StringSplitOptions.RemoveEmptyEntries);//"\\s+");
                            verts.Add(float.Parse(v[1]));
                            verts.Add(float.Parse(v[2]));
                            verts.Add(float.Parse(v[3]));
                        }
                        else if (line.StartsWith("f"))
                        {
                            string[] f = line.Split(StringConsts.Whitespace, StringSplitOptions.RemoveEmptyEntries);//"\\s+");
                            if (f.Length == 5)
                            {
                                tris.Add(int.Parse(f[1]) - 1);
                                tris.Add(int.Parse(f[2]) - 1);
                                tris.Add(int.Parse(f[3]) - 1);
                                tris.Add(int.Parse(f[1]) - 1);
                                tris.Add(int.Parse(f[3]) - 1);
                                tris.Add(int.Parse(f[4]) - 1);
                            }
                            else if (f.Length == 4)
                            {
                                tris.Add(int.Parse(f[1]) - 1);
                                tris.Add(int.Parse(f[2]) - 1);
                                tris.Add(int.Parse(f[3]) - 1);
                            }
                        }
                        if (lineNumber % 100000 == 0)
                            UI.printInfo(UI.Module.GEOM, "OBJ -   * Parsed {0} lines ...", lineNumber);
                        lineNumber++;
                    }
                    //file.close();
                    bf.Close();
                    UI.printInfo(UI.Module.GEOM, "OBJ -   * Creating mesh ...");
                    return generate(tris.ToArray(), verts.ToArray(), smoothNormals);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    UI.printError(UI.Module.GEOM, "Unable to read mesh file \"{0}\" - file not found", filename);
                }
            }
            else if (filename.EndsWith(".stl"))
            {
                try
                {
                    UI.printInfo(UI.Module.GEOM, "STL - Reading geometry: \"%s\" ...", filename);
                    //FileInputStream file = new FileInputStream(filename);
                    //DataInputStream stream = new DataInputStream(new BufferedInputStream(file));
                    BinaryReader stream = new BinaryReader(File.OpenRead(filename));
                    //file.skip(80);
                    stream.BaseStream.Seek(80, SeekOrigin.Current);
                    int numTris = getLittleEndianInt(stream.ReadInt32());
                    UI.printInfo(UI.Module.GEOM, "STL -   * Reading {0} triangles ...", numTris);
                    long filesize = stream.BaseStream.Length;
                    if (filesize != (84 + 50 * numTris))
                    {
                        UI.printWarning(UI.Module.GEOM, "STL - Size of file mismatch (expecting %s, found %s)", Memory.bytesTostring(84 + 14 * numTris), Memory.bytesTostring(filesize));
                        return null;
                    }
                    int[] tris = new int[3 * numTris];
                    float[] verts = new float[9 * numTris];
                    for (int i = 0, i3 = 0, index = 0; i < numTris; i++, i3 += 3)
                    {
                        // skip normal
                        stream.ReadInt32();
                        stream.ReadInt32();
                        stream.ReadInt32();
                        for (int j = 0; j < 3; j++, index += 3)
                        {
                            tris[i3 + j] = i3 + j;
                            // get xyz
                            verts[index + 0] = getLittleEndianFloat(stream.ReadInt32());
                            verts[index + 1] = getLittleEndianFloat(stream.ReadInt32());
                            verts[index + 2] = getLittleEndianFloat(stream.ReadInt32());
                        }
                        stream.ReadInt16();
                        if ((i + 1) % 100000 == 0)
                            UI.printInfo(UI.Module.GEOM, "STL -   * Parsed {0} triangles ...", i + 1);
                    }
                    stream.Close();
                    //file.close();
                    // create geometry
                    UI.printInfo(UI.Module.GEOM, "STL -   * Creating mesh ...");
                    if (smoothNormals)
                        UI.printWarning(UI.Module.GEOM, "STL - format does not support shared vertices - normal smoothing disabled");
                    return generate(tris, verts, false);
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e);
                    UI.printError(UI.Module.GEOM, "Unable to read mesh file \"{0}\" - file not found", filename);
                }
            }
            else
                UI.printWarning(UI.Module.GEOM, "Unable to read mesh file \"{0}\" - unrecognized format", filename);
            return null;
        }

        private TriangleMesh generate(int[] tris, float[] verts, bool smoothNormals)
        {
            ParameterList pl = new ParameterList();
            pl.addIntegerArray("triangles", tris);
            pl.addPoints("points", ParameterList.InterpolationType.VERTEX, verts);
            if (smoothNormals)
            {
                float[] normals = new float[verts.Length]; // filled with 0's
                Point3 p0 = new Point3();
                Point3 p1 = new Point3();
                Point3 p2 = new Point3();
                Vector3 n = new Vector3();
                for (int i3 = 0; i3 < tris.Length; i3 += 3)
                {
                    int v0 = tris[i3 + 0];
                    int v1 = tris[i3 + 1];
                    int v2 = tris[i3 + 2];
                    p0.set(verts[3 * v0 + 0], verts[3 * v0 + 1], verts[3 * v0 + 2]);
                    p1.set(verts[3 * v1 + 0], verts[3 * v1 + 1], verts[3 * v1 + 2]);
                    p2.set(verts[3 * v2 + 0], verts[3 * v2 + 1], verts[3 * v2 + 2]);
                    Point3.normal(p0, p1, p2, n); // compute normal
                    // add face normal to each vertex
                    // note that these are not normalized so this in fact weights
                    // each normal by the area of the triangle
                    normals[3 * v0 + 0] += n.x;
                    normals[3 * v0 + 1] += n.y;
                    normals[3 * v0 + 2] += n.z;
                    normals[3 * v1 + 0] += n.x;
                    normals[3 * v1 + 1] += n.y;
                    normals[3 * v1 + 2] += n.z;
                    normals[3 * v2 + 0] += n.x;
                    normals[3 * v2 + 1] += n.y;
                    normals[3 * v2 + 2] += n.z;
                }
                // normalize all the vectors
                for (int i3 = 0; i3 < normals.Length; i3 += 3)
                {
                    n.set(normals[i3 + 0], normals[i3 + 1], normals[i3 + 2]);
                    n.normalize();
                    normals[i3 + 0] = n.x;
                    normals[i3 + 1] = n.y;
                    normals[i3 + 2] = n.z;
                }
                pl.addVectors("normals", ParameterList.InterpolationType.VERTEX, normals);
            }
            TriangleMesh m = new TriangleMesh();
            if (m.Update(pl, null))
                return m;
            // something failed in creating the mesh, the error message will be
            // printed by the mesh itself - no need to repeat it here
            return null;
        }

        public bool Update(ParameterList pl, SunflowAPI api)
        {
            string file = pl.getstring("filename", null);
            if (file != null)
                filename = api.resolveIncludeFilename(file);
            smoothNormals = pl.getbool("smooth_normals", smoothNormals);
            return filename != null;
        }

        private int getLittleEndianInt(int i)
        {
            // input integer has its bytes in big endian byte order
            // swap them around
            return (int)(((uint)i >> 24) | (((uint)i >> 8) & 0xFF00) | ((i << 8) & 0xFF0000) | (i << 24));//>>>
        }

        private float getLittleEndianFloat(int i)
        {
            // input integer has its bytes in big endian byte order
            // swap them around and interpret data as floating point
            return ByteUtil.intBitsToFloat(getLittleEndianInt(i));
        }
    }
}