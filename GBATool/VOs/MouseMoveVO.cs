using System.Windows;

namespace GBATool.VOs;

public record MouseMoveVO : EventVO
{
    public Point Position { get; init; }
}
