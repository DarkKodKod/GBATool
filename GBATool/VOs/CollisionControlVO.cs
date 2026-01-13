using System.Windows.Shapes;

namespace GBATool.VOs;

public record CollisionControlVO
{
    public string ID { get; init; } = string.Empty;
    public Rectangle? Rectangle { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int PositionX { get; init; }
    public int PositionY { get; init; }
}
