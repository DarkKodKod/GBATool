using System.Windows;
using System.Windows.Input;

namespace GBATool.VOs;

public record MouseMoveVO : EventVO
{
    public Point AbsolutePosition { get; init; }
    public Point RelativePosition { get; init; }
    public MouseButtonState LeftButton { get; init; }
}
