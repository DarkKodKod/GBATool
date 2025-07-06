using ArchitectureLibrary.Model;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Utils;
using Nett;
using System.Collections.Generic;
using System.Windows;

namespace GBATool.Models;

using TileBlocks = (int width, int height);

public record SpriteRef
{
    public string? SpriteID { get; set; }
    public string? TileSetID { get; set; }
}

public class BankModel : AFileModel
{
    private const string _extensionKey = "extensionBanks";

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

    public bool Use256Colors { get; set; }
    public bool IsBackground { get; set; }
    public int TransparentColor { get; set; } = 0;
    public string PaletteId { get; set; } = string.Empty;
    public List<SpriteRef> Sprites { get; set; } = [];

    [TomlIgnore]
    public bool IsFull { get; private set; }

    public int GetTileIndex(string spriteID)
    {
        int count = 0;
        foreach (SpriteRef sprite in Sprites)
        {
            if (sprite.SpriteID == spriteID)
            {
                return count;
            }

            count++;
        }

        return count;
    }

    public (bool, string) RegisterSprite(SpriteModel sprite)
    {
        SpriteRef? find = Sprites.Find((spriteRef) => spriteRef.SpriteID == sprite.ID);

        if (!string.IsNullOrEmpty(find?.SpriteID))
        {
            return (false, "This sprite is already in the bank");
        }

        Sprites.Add(new SpriteRef() { SpriteID = sprite.ID, TileSetID = sprite.TileSetID });

        return (true, "");
    }

    public bool RemoveSprite(SpriteModel sprite)
    {
        SpriteRef? find = Sprites.Find((spriteRef) => spriteRef.SpriteID == sprite.ID);

        if (string.IsNullOrEmpty(find?.SpriteID))
        {
            return false;
        }

        return Sprites.Remove(new SpriteRef() { SpriteID = sprite.ID, TileSetID = sprite.TileSetID });
    }

    public void ReplaceSpriteList(List<SpriteRef> list)
    {
        Sprites = list;
    }

    public void CleanUpDeletedSprites()
    {
        List<SpriteRef> idsToRemove = [];

        foreach (SpriteRef spriteRef in Sprites)
        {
            if (spriteRef == null ||
                string.IsNullOrEmpty(spriteRef.SpriteID) ||
                string.IsNullOrEmpty(spriteRef.TileSetID))
                continue;

            TileSetModel? tileSetModel = ProjectFiles.GetModel<TileSetModel>(spriteRef.TileSetID);

            if (tileSetModel == null)
            {
                idsToRemove.Add(spriteRef);
                continue;
            }

            SpriteModel? sprite = tileSetModel.Sprites.Find((item) => item.ID == spriteRef.SpriteID);

            if (string.IsNullOrEmpty(sprite?.ID))
            {
                idsToRemove.Add(spriteRef);
            }
        }

        foreach (SpriteRef spriteToRemove in idsToRemove)
        {
            Sprites.Remove(spriteToRemove);
        }
    }

    public TileBlocks GetBoundingBoxSize()
    {
        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        bool is1DImage = IsBackground || projectModel.SpritePatternFormat == SpritePattern.Format1D;

        int tilesWidth = 0;
        int tilesHeight = 0;
        int countTiles = 0;
        int acuWidth = 0;
        int acuHeight = 0;

        foreach (SpriteRef spriteRef in Sprites)
        {
            if (spriteRef == null ||
                string.IsNullOrEmpty(spriteRef.SpriteID) ||
                string.IsNullOrEmpty(spriteRef.TileSetID))
                continue;

            TileSetModel? tileSetModel = ProjectFiles.GetModel<TileSetModel>(spriteRef.TileSetID);

            if (tileSetModel == null)
                continue;

            SpriteModel? sprite = tileSetModel.Sprites.Find((item) => item.ID == spriteRef.SpriteID);

            if (string.IsNullOrEmpty(sprite?.ID))
                continue;

            if (is1DImage)
            {
                countTiles += SpriteUtils.Count8x8Tiles(sprite.Shape, sprite.Size);
            }
            else
            {
                int width = 0;
                int height = 0;
                SpriteUtils.ConvertToWidthHeight(sprite.Shape, sprite.Size, ref width, ref height);

                width /= BankUtils.SizeOfCellInPixels;
                height /= BankUtils.SizeOfCellInPixels;

                calculate:
                if (acuWidth + width <= BankUtils.MaxTextureCellsWidth)
                {
                    acuWidth += width;

                    if (acuHeight < height)
                    {
                        acuHeight = height;
                    }
                    if (acuWidth > tilesWidth)
                    {
                        tilesWidth = acuWidth;
                    }
                }
                else
                {
                    tilesHeight += acuHeight;
                    acuWidth = 0;
                    acuHeight = 0;

                    goto calculate;
                }
            }
        }

        if (acuHeight > 0)
        {
            tilesHeight += acuHeight;
        }

        if (countTiles > 0)
        {
            tilesWidth = countTiles < BankUtils.MaxTextureCellsWidth ? countTiles : BankUtils.MaxTextureCellsWidth;

            float div = countTiles / BankUtils.MaxTextureCellsWidth;
            tilesHeight = (int)float.Ceiling(div + 0.5f);
        }

        return (tilesWidth, tilesHeight);
    }
}
