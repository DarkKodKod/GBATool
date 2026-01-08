using GBATool.Enums;
using Nett;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace GBATool.Models;

public class CharacterCollision
{
    public string ID { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
    public Color Color { get; set; }
    public int Mask { get; set; }
}

public class CharacterSprite
{
    public string ID { get; set; } = string.Empty;
    public Point Position { get; set; }
    public bool FlipHorizontal { get; set; }
    public bool FlipVertical { get; set; }
    public string SpriteID { get; set; } = string.Empty;
    public string TileSetID { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public bool EnableMosaic { get; set; }
    public GraphicMode GraphicMode { get; set; }
    public ObjectMode ObjectMode { get; set; }
}

public class FrameModel
{
    public string ID { get; set; } = string.Empty;
    public Dictionary<string, CharacterSprite> Tiles { get; set; } = [];
    public string BankID { get; set; } = string.Empty;
    public Dictionary<string, CharacterCollision> CollisionInfo { get; set; } = [];
    public bool IsHeldFrame { get; set; } = false;
}

public class CharacterAnimation
{
    public string ID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public float Speed { get; set; }
    public int Repeat { get; set; } = 1;
    public bool Looping { get; set; } = true;
    public Dictionary<string, FrameModel> Frames { get; set; } = [];
}

public class CharacterModel : AFileModel
{
    private const string _extensionKey = "extensionCharacters";

    [TomlIgnore]
    public override string FileExtension
    {
        get
        {
            if (string.IsNullOrEmpty(_fileExtension))
            {
                _fileExtension = (string)Application.Current.FindResource(_extensionKey);
            }

            return _fileExtension;
        }
    }

    public Point RelativeOrigin { get; set; }
    public int VerticalAxis { get; set; }
    public int SpriteBase { get; set; }
    public string PaletteID { get; set; } = string.Empty;
    public int PaletteIndex { get; set; }
    public Priority Priority { get; set; } = Priority.Highest;
    public Dictionary<string, CharacterAnimation> Animations { get; set; } = [];
}
