using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Signals;
using GBATool.Utils;
using Nett;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GBATool.Models
{
    public class TileSetModel : AFileModel
    {
        public static readonly int MaxSpriteSize = 100;

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
        public SpriteModel[] Sprites { get; set; } = new SpriteModel[MaxSpriteSize];

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
            for (int i = 0; i < MaxSpriteSize; i++)
            {
                if (Sprites[i].ID == spriteID)
                {
                    Sprites[i].ID = string.Empty;

                    return true;
                }
            }

            return false;
        }

        public bool StoreNewSprite(string spriteID, int posX, int posY, SpriteShape shape, SpriteSize size, string tileSetID)
        {
            for (int i = 0; i < MaxSpriteSize; i++)
            {
                if (string.IsNullOrEmpty(Sprites[i].ID))
                {
                    Sprites[i].ID = spriteID;
                    Sprites[i].Shape = shape;
                    Sprites[i].Size = size;
                    Sprites[i].PosX = posX;
                    Sprites[i].PosY = posY;
                    Sprites[i].TileSetID = tileSetID;

                    return true;
                }
            }

            MessageBox.Show($"Cannot save more sprites under this TileSet, max of {MaxSpriteSize} limit is reached", "Error", MessageBoxButton.OK);

            return false;
        }
    }
}
