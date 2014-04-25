using System;
using SunflowSharp.Core;
using SunflowSharp.Systems;

namespace SunflowSharp.Core.Bucket
{
    public class BucketOrderFactory
    {
        public static BucketOrder create(string order)
        {
            bool flip = false;
            if (order.StartsWith("inverse") || order.StartsWith("invert") || order.StartsWith("reverse"))
            {
                string[] tokens = order.Split(StringConsts.Whitespace, StringSplitOptions.RemoveEmptyEntries);//"\\s+");
                if (tokens.Length == 2)
                {
                    order = tokens[1];
                    flip = true;
                }
            }
			BucketOrder o = PluginRegistry.bucketOrderPlugins.createObject(order);
			if (o == null)
            {
				UI.printWarning(UI.Module.BCKT, "Unrecognized bucket ordering: \"{0}\" - using hilbert", order);
				return create("hilbert");
            }
			return flip ? new InvertedBucketOrder(o) : o;
        }
    }
}