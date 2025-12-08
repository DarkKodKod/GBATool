using System.Windows.Media;

namespace GBATool.VOs;

public record SpriteCollisionVO()
{
    public string ID { get; init; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
    public SolidColorBrush Color { get; set; } = new();
    public int Mask { get; set; }
}