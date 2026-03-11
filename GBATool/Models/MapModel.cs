using GBATool.Enums;
using Nett;
using System.Linq;
using System.Windows;

namespace GBATool.Models;

public struct Tile
{
    public Tile()
    {
        FlipHorizontal = false;
        FlipVertical = false;
        PaletteIndex = 0;
        SpriteTileID = string.Empty;
        TileSetID = string.Empty;
    }

    public bool FlipHorizontal { get; set; }
    public bool FlipVertical { get; set; }
    public byte PaletteIndex { get; set; }
    public string SpriteTileID { get; set; }
    public string TileSetID { get; set; }

    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(SpriteTileID) || string.IsNullOrEmpty(TileSetID);
    }

    public void Clean()
    {
        SpriteTileID = string.Empty;
        TileSetID = string.Empty;
    }
}

public class MapModel : AFileModel
{
    private const string _extensionKey = "extensionMaps";

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

    [TomlIgnore]
    public const int RegularTileMin = 32 * 32;
    [TomlIgnore]
    public const int RegularTileMax = RegularTileMin * 4;
    [TomlIgnore]
    public const int AffineTileMax = RegularTileMax * 2;

    public MapType MapType { get; set; } = MapType.Regular;
    public Priority Priority { get; set; } = Priority.Highest;
    public BckgrRegularSize BckgrRegularSize { get; set; } = BckgrRegularSize.Regular32x32;
    public BckgrAffineSize BckgrAffineSize { get; set; } = BckgrAffineSize.Affine16x16;
    public Tile[] Tiles { get; set; } = new Tile[RegularTileMax];
    public bool EnableMosaic { get; set; }
    public bool AffineWrapping { get; set; }
    public string[] PaletteIDs { get; set; } = [.. Enumerable.Repeat(string.Empty, 16)];
    public ScreenBaseBlock ScreenBaseBlock { get; set; } = ScreenBaseBlock.Block0;
    public CharacterBaseBlock CharacterBaseBlock { get; set; } = CharacterBaseBlock.Block0;
    public string BankID { get; set; } = string.Empty;
}
