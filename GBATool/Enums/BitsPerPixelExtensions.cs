using System;

namespace GBATool.Enums;

public static class BitsPerPixelExtensions
{
    public static int GetNumberOfColors(this BitsPerPixel bpp)
    {
        int bitsPerPixel = (int)bpp;
        return (int)Math.Pow(bitsPerPixel, 2);
    }
}
