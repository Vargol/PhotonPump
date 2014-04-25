using System;

namespace SunflowSharp.Core
{

    /**
     * Represents a multi-pixel image filter kernel.
     */
    public interface IFilter
    {
        /**
         * Width in pixels of the filter extents. The filter will be applied to the
         * range of pixels within a box of <code>+/- getSize() / 2</code> around
         * the center of the pixel.
         * 
         * @return width in pixels
         */
        float getSize();

        /**
         * Get value of the filter at offset (x, y). The filter should never be
         * called with values beyond its extents but should return 0 in those cases
         * anyway.
         * 
         * @param x x offset in pixels
         * @param y y offset in pixels
         * @return value of the filter at the specified location
         */
        float get(float x, float y);
    }
}