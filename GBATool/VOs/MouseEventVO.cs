using System.Windows;
using System.Windows.Input;

namespace GBATool.VOs;

public record MouseEventVO(MouseEventArgs EventArgs) : EventVO
{
    public Point AbsolutePosition { get; init; }
    public Point RelativePosition { get; init; }
    public MouseButtonState LeftButton { get; init; }
}
