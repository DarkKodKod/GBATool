using GBATool.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBATool.Utils.Image;

// Information taken from: https://sneslab.net/wiki/Graphics_Format
public static class ImageProcessing
{
    public static List<Color> GetNewPalette(BitsPerPixel bpp, int transparentColor)
    {
        List<Color> palette = [];

        int numberOfColors = bpp.GetNumberOfColors();

        for (int i = 0; i < numberOfColors; i++)
        {
            palette.Add(Util.GetColorFromInt(transparentColor));
        }

        return palette;
    }

    public static byte[]? ConvertToXbpp(BitsPerPixel bpp, WriteableBitmap bitmap, int width, int height, ref List<Color> palette, List<string> warnings)
    {
        int bitsPerPixel = (int)bpp;
        const int bitPlaneSizeInBytes = 8; // 8x8 bits in a tile

        int bufferSize = (bitsPerPixel * bitPlaneSizeInBytes) * width * height;

        byte[] bytes = new byte[bufferSize];
        Array.Fill<byte>(bytes, 0);

        int pixelIndex = 0;
        int currentX = 0;
        int currentY = 0;

        int numberOfColors = bpp.GetNumberOfColors();

        Color transparentColor = palette.First();
        Dictionary<Color, int> colors = new() { { transparentColor, 0 } };

        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                // read pixels in the 8x8 quadrant
                for (int y = currentY; y < currentY + 8; ++y)
                {
                    for (int x = currentX; x < currentX + 8; ++x)
                    {
                        Color color = bitmap.GetPixel(x, y);
                        color.A = 255;

                        if (!transparentColor.Equals(color))
                        {
                            if (!colors.TryGetValue(color, out int colorIndex))
                            {
                                colorIndex = colors.Count;

                                colors.Add(color, colorIndex);

                                if (colorIndex >= numberOfColors)
                                {
                                    warnings.Add("Color exceed the color count");
                                }
                            }

                            if (colorIndex < numberOfColors)
                            {
                                palette[colorIndex] = Color.FromRgb(color.R, color.G, color.B);

                                if (bpp == BitsPerPixel.f4bpp)
                                    Set4BitsAccordingToIndex(pixelIndex, colorIndex, ref bytes);
                                else
                                    Set8BitsAccordingToIndex(pixelIndex, colorIndex, ref bytes);
                            }
                        }

                        pixelIndex++;
                    }
                }

                currentX += 8;
            }

            currentX = 0;
            currentY += 8;
        }

        return bytes;
    }

    private static void Set4BitsAccordingToIndex(int pixelIndex, int colorIndex, ref byte[] output)
    {
        int outputIndex = (pixelIndex * 4) / 8;

        byte b = (byte)colorIndex;

        Math.DivRem(pixelIndex, 2, out int reminder);

        byte store = (byte)(reminder == 0 ? b : b << 4);

        output[outputIndex] |= store;
    }

    private static void Set8BitsAccordingToIndex(int pixelIndex, int colorIndex, ref byte[] output)
    {
    }
}
