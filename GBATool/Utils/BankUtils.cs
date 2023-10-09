using ArchitectureLibrary.Model;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace GBATool.Utils
{
    public class BankImageMetaData
    {
        public WriteableBitmap? image;
        public List<(int, string)> SpriteIndices = new();
        public List<string> UniqueTileSet = new();
    }

    public static class BankUtils
    {
        private static readonly int MaxTextureCellsWidth = 32;
        private static readonly int MaxTextureCellsHeight = 32;
        public static readonly int SizeOfCellInPixels = 8;

        public static BankImageMetaData CreateImage(BankModel bankModel, ref Dictionary<string, WriteableBitmap> bitmapCache)
        {
            BankImageMetaData metaData = new();

            WriteableBitmap bankBitmap = BitmapFactory.New(MaxTextureCellsWidth * SizeOfCellInPixels, MaxTextureCellsHeight * SizeOfCellInPixels);

            ProjectModel projectModel = ModelManager.Get<ProjectModel>();

            int index = 0;

            bool is1DImage = bankModel.IsBackground || projectModel.SpritePatternFormat == SpritePattern.Format1D;

            foreach (SpriteModel sprite in bankModel.Sprites)
            {
                if (string.IsNullOrEmpty(sprite.ID) || string.IsNullOrEmpty(sprite.TileSetID))
                {
                    continue;
                }

                WriteableBitmap? sourceBitmap = GetSourceBitmapFromCache(ref bitmapCache, sprite.TileSetID, ref metaData);

                if (sourceBitmap == null)
                {
                    continue;
                }

                if (is1DImage)
                {
                    int width = 0;
                    int height = 0;

                    SpriteUtils.ConvertToWidthHeight(sprite.Shape, sprite.Size, ref width, ref height);

                    int posX = sprite.PosX;
                    int posY = sprite.PosY;

                    for (int j = 0; j < (height / SizeOfCellInPixels); ++j)
                    {
                        for (int i = 0; i < (width / SizeOfCellInPixels); ++i)
                        {
                            WriteableBitmap cropped = sourceBitmap.Crop(posX, posY, SizeOfCellInPixels, SizeOfCellInPixels);

                            int destX = index % MaxTextureCellsWidth * SizeOfCellInPixels;
                            int destY = index / MaxTextureCellsHeight * SizeOfCellInPixels;

                            Util.CopyBitmapImageToWriteableBitmap(ref bankBitmap, destX, destY, cropped);

                            metaData.SpriteIndices.Add((index, sprite.ID));

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

            metaData.image = bankBitmap;

            return metaData;
        }

        private static WriteableBitmap? GetSourceBitmapFromCache(ref Dictionary<string, WriteableBitmap> bitmapCache, string tileSetId, ref BankImageMetaData metaData)
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

                metaData.UniqueTileSet.Add(tileSetId);
            }

            return sourceBitmap;
        }
    }
}
