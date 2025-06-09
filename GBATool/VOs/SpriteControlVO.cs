using System.Windows.Controls;

namespace GBATool.VOs;

public record SpriteControlVO
{
    public Image? Image { get; init; }
    public string? SpriteID { get; init; }
    public string? TileSetID { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int OffsetX { get; init; }
    public int OffsetY { get; init; }
}

