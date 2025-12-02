using ArchitectureLibrary.Model;
using GBATool.FileSystem;
using GBATool.Models;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBATool.Utils;

public static class TileSetUtils
{
    private readonly static ConcurrentDictionary<string, WriteableBitmap> _tileSetBitmapCache = [];

    public static WriteableBitmap? GetSourceBitmapFromCacheWithMetaData(TileSetModel model, ref BankImageMetaData metaData)
    {
        (bool foundInCache, WriteableBitmap? bitmap) = GetSourceBitmapFromCache(model);

        if (foundInCache)
        {
            if (!metaData.UniqueTileSet.Exists(x => x == model.GUID))
            {
                metaData.UniqueTileSet.Add(model.GUID);
            }
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

    /// <summary>
    /// Creates a new ImageSource with the specified width/height
    /// Taken from the page: https://dlaa.me/blog/post/6129847
    /// The canvas when it is drawing an Image, it seems that it is scaling it for no reason. 
    /// Thi is forcing it to have the correct size.
    /// </summary>
    /// <param name="source">Source image to resize</param>
    /// <param name="width">Width of resized image</param>
    /// <param name="height">Height of resized image</param>
    /// <returns>Resized image</returns>
    public static ImageSource CreateResizedImage(ImageSource source, int width, int height)
    {
        // Target Rect for the resize operation
        Rect rect = new(0, 0, width, height);

        // Create a DrawingVisual/Context to render with
        DrawingVisual drawingVisual = new();
        using (DrawingContext drawingContext = drawingVisual.RenderOpen())
        {
            drawingContext.DrawImage(source, rect);
        }

        // Use RenderTargetBitmap to resize the original image
        RenderTargetBitmap resizedImage = new(
            (int)rect.Width, (int)rect.Height,  // Resized dimensions
            96, 96,                             // Default DPI values
            PixelFormats.Default);              // Default pixel format
        resizedImage.Render(drawingVisual);

        // Return the resized image
        return resizedImage;
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
