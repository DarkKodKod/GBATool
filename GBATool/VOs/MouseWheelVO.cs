namespace GBATool.VOs;

public record MouseWheelVO : EventVO
{
    public int Delta { get; init; }
}
