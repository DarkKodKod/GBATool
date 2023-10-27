using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Signals;
using GBATool.Utils;
using Nett;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GBATool.Models
{
    public class TileSetModel : AFileModel
    {
        private const string _extensionKey = "extensionTileSets";

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

        public string ImagePath { get; set; } = string.Empty;
        public int ImageWidth { get; set; } = 0;
        public int ImageHeight { get; set; } = 0;
        public List<SpriteModel> Sprites { get; set; } = new();

        private static readonly ConcurrentDictionary<string, BitmapImage> BitmapCache = new();

        public TileSetModel()
        {
            SignalManager.Get<ProjectItemLoadedSignal>().Listener += OnProjectItemLoaded;
        }

        private void OnProjectItemLoaded(string id)
        {
            if (id != GUID)
            {
                return;
            }

            Util.GenerateBitmapFromTileSet(this, out BitmapImage? bitmap);

            if (bitmap != null && !BitmapCache.ContainsKey(GUID))
            {
                _ = BitmapCache.TryAdd(GUID, bitmap);
            }
        }

        public static BitmapImage? LoadBitmap(TileSetModel tileSetModel, bool forceRedraw = false)
        {
            bool exists = BitmapCache.TryGetValue(tileSetModel.GUID, out BitmapImage? bitmap);

            if (bitmap == null || !exists || forceRedraw)
            {
                Util.GenerateBitmapFromTileSet(tileSetModel, out bitmap);

                if (bitmap != null)
                {
                    BitmapCache[tileSetModel.GUID] = bitmap;
                }
            }

            return bitmap;
        }

        public bool RemoveSprite(string spriteID)
        {
            if (spriteID != null)
            {
                if (Sprites.RemoveAll((item) => item.ID == spriteID) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool RenameAliasSprite(string? spriteID, string newName)
        {
            if (string.IsNullOrEmpty(spriteID))
                return false;

            SpriteModel? find = Sprites.Find((item) => item.ID == spriteID);

            if (string.IsNullOrEmpty(find?.ID))
            {
                return false;
            }

            find.Alias = newName;

            return true;
        }

        public bool StoreNewSprite(string spriteID, int posX, int posY, SpriteShape shape, SpriteSize size, string tileSetID)
        {
            SpriteModel newSprite = new()
            {
                ID = spriteID,
                Shape = shape,
                Size = size,
                PosX = posX,
                PosY = posY,
                TileSetID = tileSetID,
                Alias = spriteID
            };

            SpriteModel? find = Sprites.Find((item) => item == newSprite);

            if (string.IsNullOrEmpty(find?.ID))
            {
                Sprites.Add(newSprite);

                return true;
            }

            return false;
        }
    }
}
