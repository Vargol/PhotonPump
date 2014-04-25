using System;

namespace SunflowSharp.Core.Bucket
{
    public class HilbertBucketOrder : BucketOrder
    {
        public int[] getBucketSequence(int nbw, int nbh)
        {
            uint hi = 0; // hilbert curve index
            int hn = 0; // hilbert curve order
            while (((1 << hn) < nbw || (1 << hn) < nbh) && hn < 16)
                hn++; // fit to number of buckets
            int hN = 1 << (2 * hn); // number of hilbert buckets - 2**2n
            int n = nbw * nbh; // total number of buckets
            int[] coords = new int[2 * n]; // storage for bucket coordinates
            for (int i = 0; i < n; i++)
            {
                uint hx, hy;//original int
                do
                {
                    // s is the hilbert index, shifted to start in the middle
                    uint s = hi; // (hi + (hN >> 1)) & (hN - 1);//original int
                    // int n = hn;
                    // adapted from Hacker's Delight
                    uint comp, swap, cs, t, sr;//original int
                    s = (uint)(s | (0x55555555 << (2 * hn))); // Pad s on left with 01
                    sr = ((uint)s >> 1) & 0x55555555; // (no change) groups.//>>>
                    cs = (uint)((s & 0x55555555) + sr) ^ 0x55555555;// Compute
                    // complement
                    // & swap info in
                    // two-bit groups.
                    // Parallel prefix xor op to propagate both complement
                    // and swap info together from left to right (there is
                    // no step "cs ^= cs >> 1", so in effect it computes
                    // two independent parallel prefix operations on two
                    // interleaved sets of sixteen bits).
                    cs = cs ^ ((uint)cs >> 2);//>>>
                    cs = cs ^ ((uint)cs >> 4);//>>>
                    cs = cs ^ ((uint)cs >> 8);//>>>
                    cs = cs ^ ((uint)cs >> 16);//>>>
                    swap = cs & 0x55555555; // Separate the swap and
                    comp = ((uint)cs >> 1) & 0x55555555; // complement bits.//>>>
                    t = (uint)(s & swap) ^ comp; // Calculate x and y in
                    s = s ^ sr ^ t ^ (t << 1); // the odd & even bit
                    // positions, resp.
                    s = (uint)(s & ((1 << 2 * hn) - 1)); // Clear out any junk
                    // on the left (unpad).
                    // Now "unshuffle" to separate the x and y bits.
                    t = (s ^ ((uint)s >> 1)) & 0x22222222;//>>>
                    s = s ^ t ^ (t << 1);
                    t = (s ^ ((uint)s >> 2)) & 0x0C0C0C0C;//>>>
                    s = s ^ t ^ (t << 2);
                    t = (s ^ ((uint)s >> 4)) & 0x00F000F0;//>>>
                    s = s ^ t ^ (t << 4);
                    t = (s ^ ((uint)s >> 8)) & 0x0000FF00;//>>>
                    s = s ^ t ^ (t << 8);
                    hx = (uint)s >> 16; // Assign the two halves//>>>
                    hy = s & 0xFFFF; // of t to x and y.
                    hi++;
                } while ((hx >= nbw || hy >= nbh || hx < 0 || hy < 0) && hi < hN);
                coords[2 * i + 0] = (int)hx;
                coords[2 * i + 1] = (int)hy;
            }
            return coords;
        }
    }
}