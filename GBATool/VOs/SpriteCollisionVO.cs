using System.Windows.Media;

namespace GBATool.VOs;

public record SpriteCollisionVO(string ID, int Width, int Height, int OffsetX, int OffsetY, SolidColorBrush Color, int Mask);