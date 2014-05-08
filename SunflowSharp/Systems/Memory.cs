using System;

namespace SunflowSharp.Systems
{
    public class Memory
    {
        public static string SizeOf(int[] array)
        {
            return bytesTostring(array == null ? 0 : 4 * array.Length);
        }

        public static string bytesTostring(long bytes)
        {
            if (bytes < 1024)
				return string.Format("{0}b", bytes);
            if (bytes < 1024 * 1024)
				return string.Format("{0}Kb", (ulong)(bytes + 512) >> 10);//>>>
			return string.Format("{0}Mb", (ulong)(bytes + 512 * 1024) >> 20);//>>>
        }
    }
}