using System.Windows.Controls;

namespace GBATool.VOs;

public record SpriteControlVO
{
    public string ID { get; init; } = string.Empty;
    public Image? Image { get; init; }
    public string SpriteID { get; init; } = string.Empty;
    public string TileSetID { get; init; } = string.Empty;
    public string BankID { get; init; } = string.Empty;
    public int Width { get; init; }
    public int Height { get; init; }
    public int OffsetX { get; init; }
    public int OffsetY { get; init; }
    public int PositionX { get; init; }
    public int PositionY { get; init; }
    public bool FlipHorizontal { get; set; }
    public bool FlipVertical { get; set; }
}

