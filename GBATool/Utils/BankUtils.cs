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
        public List<(int, string, string)> SpriteIndices = new();
        public List<string> UniqueTileSet = new();
        public List<SpriteModel> bankSprites = new();
    }

    public static class BankUtils
    {
        public static readonly int SizeOfCellInPixels = 8;
        public static readonly int MaxTextureCellsWidth = 32;

        public static int MaxTextureCellsHeight(bool is256color)
        {
            return is256color ? 16 : 32;
        }

        public static BankImageMetaData CreateImage(BankModel bankModel, ref Dictionary<string, WriteableBitmap> bitmapCache)
        {
            BankImageMetaData metaData = new();

            int canvasWidth = MaxTextureCellsWidth * SizeOfCellInPixels;
            int canvasHeight = MaxTextureCellsHeight(bankModel.Use256Colors) * SizeOfCellInPixels;

            WriteableBitmap bankBitmap = BitmapFactory.New(canvasWidth, canvasHeight);

            ProjectModel projectModel = ModelManager.Get<ProjectModel>();

            int index = 0;

            bool is1DImage = bankModel.IsBackground || projectModel.SpritePatternFormat == SpritePattern.Format1D;

            int widthNextPosition = 0;
            int heightNextPosition = 0;
            int keepHeightPosition = 0;

            foreach (SpriteRef spriteRef in bankModel.Sprites)
            {
                if (string.IsNullOrEmpty(spriteRef.SpriteID) || string.IsNullOrEmpty(spriteRef.TileSetID))
                {
                    continue;
                }

                TileSetModel? tileSetModel = ProjectFiles.GetModel<TileSetModel>(spriteRef.TileSetID);

                if (tileSetModel == null)
                {
                    continue;
                }

                WriteableBitmap? sourceBitmap = GetSourceBitmapFromCache(ref bitmapCache, tileSetModel, ref metaData);

                if (sourceBitmap == null)
                {
                    continue;
                }

                SpriteModel? sprite = tileSetModel.Sprites.Find((item) => item.ID == spriteRef.SpriteID);

                if (string.IsNullOrEmpty(sprite?.ID))
                {
                    continue;
                }

                metaData.bankSprites.Add(sprite);

                int width = 0;
                int height = 0;

                SpriteUtils.ConvertToWidthHeight(sprite.Shape, sprite.Size, ref width, ref height);

                if (widthNextPosition + width > canvasWidth)
                {
                    widthNextPosition = 0;
                    heightNextPosition += keepHeightPosition;
                    keepHeightPosition = 0;

                    // In case the next sprite is going to be place outside of the destinated bank size.
                    if (heightNextPosition + height > canvasHeight)
                    {
                        break;
                    }
                }

                int posX = sprite.PosX;
                int posY = sprite.PosY;

                if (is1DImage)
                {
                    for (int j = 0; j < (height / SizeOfCellInPixels); ++j)
                    {
                        for (int i = 0; i < (width / SizeOfCellInPixels); ++i)
                        {
                            WriteableBitmap cropped = sourceBitmap.Crop(posX, posY, SizeOfCellInPixels, SizeOfCellInPixels);

                            int destX = index % MaxTextureCellsWidth * SizeOfCellInPixels;
                            int destY = index / MaxTextureCellsWidth * SizeOfCellInPixels;

                            Util.CopyBitmapImageToWriteableBitmap(ref bankBitmap, destX, destY, cropped);

                            metaData.SpriteIndices.Add((index, sprite.ID, sprite.TileSetID));

                            posX += SizeOfCellInPixels;

                            index++;
                        }

                        posX = sprite.PosX;
                        posY += SizeOfCellInPixels;
                    }
                }
                else
                {
                    index = (MaxTextureCellsWidth * (heightNextPosition / SizeOfCellInPixels)) + (widthNextPosition / SizeOfCellInPixels);

                    // 2D
                    for (int j = 0; j < (height / SizeOfCellInPixels); ++j)
                    {
                        for (int i = 0; i < (width / SizeOfCellInPixels); ++i)
                        {
                            WriteableBitmap cropped = sourceBitmap.Crop(posX, posY, SizeOfCellInPixels, SizeOfCellInPixels);

                            int destX = i * SizeOfCellInPixels;
                            int destY = j * SizeOfCellInPixels;

                            Util.CopyBitmapImageToWriteableBitmap(ref bankBitmap, destX + widthNextPosition, destY + heightNextPosition, cropped);

                            metaData.SpriteIndices.Add((index, sprite.ID, sprite.TileSetID));

                            posX += SizeOfCellInPixels;

                            index++;
                        }

                        index += MaxTextureCellsWidth - (width / SizeOfCellInPixels);

                        posX = sprite.PosX;
                        posY += SizeOfCellInPixels;
                    }

                    widthNextPosition += width;

                    if (height > keepHeightPosition)
                    {
                        keepHeightPosition = height;
                    }
                }
            }

            metaData.image = bankBitmap;

            return metaData;
        }

        private static WriteableBitmap? GetSourceBitmapFromCache(ref Dictionary<string, WriteableBitmap> bitmapCache, TileSetModel model, ref BankImageMetaData metaData)
        {
            if (!bitmapCache.TryGetValue(model.GUID, out WriteableBitmap? sourceBitmap))
            {
                ProjectModel projectModel = ModelManager.Get<ProjectModel>();

                string path = Path.Combine(projectModel.ProjectPath, model.ImagePath);

                BitmapImage bmImage = new();
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                bmImage.EndInit();
                bmImage.Freeze();
                sourceBitmap = BitmapFactory.ConvertToPbgra32Format(bmImage);

                bitmapCache.Add(model.GUID, sourceBitmap);

                metaData.UniqueTileSet.Add(model.GUID);
            }

            return sourceBitmap;
        }
    }
}
