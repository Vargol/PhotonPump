using System;

namespace SunflowSharp.Image
{
    public class BlackbodySpectrum : SpectralCurve
    {
        private float temp;

        public BlackbodySpectrum(float temp)
        {
            this.temp = temp;
        }

        public override float sample(float lambda)
        {
            double waveLength = lambda * 1e-9;
            return (float)((3.74183e-16 * Math.Pow(waveLength, -5.0)) / (Math.Exp(1.4388e-2 / (waveLength * temp)) - 1.0));
        }
    }
}