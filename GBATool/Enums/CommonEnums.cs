using System.ComponentModel;

namespace GBATool.Enums
{
    public enum ProjectItemType
    {
        None = 0,
        Bank = 1,
        Character = 2,
        Map = 3,
        TileSet = 4,
        Palette = 5,
        World = 6,
        Entity = 7
    }

    public enum OutputMessageType
    {
        Information = 0,
        Warning,
        Error
    }

    public enum EditFrameTools
    {
        None = 0,
        Select = 1,
        Paint = 2,
        Erase = 3
    }

    public enum SpritePattern
    {
        [Description("1D")]
        Format1D = 0,
        [Description("2D")]
        Format2D = 1
    }
}
