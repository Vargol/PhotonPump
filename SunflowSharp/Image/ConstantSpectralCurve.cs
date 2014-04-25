using System;

namespace SunflowSharp.Image
{
    /**
     * Very simple class equivalent to a constant spectral curve. Note that this is
     * most likely physically impossible for amplitudes > 0, however this class can
     * be handy since in practice spectral curves end up being integrated against
     * the finite width color matching functions.
     */
    public class ConstantSpectralCurve : SpectralCurve
    {
        private float amp;

        public ConstantSpectralCurve(float amp)
        {
            this.amp = amp;
        }

        public override float sample(float lambda)
        {
            return amp;
        }
    }
}