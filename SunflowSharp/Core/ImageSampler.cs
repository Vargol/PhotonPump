using System;

namespace SunflowSharp.Core
{

    /**
     * This interface represents an image sampling algorithm capable of rendering
     * the entire image. Implementations are responsible for anti-aliasing and
     * filtering.
     */
    public interface ImageSampler
    {
        /**
         * Prepare the sampler for rendering an image of w x h pixels
         * 
         * @param w width of the image
         * @param h height of the image
         */
        bool prepare(Options options, Scene scene, int w, int h);

        /**
         * Render the image to the specified display. The sampler can assume the
         * display has been opened and that it will be closed after the method
         * returns.
         * 
         * @param display Display driver to send image data to
         */
        void render(IDisplay display);
    }
}