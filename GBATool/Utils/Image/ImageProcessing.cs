using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBATool.Utils.Image;

public enum BitsPerPixel
{
    f4bpp = 4,
    f8bpp = 8
}

// Information taken from: https://sneslab.net/wiki/Graphics_Format
public static class ImageProcessing
{
    public static List<Color> GetNewPalette(BitsPerPixel bpp, int transparentColor)
    {
        List<Color> palette = [];

        int numberOfColors = (int)Math.Pow((int)bpp, 2);

        for (int i = 0; i < numberOfColors; i++)
        {
            palette.Add(Util.GetColorFromInt(transparentColor));
        }

        return palette;
    }

    public static byte[]? ConvertToXbpp(BitsPerPixel bpp, WriteableBitmap bitmap, int width, int height, ref List<Color> palette)
    {
        int bitsPerPixel = (int)bpp;
        const int bitPlaneSizeInBytes = 8; // 8x8 bits in a tile

        int bufferSize = (bitsPerPixel * bitPlaneSizeInBytes) * width * height;

        byte[] bytes = new byte[bufferSize];
        Array.Fill<byte>(bytes, 0);

        int pixelIndex = 0;
        int currentX = 0;
        int currentY = 0;

        int numberOfColors = (int)Math.Pow(bitsPerPixel, 2);

        Color transparentColor = palette.First();

        Dictionary<int, Dictionary<Color, int>> groupedPalettes = [];

        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                int group = 0;

                if (!groupedPalettes.TryGetValue(group, out Dictionary<Color, int>? colors))
                {
                    colors = new Dictionary<Color, int> { { transparentColor, 0 } };

                    groupedPalettes.Add(group, colors);
                }

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
                                if (colors.Count < numberOfColors)
                                {
                                    colorIndex = colors.Count;

                                    colors.Add(color, colorIndex);

                                    palette[colorIndex] = Color.FromRgb(color.R, color.G, color.B);
                                }
                                else
                                {
                                    // This color does not fit in the color count
                                    colorIndex = -1;
                                }
                            }

                            if (bpp == BitsPerPixel.f4bpp)
                                Set4BitsAccordingToIndex(pixelIndex, colorIndex, ref bytes);
                            else
                                Set8BitsAccordingToIndex(pixelIndex, colorIndex, ref bytes);
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
        int colIndex = 8 - (pixelIndex % 8);
        int byteIndex = (pixelIndex / 8) * 2;

        bool colorIndexBit1 = 0 != (colorIndex & (1 << 0));
        bool colorIndexBit2 = 0 != (colorIndex & (1 << 1));
        bool colorIndexBit3 = 0 != (colorIndex & (1 << 2));
        bool colorIndexBit4 = 0 != (colorIndex & (1 << 3));

        byte mask = (byte)(1 << (colIndex - 1));

        if (colorIndexBit1)
        {
            output[byteIndex + 1] = (byte)(output[byteIndex + 1] | mask);
        }
        if (colorIndexBit2)
        {
            output[byteIndex] = (byte)(output[byteIndex] | mask);
        }
        if (colorIndexBit3)
        {
            output[byteIndex + 16] = (byte)(output[byteIndex + 16] | mask);
        }
        if (colorIndexBit4)
        {
            output[byteIndex + 17] = (byte)(output[byteIndex + 17] | mask);
        }
    }

    private static void Set8BitsAccordingToIndex(int pixelIndex, int colorIndex, ref byte[] output)
    {
    }
}
