using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Signals;
using GBATool.Utils;
using Nett;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GBATool.Models
{
    public struct Sprite
    {
        public string ID { get; set; }
        public SpriteShape Shape { get; set; }
        public SpriteSize Size { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
    }

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

        private static readonly Dictionary<string, BitmapImage> BitmapCache = new();
        public Sprite[] Sprites { get; set; } = new Sprite[MaxSpriteSize];

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
                BitmapCache.Add(GUID, bitmap);
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
    }
}
