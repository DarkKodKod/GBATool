﻿using ArchitectureLibrary.History.Signals;
using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using ArchitectureLibrary.WPF.Adorners;
using GBATool.Commands;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.HistoryActions;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace GBATool.ViewModels;

public class MainWindowViewModel : ViewModel
{
    #region Commands
    public OpenProjectCommand OpenProjectCommand { get; } = new();
    public CloseProjectCommand CloseProjectCommand { get; } = new();
    public DispatchSignalCommand<ExitSuccessSignal> ExitCommand { get; } = new();
    public NewProjectCommand NewProjectCommand { get; } = new();
    public NewElementCommand NewElementCommand { get; } = new();
    public LoadConfigsCommand LoadConfigsCommand { get; } = new();
    public ShowAboutDialogCommand ShowAboutDialogCommand { get; } = new();
    public OpenBuildProjectCommand OpenBuildProjectCommand { get; } = new();
    public OpenProjectPropertiesCommand OpenProjectPropertiesCommand { get; } = new();
    public ViewHelpCommand ViewHelpCommand { get; } = new();
    public CopyElementCommand CopyElementCommand { get; } = new();
    public PasteElementCommand PasteElementCommand { get; } = new();
    public DuplicateElementCommand DuplicateElementCommand { get; } = new();
    public DeleteElementCommand DeleteElementCommand { get; } = new();
    public EnableRenameElementCommand EnableRenameElementCommand { get; } = new();
    public CreateFolderCommand CreateFolderCommand { get; } = new();
    public CreateElementFromMenuCommand CreateElementFromMenuCommand { get; } = new();
    public PreviewMouseLeftButtonDownCommand PreviewMouseLeftButtonDownCommand { get; } = new();
    public DispatchSignalCommand<MouseLeftButtonUpSignal> PreviewMouseLeftButtonUpCommand { get; } = new();
    public PreviewMouseMoveCommand PreviewMouseMoveCommand { get; } = new();
    public DragEnterCommand DragEnterCommand { get; } = new();
    public DragOverCommand DragOverCommand { get; } = new();
    public DragLeaveCommand DragLeaveCommand { get; } = new();
    public DropCommand DropCommand { get; } = new();
    public OpenImportImageDialogCommand OpenImportImageDialogCommand { get; } = new();
    public WindowsGetFocusCommand WindowsGetFocusCommand { get; } = new();
    public QueryContinueDragCommand QueryContinueDragCommand { get; } = new();
    public UndoCommand UndoCommand { get; } = new();
    public RedoCommand RedoCommand { get; } = new();
    public TreeviewSelectedItemChangedCommand TreeviewSelectedItemChangedCommand { get; } = new();
    #endregion

    private const string _projectNameKey = "applicationTitle";
    private const string _folderPalettesKey = "folderPalettes";

    private string _title = "";
    private string _projectName = "";
    private List<ProjectItem>? _projectItems = [];
    private List<RecentProjectModel> _recentProjects = [];
    private bool? _isFullscreen = null;
    private readonly string _appName = "";

    #region Drag & Drop
    private Point? _startPoint = null;
    private TreeViewInsertAdorner? _insertAdorner = null;
    private TreeViewDragAdorner? _dragAdorner = null;
    private bool _isDragging = false;
    #endregion

    #region get/set
    public string ProjectName
    {
        get => _projectName;
        set
        {
            _projectName = value;
            OnPropertyChanged("ProjectName");
        }
    }

    public List<RecentProjectModel> RecentProjects
    {
        get => _recentProjects;
        set
        {
            _recentProjects = value;
            OnPropertyChanged("RecentProjects");
        }
    }

    public List<ProjectItem>? ProjectItems
    {
        get => _projectItems;
        set
        {
            _projectItems = value;
            OnPropertyChanged("ProjectItems");
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged("Title");
        }
    }
    #endregion

    public MainWindowViewModel()
    {
        _appName = (string)Application.Current.FindResource(_projectNameKey);

        Title = _appName;

        #region Signals
        SignalManager.Get<OpenProjectSuccessSignal>().Listener += OpenProjectSuccess;
        SignalManager.Get<CloseProjectSuccessSignal>().Listener += OnCloseProjectSuccess;
        SignalManager.Get<ExitSuccessSignal>().Listener += OnExitSuccess;
        SignalManager.Get<LoadConfigSuccessSignal>().Listener += OnLoadConfigSuccess;
        SignalManager.Get<UpdateRecentProjectsSignal>().Listener += OnUpdateRecentProjects;
        SignalManager.Get<MouseLeftButtonDownSignal>().Listener += OnMouseLeftButtonDown;
        SignalManager.Get<MouseLeftButtonUpSignal>().Listener += OnMouseLeftButtonUp;
        SignalManager.Get<MouseMoveSignal>().Listener += OnMouseMove;
        SignalManager.Get<UpdateAdornersSignal>().Listener += OnUpdateAdorners;
        SignalManager.Get<InitializeAdornersSignal>().Listener += OnInitializeAdorners;
        SignalManager.Get<DetachAdornersSignal>().Listener += OnDetachAdorners;
        SignalManager.Get<SizeChangedSignal>().Listener += OnSizeChanged;
        SignalManager.Get<CreateProjectSuccessSignal>().Listener += OnCreateProjectSuccess;
        SignalManager.Get<DeleteElementSignal>().Listener += OnDeleteElement;
        SignalManager.Get<PasteElementSignal>().Listener += OnPasteElement;
        SignalManager.Get<FindAndCreateElementSignal>().Listener += OnFindAndCreateElement;
        SignalManager.Get<DropElementSignal>().Listener += OnDropElement;
        SignalManager.Get<TryCreatePaletteElementSignal>().Listener += OnTryCreatePaletteElement;
        #endregion
    }

    #region Signal methods
    private static ProjectItem? FindInItemsAndDelete(ICollection<ProjectItem>? items, ProjectItem[] aPath, int index)
    {
        if (items == null)
        {
            return null;
        }

        bool res = items.Contains(aPath[index]);

        if (res && index > 0)
        {
            return FindInItemsAndDelete(aPath[index].Items, aPath, index - 1);
        }

        ProjectItem copy = aPath[index];

        aPath[index].Parent?.Items.Remove(aPath[index]);

        return copy;
    }

    private void OnPasteElement(ProjectItem selectedItem, ProjectItem newItem)
    {
        if (selectedItem.IsFolder)
        {
            newItem.Parent = selectedItem;
            selectedItem.Items.Add(newItem);
        }
        else
        {
            newItem.Parent = selectedItem.Parent;
            selectedItem.Parent?.Items.Add(newItem);
        }

        if (newItem.FileHandler != null && newItem.FileHandler.FileModel != null)
        {
            newItem.FileHandler.FileModel.GUID = Guid.NewGuid().ToString();

            _ = ProjectFiles.Handlers.TryAdd(newItem.FileHandler.FileModel.GUID, newItem.FileHandler);

            newItem.FileHandler.Save();
        }

        OnPropertyChanged("ProjectItems");
    }

    private void OnDeleteElement(ProjectItem item)
    {
        DeleteItemFromTheList(item);
    }

    private void DeleteItemFromTheList(ProjectItem item)
    {
        // Collect the chain of parents for later use
        List<ProjectItem> path = [item];

        ProjectItem? originalParent = item.Parent;

        ProjectItem? parent = item.Parent;

        while (parent != null)
        {
            path.Add(parent);

            parent = parent.Parent;
        }

        ProjectItem? matchItem = FindInItemsAndDelete(_projectItems, path.ToArray(), path.ToArray().Length - 1);

        if (matchItem != null)
        {
            OnPropertyChanged("ProjectItems");

            if (originalParent != null)
            {
                SignalManager.Get<UpdateFolderSignal>().Dispatch(originalParent);
            }
        }
    }

    private void OnLoadConfigSuccess()
    {
        GBAToolConfigurationModel model = ModelManager.Get<GBAToolConfigurationModel>();

        OnUpdateRecentProjects(model.RecentProjects);

        LoadDefaultProject();
    }

    private static void LoadDefaultProject()
    {
        GBAToolConfigurationModel config = ModelManager.Get<GBAToolConfigurationModel>();

        if (!string.IsNullOrEmpty(config.DefaultProjectPath))
        {
            using OpenProjectCommand openProjectCommand = new();

            if (openProjectCommand.CanExecute(config.DefaultProjectPath))
            {
                openProjectCommand.Execute(config.DefaultProjectPath);
            }
        }
    }

    private void OnCreateProjectSuccess(string projectFullPath)
    {
        // After creating the project now it is time to open it
        using OpenProjectCommand openProjectCommand = new();

        if (openProjectCommand.CanExecute(projectFullPath))
        {
            openProjectCommand.Execute(projectFullPath);
        }
    }

    private void OnUpdateRecentProjects(string[] recentProjects)
    {
        List<RecentProjectModel> list = [];

        int index = 1;

        foreach (string project in recentProjects)
        {
            if (!string.IsNullOrEmpty(project))
            {
                // Extract the name of the folder as our project name
                int startIndex = project.LastIndexOf("\\");
                string projectName = project.Substring(startIndex + 1, project.Length - startIndex - 1);

                list.Add(new RecentProjectModel()
                {
                    Path = project,
                    DisplayName = $"_{index} {projectName} ({project})"
                });

                index++;
            }
        }

        RecentProjects = list;
    }

    private void OpenProjectSuccess(ProjectOpenVO vo)
    {
        ProjectItems = vo.Items;

        ProjectName = vo.ProjectName;

        Title = $"{vo.ProjectName} - {_appName}";

        ProjectModel project = ModelManager.Get<ProjectModel>();

        if (project.Name != vo.ProjectName)
        {
            project.Name = vo.ProjectName;

            project.Save();
        }

        project.Name = vo.ProjectName;
    }

    private void OnCloseProjectSuccess()
    {
        ProjectItems = null;

        ProjectName = "";

        Title = $"{_appName}";
    }

    private void OnExitSuccess()
    {
        Application.Current.Shutdown();
    }

    private void OnMouseLeftButtonDown(Point point)
    {
        _startPoint = point;

        _isDragging = false;
    }

    private void OnMouseLeftButtonUp()
    {
        _isDragging = false;
    }

    private void OnMouseMove(MouseMoveVO vo)
    {
        if (_startPoint == null)
        {
            return;
        }

        if (vo.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        Vector diff = (Vector)(_startPoint - vo.AbsolutePosition);

        if ((_isDragging == false) && Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
            if (vo.OriginalSource != null)
            {
                TreeViewItem? treeViewItem = Util.FindAncestor<TreeViewItem>((DependencyObject)vo.OriginalSource);

                if (vo.Sender is not TreeView treeView || treeViewItem == null)
                {
                    return;
                }

                if (treeView.SelectedItem is not ProjectItem projectItem || projectItem.IsRoot)
                {
                    return;
                }

                DataObject dragData = new(projectItem);

                DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.Move);

                _isDragging = true;
            }
        }
    }

    private void OnInitializeAdorners(TreeViewItem control, DragEventArgs eventArgs)
    {
        if (_dragAdorner == null)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(control);

            Point startPosition = eventArgs.GetPosition(control);

            object data = eventArgs.Data.GetData(typeof(ProjectItem));

            _dragAdorner = new(data, ProjectItem.GetHeaderTemplate(), control, adornerLayer);

            _dragAdorner.UpdatePosition(startPosition.X, startPosition.Y);
        }

        if (_insertAdorner == null)
        {
            UIElement? itemContainer = Util.GetItemContainerFromPoint(control, eventArgs.GetPosition(control));

            if (itemContainer != null)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(control);
                bool isTopHalf = Util.IsPointInTopHalf(control, eventArgs);

                _insertAdorner = new(isTopHalf, itemContainer, adornerLayer);
            }
        }
    }

    private void OnUpdateAdorners(TreeViewItem control, DragEventArgs eventArgs)
    {
        if (_insertAdorner != null)
        {
            _insertAdorner.IsTopHalf = Util.IsPointInTopHalf(control, eventArgs);
            _insertAdorner.InvalidateVisual();
        }

        if (_dragAdorner != null)
        {
            Point currentPosition = eventArgs.GetPosition(control);

            _dragAdorner.UpdatePosition(currentPosition.X, currentPosition.Y);
        }
    }

    private void OnDetachAdorners()
    {
        _isDragging = false;

        if (_insertAdorner != null)
        {
            _insertAdorner.Destroy();
            _insertAdorner = null;
        }

        if (_dragAdorner != null)
        {
            _dragAdorner.Destroy();
            _dragAdorner = null;
        }
    }

    private void OnSizeChanged(SizeChangedEventArgs args, bool fullscreen)
    {
        bool changed = false;

        if (args.HeightChanged)
        {
            ModelManager.Get<GBAToolConfigurationModel>().WindowSizeY = (int)args.NewSize.Height;

            changed = true;
        }

        if (args.WidthChanged)
        {
            ModelManager.Get<GBAToolConfigurationModel>().WindowSizeX = (int)args.NewSize.Width;

            changed = true;
        }

        if (_isFullscreen != fullscreen)
        {
            _isFullscreen = fullscreen;

            ModelManager.Get<GBAToolConfigurationModel>().FullScreen = fullscreen;

            changed = true;
        }

        if (changed)
        {
            ModelManager.Get<GBAToolConfigurationModel>().Save();
        }
    }

    private void OnFindAndCreateElement(ProjectItem newElement)
    {
        if (ProjectItems == null)
        {
            return;
        }

        foreach (ProjectItem item in ProjectItems)
        {
            if (item.IsRoot == true && item.Type == newElement.Type)
            {
                newElement.Parent = item;

                item.Items.Add(newElement);

                SignalManager.Get<CreateNewElementSignal>().Dispatch(newElement);

                break;
            }
        }
    }

    private void OnTryCreatePaletteElement(string name, List<Color> colorArray, bool use256Colors)
    {
        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        string palette = (string)Application.Current.FindResource(_folderPalettesKey);

        string path = Path.Combine(projectModel.ProjectPath, palette);

        name = ProjectItemFileSystem.GetValidFileName(path, name, Util.GetExtensionByType(ProjectItemType.Palette));

        ProjectItem newElement = new()
        {
            DisplayName = name,
            IsFolder = false,
            IsRoot = false,
            Type = ProjectItemType.Palette
        };

        SignalManager.Get<RegisterHistoryActionSignal>().Dispatch(new CreateNewElementHistoryAction(newElement));

        SignalManager.Get<FindAndCreateElementSignal>().Dispatch(newElement);

        ProjectItemFileSystem.CreateFileElement(newElement, path, name);

        if (newElement.FileHandler?.FileModel is PaletteModel paletteModel)
        {
            int[] colorList = paletteModel.Colors;

            int colorIndex = 0;
            foreach (Color color in colorArray)
            {
                colorList[colorIndex] = PaletteUtils.ConvertColorToInt(color);
                colorIndex++;
            }

            paletteModel.Colors = colorList;

            newElement.FileHandler?.Save();

            SignalManager.Get<PaletteCreatedSuccessfullySignal>().Dispatch(paletteModel);
        }
    }

    private void OnDropElement(ProjectItem targetElement, ProjectItem draggedElement)
    {
        draggedElement.Parent?.Items.Remove(draggedElement);

        if (draggedElement.Parent != null)
        {
            SignalManager.Get<UpdateFolderSignal>().Dispatch(draggedElement.Parent);
        }

        if (targetElement.IsFolder)
        {
            draggedElement.Parent = targetElement;
            targetElement.Items.Add(draggedElement);

            SignalManager.Get<UpdateFolderSignal>().Dispatch(targetElement);
        }
        else
        {
            draggedElement.Parent = targetElement.Parent;
            targetElement.Parent?.Items.Add(draggedElement);

            if (targetElement.Parent != null)
            {
                SignalManager.Get<UpdateFolderSignal>().Dispatch(targetElement.Parent);
            }
        }

        draggedElement.IsSelected = true;

        OnPropertyChanged("ProjectItems");
    }
    #endregion
}
