using System.Windows.Media;

namespace GBATool.Utils;

public static class PaletteUtils
{
    public static int ConvertColorToInt(Color color)
    {
        return ((color.R & 0xff) << 16) | ((color.G & 0xff) << 8) | (color.B & 0xff);
    }
}
