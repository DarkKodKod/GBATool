using ArchitectureLibrary.Model;
using GBATool.Enums;
using GBATool.Models;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBATool.Utils;

public static class Util
{
    private static readonly Regex _regex = new(@"^[A-Za-z_][a-zA-Z0-9_\-\x20]*$");

    private const string _folderBanksKey = "folderBanks";
    private const string _folderCharactersKey = "folderCharacters";
    private const string _folderMapsKey = "folderMaps";
    private const string _folderTileSetsKey = "folderTileSets";
    private const string _folderPalettesKey = "folderPalettes";
    private const string _folderWorldsKey = "folderWorlds";
    private const string _folderEntitiesKey = "folderEntities";
    private const string _extensionBanksKey = "extensionBanks";
    private const string _extensionCharactersKey = "extensionCharacters";
    private const string _extensionMapsKey = "extensionMaps";
    private const string _extensionTileSetsKey = "extensionTileSets";
    private const string _extensionPalettesKey = "extensionPalettes";
    private const string _extensionWorldsKey = "extensionWorlds";
    private const string _extensionEntitiesKey = "extensionEntities";

    public static AFileModel? FileModelFactory(ProjectItemType type)
    {
        return type switch
        {
            ProjectItemType.Bank => new BankModel(),
            ProjectItemType.Character => new CharacterModel(),
            ProjectItemType.Map => new MapModel(),
            ProjectItemType.TileSet => new TileSetModel(),
            ProjectItemType.Palette => new PaletteModel(),
            ProjectItemType.World => new WorldModel(),
            ProjectItemType.Entity => new EntityModel(),
            _ => null,
        };
    }

    public static ProjectItemType GetItemType(string extension)
    {
        string extensionBanks = (string)Application.Current.FindResource(_extensionBanksKey);
        string extensionCharacters = (string)Application.Current.FindResource(_extensionCharactersKey);
        string extensionMaps = (string)Application.Current.FindResource(_extensionMapsKey);
        string extensionTileSets = (string)Application.Current.FindResource(_extensionTileSetsKey);
        string extensionPalettes = (string)Application.Current.FindResource(_extensionPalettesKey);
        string extensionWorld = (string)Application.Current.FindResource(_extensionWorldsKey);
        string extensionEntities = (string)Application.Current.FindResource(_extensionEntitiesKey);

        if (extension == extensionBanks) return ProjectItemType.Bank;
        if (extension == extensionCharacters) return ProjectItemType.Character;
        if (extension == extensionMaps) return ProjectItemType.Map;
        if (extension == extensionTileSets) return ProjectItemType.TileSet;
        if (extension == extensionPalettes) return ProjectItemType.Palette;
        if (extension == extensionWorld) return ProjectItemType.World;
        if (extension == extensionEntities) return ProjectItemType.Entity;

        return ProjectItemType.None;
    }

    public static string GetExtensionByType(ProjectItemType type)
    {
        string extensionBanks = (string)Application.Current.FindResource(_extensionBanksKey);
        string extensionCharacters = (string)Application.Current.FindResource(_extensionCharactersKey);
        string extensionMaps = (string)Application.Current.FindResource(_extensionMapsKey);
        string extensionTileSets = (string)Application.Current.FindResource(_extensionTileSetsKey);
        string extensionPalettes = (string)Application.Current.FindResource(_extensionPalettesKey);
        string extensionWorlds = (string)Application.Current.FindResource(_extensionWorldsKey);
        string extensionEntities = (string)Application.Current.FindResource(_extensionEntitiesKey);

        return type switch
        {
            ProjectItemType.Bank => extensionBanks,
            ProjectItemType.Character => extensionCharacters,
            ProjectItemType.Map => extensionMaps,
            ProjectItemType.TileSet => extensionTileSets,
            ProjectItemType.Palette => extensionPalettes,
            ProjectItemType.World => extensionWorlds,
            ProjectItemType.Entity => extensionEntities,
            _ => string.Empty,
        };
    }

    public static string GetFolderExtension(string folderName)
    {
        string folderBanks = (string)Application.Current.FindResource(_folderBanksKey);
        string folderCharacters = (string)Application.Current.FindResource(_folderCharactersKey);
        string folderMaps = (string)Application.Current.FindResource(_folderMapsKey);
        string folderTileSets = (string)Application.Current.FindResource(_folderTileSetsKey);
        string folderPalettes = (string)Application.Current.FindResource(_folderPalettesKey);
        string folderWorlds = (string)Application.Current.FindResource(_folderWorldsKey);
        string folderEntities = (string)Application.Current.FindResource(_folderEntitiesKey);

        string extensionBanks = (string)Application.Current.FindResource(_extensionBanksKey);
        string extensionCharacters = (string)Application.Current.FindResource(_extensionCharactersKey);
        string extensionMaps = (string)Application.Current.FindResource(_extensionMapsKey);
        string extensionTileSets = (string)Application.Current.FindResource(_extensionTileSetsKey);
        string extensionPalettes = (string)Application.Current.FindResource(_extensionPalettesKey);
        string extensionWorlds = (string)Application.Current.FindResource(_extensionWorldsKey);
        string extensionEntities = (string)Application.Current.FindResource(_extensionEntitiesKey);

        if (folderName == folderBanks) return extensionBanks;
        if (folderName == folderCharacters) return extensionCharacters;
        if (folderName == folderMaps) return extensionMaps;
        if (folderName == folderTileSets) return extensionTileSets;
        if (folderName == folderPalettes) return extensionPalettes;
        if (folderName == folderWorlds) return extensionWorlds;
        if (folderName == folderEntities) return extensionEntities;

        return string.Empty;
    }

    public static bool ValidFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return true;
        }

        return _regex != null && _regex.IsMatch(fileName);
    }

    public static bool IsPointInTopHalf(ItemsControl itemsControl, DragEventArgs e)
    {
        UIElement? selectedItemContainer = GetItemContainerFromPoint(itemsControl, e.GetPosition(itemsControl));

        if (selectedItemContainer != null)
        {
            Point relativePosition = e.GetPosition(selectedItemContainer);

            return relativePosition.Y < ((FrameworkElement)selectedItemContainer).ActualHeight / 2;
        }

        return false;
    }

    public static UIElement? GetItemContainerFromPoint(ItemsControl itemsControl, Point p)
    {
        UIElement? element = itemsControl?.InputHitTest(p) as UIElement;
        while (element != null)
        {
            if (element == itemsControl)
            {
                return element;
            }

            object? data = itemsControl?.ItemContainerGenerator.ItemFromContainer(element);

            if (data != null && data != DependencyProperty.UnsetValue)
            {
                return element;
            }
            else
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
            }
        }

        return null;
    }

    public static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        do
        {
            if (current is T t)
            {
                return t;
            }
            current = VisualTreeHelper.GetParent(current);
        }
        while (current != null);
        return null;
    }

    public static void GenerateBitmapFromTileSet(TileSetModel model, out BitmapImage? bitmap)
    {
        bitmap = null;

        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        if (string.IsNullOrEmpty(model.ImagePath))
        {
            return;
        }

        string path = Path.Combine(projectModel.ProjectPath, model.ImagePath);

        if (File.Exists(path))
        {
            bitmap = new();

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bitmap.EndInit();
            bitmap.Freeze();
        }
    }

    public static void CopyBitmapImageToWriteableBitmap(ref WriteableBitmap dest, int nXDest, int nYDest, WriteableBitmap src)
    {
        using (dest.GetBitmapContext())
        {
            // copy the source image into a byte buffer
            int src_stride = src.PixelWidth * (src.Format.BitsPerPixel >> 3);
            byte[] src_buffer = new byte[src_stride * src.PixelHeight];
            src.CopyPixels(src_buffer, src_stride, 0);

            int dest_stride = src.PixelWidth * (dest.Format.BitsPerPixel >> 3);
            byte[] dest_buffer = new byte[(src.PixelWidth * src.PixelHeight) << 2];

            // do merge (could be made faster through parallelization), alpha channel is not used at all
            for (int i = 0; i < src_buffer.Length; i += 4)
            {
                dest_buffer[i + 0] = src_buffer[i + 0];
                dest_buffer[i + 1] = src_buffer[i + 1];
                dest_buffer[i + 2] = src_buffer[i + 2];
                dest_buffer[i + 3] = 255;
            }

            // copy dest buffer back to the dest WriteableBitmap
            dest.WritePixels(new Int32Rect(nXDest, nYDest, src.PixelWidth, src.PixelHeight), dest_buffer, dest_stride, 0);
        }
    }

    public static Color GetColorFromInt(int colorInt)
    {
        byte R = (byte)(colorInt >> 16);
        byte G = (byte)(colorInt >> 8);
        byte B = (byte)colorInt;

        return Color.FromRgb(R, G, B);
    }

    public static int GetIntFromColor(Color color)
    {
        return (color.R << 16) | (color.G << 8) | (color.B);
    }
}
