using GBATool.Enums;
using GBATool.Models;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBATool.Utils;

public static class PaletteUtils
{
    private static readonly string folderPalettesKey = "folderPalettes";

    public static int ConvertColorToInt(Color color)
    {
        return ((color.R & 0xff) << 16) | ((color.G & 0xff) << 8) | (color.B & 0xff);
    }
    public static Color GetColorFromInt(int colorInt)
    {
        byte R = (byte)(colorInt >> 16);
        byte G = (byte)(colorInt >> 8);
        byte B = (byte)colorInt;

        return Color.FromRgb(R, G, B);
    }

    public static string? GetPaletteName(FileModelVO fileModelVO)
    {
        if (fileModelVO.Model is not PaletteModel _)
        {
            return null;
        }

        string folderPalettes = (string)Application.Current.FindResource(folderPalettesKey);

        if (string.IsNullOrEmpty(fileModelVO.Name))
        {
            return null;
        }

        if (string.IsNullOrEmpty(fileModelVO.Path))
        {
            return null;
        }

        string[] array = fileModelVO.Path.Split(Path.DirectorySeparatorChar);
        int index = Array.IndexOf(array, folderPalettes);

        StringBuilder sb = new();
        for (int i = index + 1; i < array.Length; i++)
        {
            sb.Append(array[i]);
            sb.Append('_');
        }

        return $"palette_{sb}{fileModelVO.Name.Replace(' ', '_').ToLower()}";
    }

    public static List<Color> GeneratePaletteColorList(IEnumerable<SpriteModel> bankSprites, Color transparentColor, BitsPerPixel bitPerPixel)
    {
        int maxNumberOfColor = bitPerPixel.GetNumberOfColors();

        List<Color> colorArray = new([transparentColor]);

        foreach (SpriteModel spriteModel in bankSprites)
        {
            (_, WriteableBitmap? sourceBitmapCached) = TileSetUtils.GetSourceBitmapFromCache(spriteModel.TileSetID);

            if (sourceBitmapCached == null)
            {
                continue;
            }

            WriteableBitmap sourceBitmap = sourceBitmapCached.CloneCurrentValue();

            int width = 0;
            int height = 0;

            if (spriteModel.Shape == SpriteShape.Custom || spriteModel.Size == SpriteSize.Custom)
            {
                width = spriteModel.Width;
                height = spriteModel.Height;
            }
            else
            {
                SpriteUtils.ConvertToWidthHeight(spriteModel.Shape, spriteModel.Size, ref width, ref height);
            }

            WriteableBitmap cropped = sourceBitmap.Crop(spriteModel.PosX, spriteModel.PosY, width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = cropped.GetPixel(x, y);

                    if (!colorArray.Contains(color))
                    {
                        if (colorArray.Count < maxNumberOfColor)
                        {
                            colorArray.Add(color);
                        }
                    }
                }
            }
        }

        return colorArray;
    }
}
