using System;
using System.Collections.Generic;
using SunflowSharp;

namespace SunflowSharp.Core
{

    /**
     * This holds rendering objects as key, value pairs.
     */
    public class Options : ParameterList, IRenderObject
    {
        public bool Update(ParameterList pl, SunflowAPI api)
        {
            // take all attributes, and update them into the current set
            foreach (KeyValuePair<string, Parameter> e in pl.list)
            {
                list[e.Key] = e.Value;
                e.Value.check();
            }
            return true;
        }
    }
}