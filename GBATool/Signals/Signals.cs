using ArchitectureLibrary.Signals;
using GBATool.ViewModels;
using GBATool.VOs;
using System.Windows;

namespace GBATool.Signals
{
    // Generics
    public class SetUpWindowPropertiesSignal : Signal<WindowVO> { }
    public class FinishedLoadingProjectSignal : Signal { }
    public class ProjectItemLoadedSignal : Signal<string> { }

    // MainWindowViewModel
    public class SizeChangedSignal : Signal<SizeChangedEventArgs, bool> { }
    public class LoadConfigSuccessSignal : Signal { }
    public class ProjectItemSelectedSignal : Signal<ProjectItem> { }
    public class ProjectItemUnselectedSignal : Signal<ProjectItem> { }

    // File system
    public class RegisterFileHandlerSignal : Signal<ProjectItem, string> { }
    public class RenameFileSignal : Signal<ProjectItem> { }

    // Edit
    public class PasteElementSignal : Signal<ProjectItem, ProjectItem> { }
    public class MoveElementSignal : Signal<ProjectItem, ProjectItem> { }
    public class DeleteElementSignal : Signal<ProjectItem> { }
    public class FindAndCreateElementSignal : Signal<ProjectItem> { }
    public class CreateNewElementSignal : Signal<ProjectItem> { }
}
