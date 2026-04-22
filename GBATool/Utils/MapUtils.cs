using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GBATool.Utils;

public static class MapUtils
{
    private readonly static ConcurrentDictionary<string, WriteableBitmap> _frameBitmapCache = [];

    public static int CellSize = 8;
    public static int MapSizeWidth = 32;

    public static int GetCellIndexFromPoint(Point point)
    {
        int cellIndex = ((int)point.X / CellSize) + ((int)point.Y / CellSize * MapSizeWidth);

        return cellIndex;
    }

    public static void InvalidateImageFromCache(string mapID)
    {
        if (_frameBitmapCache.ContainsKey(mapID))
        {
            _frameBitmapCache.TryRemove(mapID, out WriteableBitmap? _);
        }
    }
}
