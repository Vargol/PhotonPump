using System;

namespace SunflowSharp.Core
{

    /**
     * This class is a generic interface to caustic photon mapping capabilities.
     */
    public interface CausticPhotonMapInterface : PhotonStore
    {
        /**
         * Retrieve caustic photons at the specified shading location and add them
         * as diffuse light samples.
         * 
         * @param state
         */
        void getSamples(ShadingState state);
    }
}