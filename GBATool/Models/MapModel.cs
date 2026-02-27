using GBATool.Enums;
using Nett;
using System.Collections.Generic;
using System.Windows;

namespace GBATool.Models;

public class Tile
{
    public string ID { get; set; } = string.Empty;
    public bool FlipHorizontal { get; set; }
    public bool FlipVertical { get; set; }
    public int PaletteIndex { get; set; }
    public string SpriteTileID { get; set; } = string.Empty;
    public string TileSetID { get; set; } = string.Empty;
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
    public List<Tile> Tiles { get; set; } = [];
    public bool EnableMosaic { get; set; }
    public bool AffineWrapping { get; set; }
    public List<string> PaletteIDs { get; set; } = [];
}
