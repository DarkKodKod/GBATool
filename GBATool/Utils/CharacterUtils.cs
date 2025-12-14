using GBATool.FileSystem;
using GBATool.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace GBATool.Utils;

public static class CharacterUtils
{
    public const int CanvasWidth = 232;
    public const int CanvasHeight = 152;

    private readonly static ConcurrentDictionary<string, WriteableBitmap> _frameBitmapCache = [];

    public static WriteableBitmap? GetFrameImageFromCache(CharacterModel characterModel, string animationID, string frameID)
    {
        if (!_frameBitmapCache.TryGetValue(frameID, out WriteableBitmap? sourceBitmap))
        {
            sourceBitmap = CreateImage(characterModel, animationID, frameID);

            if (sourceBitmap == null)
            {
                return null;
            }

            sourceBitmap.Freeze();

            _frameBitmapCache.TryAdd(frameID, sourceBitmap);
        }

        return sourceBitmap;
    }

    public static void InvalidateFrameImageFromCache(string frameID)
    {
        if (_frameBitmapCache.ContainsKey(frameID))
        {
            _frameBitmapCache.TryRemove(frameID, out WriteableBitmap? _);
        }
    }

    private static WriteableBitmap? CreateImage(CharacterModel characterModel, string animationID, string frameID)
    {
        if (characterModel.Animations[animationID].Frames == null ||
            characterModel.Animations[animationID].Frames.Count == 0)
        {
            return null;
        }

        if (characterModel.Animations[animationID].Frames[frameID].Tiles == null)
        {
            return null;
        }

        int minPosX = CanvasWidth;
        int minPosY = CanvasHeight;
        int maxPosX = 0;
        int maxPosY = 0;

        WriteableBitmap? bankBitmap = BitmapFactory.New(CanvasWidth, CanvasHeight);

        using (bankBitmap.GetBitmapContext())
        {
            Dictionary<string, CharacterSprite> listCharacterTile = characterModel.Animations[animationID].Frames[frameID].Tiles;

            foreach (KeyValuePair<string, CharacterSprite> tile in listCharacterTile)
            {
                CharacterSprite sprite = tile.Value;

                if (string.IsNullOrEmpty(sprite.SpriteID) || string.IsNullOrEmpty(sprite.TileSetID))
                {
                    continue;
                }

                TileSetModel? tileSetModel = ProjectFiles.GetModel<TileSetModel>(sprite.TileSetID);

                if (tileSetModel == null)
                {
                    continue;
                }

                SpriteModel? spriteModel = tileSetModel.Sprites.Find((item) => item.ID == sprite.SpriteID);

                if (spriteModel == null)
                {
                    continue;
                }

                (_, WriteableBitmap? bitmapCached) = TileSetUtils.GetSourceBitmapFromCache(tileSetModel);

                if (bitmapCached == null)
                {
                    continue;
                }

                WriteableBitmap sourceBitmap = bitmapCached.CloneCurrentValue();

                int width = 0;
                int height = 0;

                SpriteUtils.ConvertToWidthHeight(spriteModel.Shape, spriteModel.Size, ref width, ref height);

                WriteableBitmap cropped = sourceBitmap.Crop(spriteModel.PosX, spriteModel.PosY, width, height);

                using (cropped.GetBitmapContext())
                {
                    if (sprite.FlipHorizontal)
                    {
                        cropped = WriteableBitmapExtensions.Flip(cropped, WriteableBitmapExtensions.FlipMode.Vertical);
                    }

                    if (sprite.FlipVertical)
                    {
                        cropped = WriteableBitmapExtensions.Flip(cropped, WriteableBitmapExtensions.FlipMode.Horizontal);
                    }
                }

                int destX = (int)sprite.Position.X;
                int destY = (int)sprite.Position.Y;

                if (destX < minPosX)
                {
                    minPosX = destX;
                }
                if (destY < minPosY)
                {
                    minPosY = destY;
                }
                if (maxPosX < destX + width)
                {
                    maxPosX = destX + width;
                }
                if (maxPosY < destY + height)
                {
                    maxPosY = destY + height;
                }

                Util.CopyBitmapImageToWriteableBitmap(ref bankBitmap, destX, destY, cropped);
            }
        }

        if (minPosX != CanvasWidth && minPosY != CanvasHeight)
        {
            bankBitmap = bankBitmap.Crop(minPosX, minPosY, maxPosX - minPosX, maxPosY - minPosY);
            bankBitmap.Freeze();
        }

        return bankBitmap;
    }

    public static PaletteModel? GetPaletteUsedByCharacter(CharacterModel characterModel)
    {
        if (string.IsNullOrEmpty(characterModel.PaletteID))
        {
            return null;
        }

        PaletteModel? paletteModel = ProjectFiles.GetModel<PaletteModel>(characterModel.PaletteID);

        if (paletteModel == null)
        {
            return null;
        }

        return paletteModel;
    }
}
