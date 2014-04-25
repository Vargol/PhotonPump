using System;

namespace SunflowSharp.Core.Bucket
{
    public class InvertedBucketOrder : BucketOrder
    {
        private BucketOrder order;

        public InvertedBucketOrder(BucketOrder order)
        {
            this.order = order;
        }

        public int[] getBucketSequence(int nbw, int nbh)
        {
            int[] coords = order.getBucketSequence(nbw, nbh);
            for (int i = 0; i < coords.Length / 2; i += 2)
            {
                int src = i;
                int dst = coords.Length - 2 - i;
                int tmp = coords[src + 0];
                coords[src + 0] = coords[dst + 0];
                coords[dst + 0] = tmp;
                tmp = coords[src + 1];
                coords[src + 1] = coords[dst + 1];
                coords[dst + 1] = tmp;
            }
            return coords;
        }
    }
}