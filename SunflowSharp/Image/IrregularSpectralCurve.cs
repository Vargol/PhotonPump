using System;

namespace SunflowSharp.Image
{
    /**
     * This class allows spectral curves to be defined from irregularly sampled
     * data. Note that the waveLength array is assumed to be sorted low to high. Any
     * values beyond the defined range will simply be extended to infinity from the
     * end points. Points inside the valid range will be linearly interpolated
     * between the two nearest samples. No explicit error checking is performed, but
     * this class will run into {@link ArrayIndexOutOfBoundsException}s if the
     * array Lengths don't match.
     */
    public class IrregularSpectralCurve : SpectralCurve
    {
        private float[] waveLengths;
        private float[] amplitudes;

        /**
         * Define an irregular spectral curve from the provided (sorted) waveLengths
         * and amplitude data. The waveLength array is assumed to contain values in
         * nanometers. Array Lengths must match.
         * 
         * @param waveLengths sampled waveLengths in nm
         * @param amplitudes amplitude of the curve at the sampled points
         */
        public IrregularSpectralCurve(float[] waveLengths, float[] amplitudes)
        {
            this.waveLengths = waveLengths;
            this.amplitudes = amplitudes;
            if (waveLengths.Length != amplitudes.Length)
                throw new Exception(string.Format("Error creating irregular spectral curve: {0} waveLengths and {1} amplitudes", waveLengths.Length, amplitudes.Length));
            for (int i = 1; i < waveLengths.Length; i++)
                if (waveLengths[i - 1] >= waveLengths[i])
                    throw new Exception(string.Format("Error creating irregular spectral curve: values are not sorted - error at index {0}", i));
        }

        public override float sample(float lambda)
        {
            if (waveLengths.Length == 0)
                return 0; // no data
            if (waveLengths.Length == 1 || lambda <= waveLengths[0])
                return amplitudes[0];
            if (lambda >= waveLengths[waveLengths.Length - 1])
                return amplitudes[waveLengths.Length - 1];
            for (int i = 1; i < waveLengths.Length; i++)
            {
                if (lambda < waveLengths[i])
                {
                    float dx = (lambda - waveLengths[i - 1]) / (waveLengths[i] - waveLengths[i - 1]);
                    return (1 - dx) * amplitudes[i - 1] + dx * amplitudes[i];
                }
            }
            return amplitudes[waveLengths.Length - 1];
        }
    }
}