﻿using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;
using Nett;
using System;
using System.IO;

namespace GBATool.FileSystem
{
    public static class ProjectItemFileSystem
    {
        public static void Initialize()
        {
            SignalManager.Get<RegisterFileHandlerSignal>().Listener += OnRegisterFileHandler;
            SignalManager.Get<RenameFileSignal>().Listener += OnRenameFile;
            SignalManager.Get<MoveElementSignal>().Listener += OnMoveElement;
        }

        private static void OnRenameFile(ProjectItem item)
        {
            if (item.FileHandler == null)
            {
                return;
            }

            FileHandler fileHandler = item.FileHandler;

            if (fileHandler.FileModel != null)
            {
                string itemPath = Path.Combine(fileHandler.Path, fileHandler.Name + fileHandler.FileModel.FileExtension);
                string itemNewPath = Path.Combine(fileHandler.Path, item.DisplayName + fileHandler.FileModel.FileExtension);

                if (itemPath == itemNewPath)
                {
                    return;
                }

                if (File.Exists(itemPath))
                {
                    File.Move(itemPath, itemNewPath);
                }
            }
            else
            {
                // Is a folder
                string itemPath = Path.Combine(fileHandler.Path, fileHandler.Name);
                string itemNewPath = Path.Combine(fileHandler.Path, item.DisplayName);

                if (Directory.Exists(itemPath))
                {
                    if (itemPath == itemNewPath)
                    {
                        return;
                    }

                    Directory.Move(itemPath, itemNewPath);

                    UpdatePath(item, itemNewPath);
                }
            }

            fileHandler.Name = item.DisplayName;
        }

        private static async void OnRegisterFileHandler(ProjectItem item, string? path)
        {
            FileHandler fileHandler = new() { Name = item.DisplayName, Path = path ?? string.Empty };

            item.FileHandler = fileHandler;

            if (item.IsFolder)
            {
                return;
            }

            AFileModel? model = Util.FileModelFactory(item.Type);

            if (model == null)
            {
                return;
            }

            string itemPath = Path.Combine(fileHandler.Path, fileHandler.Name + model.FileExtension);

            if (!File.Exists(itemPath))
            {
                return;
            }

            fileHandler.FileModel = await FileUtils.ReadFileAndLoadModelAsync(itemPath, item.Type).ConfigureAwait(false);

            if (string.IsNullOrEmpty(fileHandler.FileModel?.GUID))
            {
                if (fileHandler.FileModel != null)
                {
                    fileHandler.FileModel.GUID = Guid.NewGuid().ToString();
                }
            }

            if (fileHandler.FileModel == null)
            {
                return;
            }

            if (ProjectFiles.Handlers.ContainsKey(fileHandler.FileModel.GUID))
            {
                return;
            }

            _ = ProjectFiles.Handlers.TryAdd(fileHandler.FileModel.GUID, fileHandler);

            SignalManager.Get<ProjectItemLoadedSignal>().Dispatch(fileHandler.FileModel.GUID);

            ProjectFiles.ObjectsLoading--;

            if (ProjectFiles.ObjectsLoading <= 0)
            {
                SignalManager.Get<FinishedLoadingProjectSignal>().Dispatch();
            }
        }

        public static string GetValidFolderName(string path, string name)
        {
            int counter = 1;
            string outName = name;

            string folderPath = Path.Combine(path, name);

            while (Directory.Exists(folderPath))
            {
                outName = name + "_" + counter;

                folderPath = Path.Combine(path, outName);

                counter++;
            }

            return outName;
        }

        public static string GetValidFileName(string path, string name, string extension)
        {
            int counter = 1;
            string outName = name;

            string filePath = Path.Combine(path, name + extension);

            while (File.Exists(filePath))
            {
                outName = name + "_" + counter;

                filePath = Path.Combine(path, outName + extension);

                counter++;
            }

            return outName;
        }

        public static void CreateElement(ProjectItem item, string path, string name)
        {
            if (item.IsFolder)
            {
                CreateFolderElement(item, path, name);
            }
            else
            {
                CreateFileElement(item, path, name);
            }
        }

        private static void CreateFolderElement(ProjectItem item, string path, string name)
        {
            string folderPath = Path.Combine(path, name);

            foreach (ProjectItem itm in item.Items)
            {
                if (itm.IsFolder)
                {
                    CreateFolderElement(itm, folderPath, itm.DisplayName);
                }
                else
                {
                    CreateFileElement(itm, folderPath, itm.DisplayName);
                }
            }

            CreateFileElement(item, path, name);
        }

        public static void CreateFileElement(ProjectItem item, string path, string name)
        {
            item.FileHandler = new FileHandler() { Name = name, Path = path };

            if (!item.IsFolder)
            {
                AFileModel? model = Util.FileModelFactory(item.Type);

                if (model != null)
                {
                    string filePath = Path.Combine(path, name + model.FileExtension);

                    Toml.WriteFile(model, filePath);

                    item.FileHandler.FileModel = model;

                    _ = ProjectFiles.Handlers.TryAdd(item.FileHandler.FileModel.GUID, item.FileHandler);
                }
            }
            else
            {
                string folderPath = Path.Combine(path, name);

                _ = Directory.CreateDirectory(folderPath);
            }
        }

        private static void OnMoveElement(ProjectItem targetElement, ProjectItem draggedElement)
        {
            if (targetElement.FileHandler == null ||
                draggedElement.FileHandler == null)
            {
                return;
            }

            string destFolder;

            if (targetElement.IsFolder)
            {
                destFolder = Path.Combine(targetElement.FileHandler.Path, targetElement.FileHandler.Name);
            }
            else
            {
                destFolder = Path.Combine(targetElement.FileHandler.Path);
            }

            if (draggedElement.IsFolder)
            {
                string originalPath = Path.Combine(draggedElement.FileHandler.Path, draggedElement.FileHandler.Name);
                string destinationPath = Path.Combine(destFolder, draggedElement.FileHandler.Name);

                Directory.Move(originalPath, destinationPath);

                UpdatePath(draggedElement, destinationPath);
            }
            else
            {
                string fileName = draggedElement.FileHandler.Name + draggedElement.FileHandler.FileModel?.FileExtension;

                string originalFile = Path.Combine(draggedElement.FileHandler.Path, fileName);
                string destinationFile = Path.Combine(destFolder, fileName);

                File.Move(originalFile, destinationFile);
            }

            draggedElement.FileHandler.Path = destFolder;
            draggedElement.FileHandler.Name = draggedElement.DisplayName;
        }

        private static void UpdatePath(ProjectItem rootElement, string destinationPath)
        {
            foreach (ProjectItem item in rootElement.Items)
            {
                if (item.FileHandler == null)
                {
                    continue;
                }

                item.FileHandler.Path = destinationPath;

                if (item.IsFolder)
                {
                    UpdatePath(item, Path.Combine(destinationPath, item.DisplayName));
                }
            }
        }

        private static void DeleteFolders(ProjectItem item)
        {
            foreach (ProjectItem itm in item.Items)
            {
                if (itm.IsFolder)
                {
                    DeleteFolders(itm);
                }
                else
                {
                    DeleteElement(itm.FileHandler);
                }
            }

            DeleteElement(item.FileHandler);
        }

        private static void DeleteElement(FileHandler? fileHandler)
        {
            if (fileHandler == null)
            {
                return;
            }

            if (fileHandler.FileModel != null)
            {
                string itemPath = Path.Combine(fileHandler.Path, fileHandler.Name + fileHandler.FileModel.FileExtension);

                if (File.Exists(itemPath))
                {
                    File.Delete(itemPath);
                }

                _ = ProjectFiles.Handlers.TryRemove(fileHandler.FileModel.GUID, out _);
            }
            else
            {
                // Is a folder
                string itemPath = Path.Combine(fileHandler.Path, fileHandler.Name);

                if (Directory.Exists(itemPath))
                {
                    Directory.Delete(itemPath, true);
                }
            }
        }

        // todo: when deleting a folder we have to iterate all the sub folders deleting everything!
        public static void DeteElement(ProjectItem item)
        {
            if (item.IsFolder)
            {
                DeleteFolders(item);
            }
            else
            {
                DeleteElement(item.FileHandler);
            }
        }
    }
}
