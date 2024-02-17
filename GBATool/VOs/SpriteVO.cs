using System.Windows.Media.Imaging;

namespace GBATool.VOs;

public record SpriteVO
{
    public string? SpriteID { get; init; }
    public BitmapSource? Bitmap { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
}
