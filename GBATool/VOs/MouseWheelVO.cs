using System.Windows.Input;

namespace GBATool.VOs;

public record MouseWheelVO(MouseWheelEventArgs EventArgs) : EventVO
{
    public int Delta { get; init; }
}
