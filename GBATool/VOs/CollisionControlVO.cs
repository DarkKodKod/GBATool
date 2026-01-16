using GBATool.Enums;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GBATool.VOs;

public record CollisionControlVO : IFrameDraggable
{
    public string ID { get; init; } = string.Empty;
    public Rectangle? Rectangle { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int PositionX { get; init; }
    public int PositionY { get; init; }
    public CollisionMask Mask { get; init; }
    public int CustomMask { get; init; }
    public SolidColorBrush Color { get; init; } = new();
    public string AnimationID { get; init; } = string.Empty;
    public string FrameID { get; init; } = string.Empty;
}
