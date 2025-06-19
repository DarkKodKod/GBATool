using ArchitectureLibrary.Model;
using GBATool.FileSystem;
using GBATool.Models;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Windows.Media.Imaging;

namespace GBATool.Utils;

public static class TileSetUtils
{
    private readonly static ConcurrentDictionary<string, WriteableBitmap> _tileSetBitmapCache = [];

    public static WriteableBitmap? GetSourceBitmapFromCacheWithMetaData(TileSetModel model, ref BankImageMetaData metaData)
    {
        (bool foundInCache, WriteableBitmap? bitmap) = GetSourceBitmapFromCache(model);

        if (!foundInCache)
        {
            metaData.UniqueTileSet.Add(model.GUID);
        }

        return bitmap;
    }

    public static (bool foundInCache, WriteableBitmap? bitmap) GetSourceBitmapFromCache(string tileSetID)
    {
        TileSetModel? tileSetModel = ProjectFiles.GetModel<TileSetModel>(tileSetID);

        if (tileSetModel == null)
        {
            return (false, null);
        }

        return GetSourceBitmapFromCache(tileSetModel);
    }

    public static (bool foundInCache, WriteableBitmap? bitmap) GetSourceBitmapFromCache(TileSetModel model, bool forceRedraw = false)
    {
        if (string.IsNullOrEmpty(model.ImagePath))
        {
            return (false, null);
        }

        if (forceRedraw)
        {
            if (_tileSetBitmapCache.ContainsKey(model.GUID))
            {
                _tileSetBitmapCache.TryRemove(model.GUID, out WriteableBitmap? _);
            }
        }

        bool foundInCache = false;

        if (!_tileSetBitmapCache.TryGetValue(model.GUID, out WriteableBitmap? sourceBitmap))
        {
            ProjectModel projectModel = ModelManager.Get<ProjectModel>();

            string path = Path.Combine(projectModel.ProjectPath, model.ImagePath);

            if (!File.Exists(path))
            {
                return (false, null);
            }

            BitmapImage bmImage = new();
            bmImage.BeginInit();
            bmImage.CacheOption = BitmapCacheOption.OnLoad;
            bmImage.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bmImage.EndInit();
            bmImage.Freeze();

            sourceBitmap = BitmapFactory.ConvertToPbgra32Format(bmImage);
            sourceBitmap.Freeze();

            _tileSetBitmapCache.TryAdd(model.GUID, sourceBitmap);
        }
        else
        {
            foundInCache = true;
        }

        return (foundInCache, sourceBitmap);
    }
}
