﻿using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.ViewModels;
using GBATool.VOs;
using System.Windows;
using System.Windows.Controls;

namespace GBATool.Signals
{
    // Generics
    public class BrowseFolderSuccessSignal : Signal<string> { }
    public class BrowseFileSuccessSignal : Signal<string, bool> { }
    public class SetUpWindowPropertiesSignal : Signal<WindowVO> { }
    public class FinishedLoadingProjectSignal : Signal { }
    public class ProjectItemLoadedSignal : Signal<string> { }
    public class ProjectConfigurationSavedSignal : Signal { }
    public class ShowLoadingDialogSignal : Signal { }
    public class CloseDialogSignal : Signal { }
    public class WriteBuildOutputSignal : Signal<string, OutputMessageType, string> { }
    public class MouseWheelSignal : Signal<MouseWheelVO> { }

    // MainWindowViewModel
    public class SizeChangedSignal : Signal<SizeChangedEventArgs, bool> { }
    public class LoadConfigSuccessSignal : Signal { }
    public class ExitSuccessSignal : Signal { }
    public class UpdateAdornersSignal : Signal<TreeViewItem, DragEventArgs> { }
    public class MouseLeftButtonDownSignal : Signal<Point> { }
    public class MouseLeftButtonUpSignal : Signal { }
    public class MouseMoveSignal : Signal<MouseMoveVO> { }
    public class ProjectItemSelectedSignal : Signal<ProjectItem> { }
    public class ProjectItemUnselectedSignal : Signal<ProjectItem> { }
    public class CloseProjectSuccessSignal : Signal { }
    public class OpenProjectSuccessSignal : Signal<ProjectOpenVO> { }
    public class UpdateRecentProjectsSignal : Signal<string[]> { }
    public class DropElementSignal : Signal<ProjectItem, ProjectItem> { }
    public class LoadProjectItemSignal : Signal<ProjectItem> { }
    public class UpdateFolderSignal : Signal<ProjectItem> { }
    public class CreateProjectSuccessSignal : Signal<string> { }
    public class InitializeAdornersSignal : Signal<TreeViewItem, DragEventArgs> { }
    public class DetachAdornersSignal : Signal { }

    // File system
    public class RegisterFileHandlerSignal : Signal<ProjectItem, string?> { }
    public class RenameFileSignal : Signal<ProjectItem> { }

    // Edit
    public class PasteElementSignal : Signal<ProjectItem, ProjectItem> { }
    public class MoveElementSignal : Signal<ProjectItem, ProjectItem> { }
    public class DeleteElementSignal : Signal<ProjectItem> { }
    public class FindAndCreateElementSignal : Signal<ProjectItem> { }
    public class CreateNewElementSignal : Signal<ProjectItem> { }

    // TileSet
    public class UpdateTileSetImageSignal : Signal { }
}