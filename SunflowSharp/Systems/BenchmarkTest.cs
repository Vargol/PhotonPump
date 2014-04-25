using System;

namespace SunflowSharp.Systems
{
/**
 * This interface is used to represent a piece of code which is to be
 * benchmarked by repeatedly running and timing the kernel code. The begin/end
 * routines are called per-iteration to do any local initialization which is not
 * meant to be taken into acount in the timing (like preparing or destroying
 * data structures).
 */
    public interface BenchmarkTest
    {

        void kernelBegin();

        void kernelMain();

        void kernelEnd();
    }
}