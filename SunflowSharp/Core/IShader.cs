using System;
using SunflowSharp.Image;

namespace SunflowSharp.Core
{

/**
 * A shader represents a particular light-surface interaction.
 */
    public interface IShader : IRenderObject
    {
        /**
         * Gets the radiance for a specified rendering state. When this method is
         * called, you can assume that a hit has been registered in the state and
         * that the hit surface information has been computed.
         * 
         * @param state current render state
         * @return color emitted or reflected by the shader
         */
        Color GetRadiance(ShadingState state);

        /**
         * Scatter a photon with the specied power. Incoming photon direction is
         * specified by the ray attached to the current render state. This method
         * can safely do nothing if photon scattering is not supported or relevant
         * for the shader type.
         * 
         * @param state current state
         * @param power power of the incoming photon.
         */
        void ScatterPhoton(ShadingState state, Color power);
    }
}