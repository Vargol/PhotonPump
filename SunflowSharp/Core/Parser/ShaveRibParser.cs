using System;
using System.IO;
using System.Collections.Generic;
using SunflowSharp.Core;
using SunflowSharp.Core.Primitive;
using SunflowSharp.Systems;

namespace SunflowSharp.Core.Parser
{

    public class ShaveRibParser : SceneParserBase
    {
        public override bool parse(Stream stream, SunflowAPI api)
        {
            try
            {
                SunflowSharp.Systems.Parser p = new SunflowSharp.Systems.Parser(stream);
                p.checkNextToken("version");
                p.checkNextToken("3.04");
                p.checkNextToken("TransformBegin");

                if (p.peekNextToken("Procedural"))
                {
                    // read procedural shave rib
                    bool done1 = false;
                    while (!done1)
                    {
                        p.checkNextToken("DelayedReadArchive");
                        p.checkNextToken("[");
                        string f = p.getNextToken();
                        UI.printInfo(UI.Module.USER, "RIB - Reading voxel: \"{0}\" ...", f);
                        api.include(f);
                        p.checkNextToken("]");
                        while (true)
                        {
                            string t = p.getNextToken();
                            if (t == null || t == "TransformEnd")
                            {
                                done1 = true;
                                break;
                            }
                            else if (t == "Procedural")
                                break;
                        }
                    }
                    return true;
                }

                bool cubic = false;
                if (p.peekNextToken("Basis"))
                {
                    cubic = true;
                    // u basis
                    p.checkNextToken("catmull-rom");
                    p.checkNextToken("1");
                    // v basis
                    p.checkNextToken("catmull-rom");
                    p.checkNextToken("1");
                }
                while (p.peekNextToken("Declare"))
                {
                    p.getNextToken(); // name
                    p.getNextToken(); // interpolation & type
                }
                int index = 0;
                bool done = false;
                p.checkNextToken("Curves");
                do
                {
                    if (cubic)
                        p.checkNextToken("cubic");
                    else
                        p.checkNextToken("linear");
                    int[] nverts = parseIntArray(p);
                    for (int i = 1; i < nverts.Length; i++)
                    {
                        if (nverts[0] != nverts[i])
                        {
                            UI.printError(UI.Module.USER, "RIB - Found variable number of hair segments");
                            return false;
                        }
                    }
                    int nhairs = nverts.Length;

                    UI.printInfo(UI.Module.USER, "RIB - Parsed {0} hair curves", nhairs);

                    api.parameter("segments", nverts[0] - 1);

                    p.checkNextToken("nonperiodic");
                    p.checkNextToken("P");
                    float[] points = parseFloatArray(p);
                    if (points.Length != 3 * nhairs * nverts[0])
                    {
                        UI.printError(UI.Module.USER, "RIB - Invalid number of points - expecting {0} - found {0}", nhairs * nverts[0], points.Length / 3);
                        return false;
                    }
                    api.parameter("points", "point", "vertex", points);

                    UI.printInfo(UI.Module.USER, "RIB - Parsed {0} hair vertices", points.Length / 3);

                    p.checkNextToken("width");
                    float[] w = parseFloatArray(p);
                    if (w.Length != nhairs * nverts[0])
                    {
                        UI.printError(UI.Module.USER, "RIB - Invalid number of hair widths - expecting {0} - found {0}", nhairs * nverts[0], w.Length);
                        return false;
                    }
                    api.parameter("widths", "float", "vertex", w);

                    UI.printInfo(UI.Module.USER, "RIB - Parsed {0} hair widths", w.Length);

                    string name = string.Format("{0}[{1}]", "[Stream]", index);
                    UI.printInfo(UI.Module.USER, "RIB - Creating hair object \"{0}\"", name);
                    api.geometry(name, "hair");
                    api.instance(name + ".instance", name);

                    UI.printInfo(UI.Module.USER, "RIB - Searching for next curve group ...");
                    while (true)
                    {
                        string t = p.getNextToken();
                        if (t == null || t == "TransformEnd")
                        {
                            done = true;
                            break;
                        }
                        else if (t == "Curves")
                            break;
                    }
                    index++;
                } while (!done);
                UI.printInfo(UI.Module.USER, "RIB - Finished reading rib file");
            }
            catch (Exception e)
            {
                UI.printError(UI.Module.USER, "RIB - File not found: {0}", "[Stream]");
                return false;
            }
            return true;
        }

        private int[] parseIntArray(SunflowSharp.Systems.Parser p)
        {
            List<int> array = new List<int>();
            bool done = false;
            do
            {
                string s = p.getNextToken();
                if (s.StartsWith("["))
                    s = s.Substring(1);
                if (s.EndsWith("]"))
                {
                    s = s.Substring(0, s.Length - 1);
                    done = true;
                }
                array.Add(int.Parse(s));
            } while (!done);
            return array.ToArray();
        }

        private float[] parseFloatArray(SunflowSharp.Systems.Parser p)
        {
            List<float> array = new List<float>();
            bool done = false;
            do
            {
                string s = p.getNextToken();
                if (s.StartsWith("["))
                    s = s.Substring(1);
                if (s.EndsWith("]"))
                {
                    s = s.Substring(0, s.Length - 1);
                    done = true;
                }
                array.Add(float.Parse(s, System.Globalization.CultureInfo.InvariantCulture));
            } while (!done);
            return array.ToArray();
        }
    }
}