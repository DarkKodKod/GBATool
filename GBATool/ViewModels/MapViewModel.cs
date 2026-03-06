using GBATool.FileSystem;
using GBATool.Models;
using GBATool.VOs;
using System;
using System.Collections.Generic;

namespace GBATool.ViewModels;

public class MapViewModel : ItemViewModel
{
    public MapModel? GetModel()
    {
        return ProjectItem?.FileHandler?.FileModel is MapModel model ? model : null;
    }

    #region Commands
    #endregion

    #region get/set
    #endregion

    public override void OnActivate()
    {
        base.OnActivate();

        MapModel? model = GetModel();

        if (model == null)
            return;

        LoadMapData();
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
    }

    private void LoadMapData()
    {
        MapModel? map = GetModel();

        if (map == null)
        {
            return;
        }

        if (map.Tiles.Count > 0)
        {
            return;
        }

        List<FileModelVO> bankModels = ProjectFiles.GetModels<BankModel>();

        FileModelVO? fileModel = bankModels.Find(t =>
        {
            if (t.Model != null)
            {
                if (t.Model is BankModel bm)
                {
                    return bm.Sprites.Count > 1;
                }
            }

            return false;
        });

        if (fileModel?.Model is not BankModel bank)
        {
            return;
        }

        map.BankID = bank.GUID;

        for (int i = 0; i < 60; i++)
        {
            map.Tiles.Add(new()
            {
                ID = Guid.NewGuid().ToString(),
                FlipHorizontal = false,
                FlipVertical = false,
                PaletteIndex = 0,
                SpriteTileID = bank.Sprites[4].SpriteID ?? string.Empty,
                TileSetID = bank.Sprites[4].TileSetID ?? string.Empty
            });
            map.Tiles.Add(new()
            {
                ID = Guid.NewGuid().ToString(),
                FlipHorizontal = false,
                FlipVertical = false,
                PaletteIndex = 0,
                SpriteTileID = bank.Sprites[6].SpriteID ?? string.Empty,
                TileSetID = bank.Sprites[6].TileSetID ?? string.Empty
            });
        }

        ProjectItem?.FileHandler?.Save();
    }
}
