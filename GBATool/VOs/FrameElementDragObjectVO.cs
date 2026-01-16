namespace GBATool.VOs;

public interface IFrameDraggable
{
    string ID { get; init; }
}

public record FrameElementDragObjectVO(IFrameDraggable DragableObject, double OffsetX, double OffsetY);
