using System;
using SunflowSharp.Core.Accel;
using SunflowSharp.Systems;

namespace SunflowSharp.Core
{
    public class AccelerationStructureFactory
    {
        public static AccelerationStructure create(string name, int n, bool primitives)
        {

			if (name == null || name == "auto")
            {
                if (primitives)
                {
                    if (n > 20000000)
                        name = "uniformgrid";
                    else if (n > 2000000)
						name = "bih";
                    else if (n > 2)
						name = "kdtree";
                    else
						name = "null";
                }
                else
                {
                    if (n > 2)
						name = "bih";
					else
						name = "null";
				}
            }
			AccelerationStructure accel = PluginRegistry.accelPlugins.createObject(name);
			if (accel == null) 
			{
                UI.printWarning(UI.Module.ACCEL, "Unrecognized intersection accelerator \"{0}\" - using auto", name);
                return create(null, n, primitives);
            }
			UI.printInfo(UI.Module.ACCEL, "Building {0} acceleration structure...", name);
			return accel;
        }
    }
}