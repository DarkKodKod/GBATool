using ArchitectureLibrary.Model;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace GBATool.Utils;

public class SpriteInfo
{
    public BitmapSource? BitmapSource { get; set; }
    public int OffsetX { get; set; } = 0;
    public int OffsetY { get; set; } = 0;
}

public class BankImageMetaData
{
    public WriteableBitmap? image;
    public List<(int, string, string)> SpriteIndices = [];
    public List<string> UniqueTileSet = [];
    public List<SpriteModel> bankSprites = [];
    public Dictionary<string, SpriteInfo> Sprites = [];
}

public static class BankUtils
{
    public static readonly int SizeOfCellInPixels = 8;
    public static readonly int MaxTextureCellsWidth = 32;

    public static BankImageMetaData CreateImage(BankModel bankModel, bool foce2DView = false, int canvasWidth = 0, int canvasHeight = 0)
    {
        BankImageMetaData metaData = new();

        if (canvasWidth == 0)
        {
            canvasWidth = MaxTextureCellsWidth * SizeOfCellInPixels;
        }

        if (canvasHeight == 0)
        {
            canvasHeight = (bankModel.Use256Colors ? 16 : 32) * SizeOfCellInPixels;
        }

        WriteableBitmap bankBitmap = BitmapFactory.New(canvasWidth, canvasHeight);

        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        int index = 0;

        bool is1DImage = !foce2DView && (bankModel.IsBackground || (projectModel.SpritePatternFormat == SpritePattern.Format1D));

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

            WriteableBitmap? sourceBitmapCached = TileSetUtils.GetSourceBitmapFromCacheWithMetaData(tileSetModel, ref metaData);

            if (sourceBitmapCached == null)
            {
                continue;
            }

            WriteableBitmap sourceBitmap = sourceBitmapCached.CloneCurrentValue();

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

                // Keep the sprite as a separated image in a cache
                metaData.Sprites.Add(
                    sprite.ID,
                    new()
                    {
                        BitmapSource = sourceBitmap.Crop(posX, posY, width, height),
                        OffsetX = widthNextPosition,
                        OffsetY = heightNextPosition
                    });

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
}
