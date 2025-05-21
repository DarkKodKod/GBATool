using ArchitectureLibrary.Commands;
using ArchitectureLibrary.History.Signals;
using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.HistoryActions;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;

namespace GBATool.Commands.FileSystem;

public class ImportImageCommand : Command
{
    private const string _folderTileSetsKey = "folderTileSets";
    private const string _folderImagesKey = "folderImages";

    public override bool CanExecute(object? parameter)
    {
        if (parameter == null)
        {
            return false;
        }

        object[] values = (object[])parameter;
        string filePath = (string)values[0];

        if (string.IsNullOrEmpty(filePath))
        {
            return false;
        }

        if (!File.Exists(filePath))
        {
            return false;
        }

        return true;
    }

    public override void Execute(object? parameter)
    {
        if (parameter == null)
        {
            return;
        }

        object[] values = (object[])parameter;
        string filePath = (string)values[0];

        ProjectItem? item = null;

        if (values.Length > 1)
        {
            item = (ProjectItem)values[1];
        }

        if (item == null)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);

            item = CreateTileSetElement(name);
        }

        if (item.FileHandler?.FileModel is TileSetModel tileSet)
        {
            ProcessImage(item, tileSet, filePath);
        }
    }

    private static void ProcessImage(ProjectItem item, TileSetModel tileSet, string filePath)
    {
        string imagesFolder = (string)Application.Current.FindResource(_folderImagesKey);

        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        string imageFolderFullPath = Path.Combine(projectModel.ProjectPath, imagesFolder);

        if (!Directory.Exists(imageFolderFullPath))
        {
            _ = Directory.CreateDirectory(imageFolderFullPath);
        }

        string outputImagePath = Path.Combine(imageFolderFullPath, item.DisplayName + ".bmp");

        using Image image = Image.FromFile(filePath);

        if (filePath != outputImagePath)
        {
            image.Save(outputImagePath, ImageFormat.Bmp);
        }

        ProjectModel project = ModelManager.Get<ProjectModel>();

        string relativePath = Path.GetRelativePath(project.ProjectPath, outputImagePath);

        tileSet.ImagePath = relativePath;
        tileSet.ImageWidth = image.Width;
        tileSet.ImageHeight = image.Height;

        item.FileHandler?.Save();

        SignalManager.Get<UpdateTileSetImageSignal>().Dispatch();
    }

    private static ProjectItem CreateTileSetElement(string name)
    {
        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        string tileSets = (string)Application.Current.FindResource(_folderTileSetsKey);

        string path = Path.Combine(projectModel.ProjectPath, tileSets);

        name = ProjectItemFileSystem.GetValidFileName(
            path,
            name,
            Util.GetExtensionByType(ProjectItemType.TileSet));

        ProjectItem newElement = new()
        {
            DisplayName = name,
            IsFolder = false,
            IsRoot = false,
            Type = ProjectItemType.TileSet
        };

        SignalManager.Get<RegisterHistoryActionSignal>().Dispatch(new CreateNewElementHistoryAction(newElement));

        SignalManager.Get<FindAndCreateElementSignal>().Dispatch(newElement);

        ProjectItemFileSystem.CreateFileElement(newElement, path, name);

        return newElement;
    }
}
