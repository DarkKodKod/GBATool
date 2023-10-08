using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace GBATool.Utils
{
    public static class BankUtils
    {
        private static readonly int MaxTextureCellsWidth = 32;
        private static readonly int MaxTextureCellsHeight = 32;
        private static readonly int SizeOfCellInPixels = 8;

        public static WriteableBitmap CreateImage(BankModel bankModel, ref Dictionary<string, WriteableBitmap> bitmapCache, bool sendSignals = true)
        {
            WriteableBitmap bankBitmap = BitmapFactory.New(MaxTextureCellsWidth * SizeOfCellInPixels, MaxTextureCellsHeight * SizeOfCellInPixels);

            ProjectModel projectModel = ModelManager.Get<ProjectModel>();

            int index = 0;

            foreach (SpriteModel sprite in bankModel.Sprites)
            {
                if (string.IsNullOrEmpty(sprite.ID) || string.IsNullOrEmpty(sprite.TileSetID))
                {
                    continue;
                }

                WriteableBitmap? sourceBitmap = GetSourceBitmapFromCache(ref bitmapCache, sprite.TileSetID, sendSignals);

                if (sourceBitmap == null)
                {
                    continue;
                }

                int width = 0;
                int height = 0;

                SpriteUtils.ConvertToWidthHeight(sprite.Shape, sprite.Size, ref width, ref height);

                int posX = sprite.PosX;
                int posY = sprite.PosY;

                if (projectModel.SpritePatternFormat == SpritePattern.Format1D)
                {
                    for (int j = 0; j < (height / SizeOfCellInPixels); ++j)
                    {
                        for (int i = 0; i < (width / SizeOfCellInPixels); ++i)
                        {
                            WriteableBitmap cropped = sourceBitmap.Crop(posX, posY, SizeOfCellInPixels, SizeOfCellInPixels);

                            int destX = index % MaxTextureCellsWidth * SizeOfCellInPixels;
                            int destY = index / MaxTextureCellsHeight * SizeOfCellInPixels;

                            Util.CopyBitmapImageToWriteableBitmap(ref bankBitmap, destX, destY, cropped);

                            posX += SizeOfCellInPixels;

                            index++;
                        }

                        posX = sprite.PosX;
                        posY += SizeOfCellInPixels;
                    }
                }
                else
                {
                    //
                }
            }

            return bankBitmap;
        }

        private static WriteableBitmap? GetSourceBitmapFromCache(ref Dictionary<string, WriteableBitmap> bitmapCache, string tileSetId, bool sendSignals)
        {
            if (!bitmapCache.TryGetValue(tileSetId, out WriteableBitmap? sourceBitmap))
            {
                TileSetModel? model = ProjectFiles.GetModel<TileSetModel>(tileSetId);

                if (model == null)
                {
                    return null;
                }

                ProjectModel projectModel = ModelManager.Get<ProjectModel>();

                string path = Path.Combine(projectModel.ProjectPath, model.ImagePath);

                BitmapImage bmImage = new();
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                bmImage.EndInit();
                bmImage.Freeze();
                sourceBitmap = BitmapFactory.ConvertToPbgra32Format(bmImage);

                bitmapCache.Add(tileSetId, sourceBitmap);

                if (sendSignals)
                {
                    FileModelVO[] tileSets = ProjectFiles.GetModels<TileSetModel>().ToArray();

                    // Add the link object
                    foreach (FileModelVO tileset in tileSets)
                    {
                        if (tileset.Model?.GUID == tileSetId)
                        {
                            SignalManager.Get<AddNewTileSetLinkSignal>().Dispatch(new BankLinkVO() { Caption = tileset.Name, Id = tileSetId });
                            break;
                        }
                    }
                }
            }

            return sourceBitmap;
        }
    }
}
