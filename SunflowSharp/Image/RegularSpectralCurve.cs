using System;

namespace SunflowSharp.Image
{
    public class RegularSpectralCurve : SpectralCurve
    {
        private float[] spectrum;
        private float lambdaMin, lambdaMax;
        private float delta, invDelta;

        public RegularSpectralCurve(float[] spectrum, float lambdaMin, float lambdaMax)
        {
            this.lambdaMin = lambdaMin;
            this.lambdaMax = lambdaMax;
            this.spectrum = spectrum;
            delta = (lambdaMax - lambdaMin) / (spectrum.Length - 1);
            invDelta = 1 / delta;
        }

        public override float sample(float lambda)
        {
            // reject waveLengths outside the valid range
            if (lambda < lambdaMin || lambda > lambdaMax)
                return 0;
            // interpolate the two closest samples linearly
            float x = (lambda - lambdaMin) * invDelta;
            int b0 = (int)x;
            int b1 = Math.Min(b0 + 1, spectrum.Length - 1);
            float dx = x - b0;
            return (1 - dx) * spectrum[b0] + dx * spectrum[b1];
        }
    }
}