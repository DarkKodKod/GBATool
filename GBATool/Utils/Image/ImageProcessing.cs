using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBATool.Utils.Image;

public enum BitsPerPixel
{
    f1bpp = 1,
    f2bpp = 2,
    f3bpp = 3,
    f4bpp = 4,
    f8bpp = 8
}

// Information taken from: https://sneslab.net/wiki/Graphics_Format
public static class ImageProcessing
{
    public const int TileRowSizeInPixels = 8;
    public const int BitPlaneSize = TileRowSizeInPixels * TileRowSizeInPixels;

    public static List<Color> GetNewPalette(int numberOfColors, int transparentColor)
    {
        List<Color> palette = [];

        for (int i = 0; i < numberOfColors; i++)
        {
            palette.Add(Util.GetColorFromInt(transparentColor));
        }

        return palette;
    }

    public static byte[]? ConvertToXbpp(BitsPerPixel bpp, WriteableBitmap bitmap, int width, int height, ref List<Color> palette)
    {
        if (bpp != BitsPerPixel.f4bpp && bpp != BitsPerPixel.f8bpp)
        {
            return null;
        }

        int bitsPerPixel = (int)bpp;

        BitArray bits = new(BitPlaneSize * bitsPerPixel * width * height, false);

        int currentIndex = 0;
        int currentX = 0;
        int currentY = 0;

        int numberOfColors = (int)Math.Pow(bitsPerPixel, 2);

        Color transparentColor = palette.First();

        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                Dictionary<Color, int> colors = new() { { transparentColor, 0 } };

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

                            MarkBitsAccordingToIndex(colorIndex, bitsPerPixel, ref bits, currentIndex);
                        }

                        currentIndex++;
                    }
                }

                currentX += 8;
                currentIndex += (BitPlaneSize * (bitsPerPixel - 1));
            }

            currentX = 0;
            currentY += 8;
        }

        for (int i = 0; i < BitPlaneSize * bitsPerPixel * width * height;)
        {
            Reverse(ref bits, i, 8);
            i += 8;
        }

        byte[] bytes = new byte[bits.Length / 8];

        bits.CopyTo(bytes, 0);
        return bytes;
    }

    private static void Reverse(ref BitArray array, int start, int length)
    {
        int mid = length / 2;

        for (int i = start; i < start + mid; i++)
        {
            (array[start + start + length - i - 1], array[i]) = (array[i], array[start + start + length - i - 1]);
        }
    }

    private static void MarkBitsAccordingToIndex(int colorIndex, int bitsPerPixel, ref BitArray bits, int currentIndex)
    {
        switch (colorIndex)
        {
            case 1:
                bits[currentIndex + BitPlaneSize] = true;
                break;
            case 2:
                bits[currentIndex] = true;
                break;
            case 3:
                bits[currentIndex] = true;
                bits[currentIndex + BitPlaneSize] = true;
                break;
            case 4:
                bits[currentIndex + BitPlaneSize + BitPlaneSize] = true;
                break;
            case 5:
                bits[currentIndex + BitPlaneSize] = true;
                bits[currentIndex + BitPlaneSize + BitPlaneSize] = true;
                break;
            case 6:
                bits[currentIndex] = true;
                bits[currentIndex + BitPlaneSize + BitPlaneSize] = true;
                break;
            case 7:
                bits[currentIndex] = true;
                bits[currentIndex + BitPlaneSize] = true;
                bits[currentIndex + BitPlaneSize + BitPlaneSize] = true;
                break;
            case 8:
                bits[currentIndex + BitPlaneSize + BitPlaneSize + BitPlaneSize] = true;
                break;
            case 9:
                bits[currentIndex + BitPlaneSize] = true;
                bits[currentIndex + BitPlaneSize + BitPlaneSize + BitPlaneSize] = true;
                break;
            case 10:
                bits[currentIndex] = true;
                bits[currentIndex + BitPlaneSize + BitPlaneSize + BitPlaneSize] = true;
                break;
            case 11:
                bits[currentIndex] = true;
                bits[currentIndex + BitPlaneSize] = true;
                bits[currentIndex + BitPlaneSize + BitPlaneSize + BitPlaneSize] = true;
                break;
            case 12:
                bits[currentIndex + BitPlaneSize + BitPlaneSize] = true;
                bits[currentIndex + BitPlaneSize + BitPlaneSize + BitPlaneSize] = true;
                break;
            case 13:
                bits[currentIndex + BitPlaneSize] = true;
                bits[currentIndex + BitPlaneSize + BitPlaneSize] = true;
                bits[currentIndex + BitPlaneSize + BitPlaneSize + BitPlaneSize] = true;
                break;
            case 14:
                bits[currentIndex] = true;
                bits[currentIndex + BitPlaneSize + BitPlaneSize] = true;
                bits[currentIndex + BitPlaneSize + BitPlaneSize + BitPlaneSize] = true;
                break;
            case 15:
                bits[currentIndex] = true;
                bits[currentIndex + BitPlaneSize] = true;
                bits[currentIndex + BitPlaneSize + BitPlaneSize] = true;
                bits[currentIndex + BitPlaneSize + BitPlaneSize + BitPlaneSize] = true;
                break;
        }
    }
}
