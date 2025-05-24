using Nett;
using System.Collections.Generic;
using System.Windows;

namespace GBATool.Models;

public struct CollisionInfo
{
    public CollisionInfo()
    {
        Width = 0;
        Height = 0;
        OffsetX = 0;
        OffsetY = 0;
    }

    public int Width { get; set; }
    public int Height { get; set; }
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
}

public class CharacterTile
{
    public string ID { get; set; } = string.Empty;
    public Point Point { get; set; }
    public bool FlipX { get; set; }
    public bool FlipY { get; set; }
}

public class FrameModel
{
    public string ID { get; set; } = string.Empty;
    public Dictionary<string, CharacterTile> Tiles { get; set; } = [];
    public string BankID { get; set; } = string.Empty;
}

public class CharacterAnimation
{
    public string ID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public float Speed { get; set; }
    public Dictionary<string, FrameModel> Frames { get; set; } = [];
    public CollisionInfo CollisionInfo { get; set; }
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

    public string PaletteID { get; set; } = string.Empty;
    public int PaletteIndex { get; set; } = 0;
    public Dictionary<string, CharacterAnimation> Animations { get; set; } = [];
}
