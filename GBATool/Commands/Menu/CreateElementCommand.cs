using ArchitectureLibrary.Commands;
using ArchitectureLibrary.History.Signals;
using ArchitectureLibrary.Signals;
using GBATool.FileSystem;
using GBATool.HistoryActions;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;
using System.Windows;

namespace GBATool.Commands.Menu;

public class CreateElementCommand : Command
{
    private const string _fileNameKey = "NewElementName";

    private readonly string _newFileName;

    public CreateElementCommand()
    {
        _newFileName = (string)Application.Current.FindResource(_fileNameKey);
    }

    public override void Execute(object? parameter)
    {
        if (parameter == null)
        {
            return;
        }

        if (parameter is not ElementTypeModel element)
        {
            return;
        }

        string name = ProjectItemFileSystem.GetValidFileName(
            element.Path,
            _newFileName,
            Util.GetExtensionByType(element.Type));

        ProjectItem newElement = new()
        {
            DisplayName = name,
            IsFolder = false,
            IsRoot = false,
            Type = element.Type
        };

        SignalManager.Get<RegisterHistoryActionSignal>().Dispatch(new CreateNewElementHistoryAction(newElement));

        SignalManager.Get<FindAndCreateElementSignal>().Dispatch(newElement);

        ProjectItemFileSystem.CreateElement(newElement, element.Path, name);
    }
}
