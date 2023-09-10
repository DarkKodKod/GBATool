using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.Utils;
using Nett;
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

        public static Dictionary<string, WriteableBitmap> BitmapCache = new();

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

            Util.GenerateBitmapFromTileSet(this, out WriteableBitmap? bitmap);

            if (bitmap != null && !BitmapCache.ContainsKey(GUID))
            {
                BitmapCache.Add(GUID, bitmap);
            }
        }

        public static WriteableBitmap? LoadBitmap(TileSetModel tileSetModel, bool forceRedraw = false)
        {
            bool exists = BitmapCache.TryGetValue(tileSetModel.GUID, out WriteableBitmap? bitmap);

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
