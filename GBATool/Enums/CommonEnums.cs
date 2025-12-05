using System.ComponentModel;

namespace GBATool.Enums;

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

public enum OutputFormat
{
    [Description("None")]
    None = 0,
    [Description("Binary")]
    Binary = 1,
    [Description("FASMARM")]
    Fasmarm = 2,
    [Description("Butano")]
    Butano = 3
}