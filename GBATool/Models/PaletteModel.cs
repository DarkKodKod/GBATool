using GBATool.Enums;
using GBATool.Utils;
using Nett;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace GBATool.Models;

public class PaletteModel : AFileModel
{
    private const string _extensionKey = "extensionPalettes";

    public static readonly int MaxColor = BitsPerPixel.f4bpp.GetNumberOfColors();

    [TomlIgnore]
    public override string FileExtension
    {
        get
        {
            if (string.IsNullOrEmpty(_fileExtension))
            {
                _fileExtension = (string)Application.Current.FindResource(_extensionKey);
            }

            return _fileExtension;
        }
    }

    public int[] Colors { get; set; } = [.. Enumerable.Repeat(0, MaxColor)];
    public List<string> LinkedPalettes { get; set; } = [];

    public void GetColors(ref List<Color> colors)
    {
        for (int i = 0; i < Colors.Length; i++)
        {
            colors.Add(PaletteUtils.GetColorFromInt(Colors[i]));
        }
    }
}
