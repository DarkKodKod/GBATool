using System.Windows.Media.Imaging;

namespace GBATool.VOs;

public record ImageVO
{
    public WriteableBitmap? Image { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
}
