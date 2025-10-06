using ArchitectureLibrary.History.Signals;
using ArchitectureLibrary.Signals;
using GBATool.FileSystem;
using GBATool.HistoryActions;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;
using System.IO;
using System.Windows;

namespace GBATool.Commands.Menu;

public class CreateElementFromMenuCommand : ItemSelectedCommand
{
    private const string _fileNameKey = "NewElementName";

    private readonly string _newFileName;

    public CreateElementFromMenuCommand()
    {
        _newFileName = (string)Application.Current.FindResource(_fileNameKey);
    }

    public override bool CanExecute(object? parameter)
    {
        if (ItemSelected != null)
        {
            if (ItemSelected.IsFolder)
            {
                return true;
            }
        }

        return false;
    }

    public override void Execute(object? parameter)
    {
        if (ItemSelected == null || ItemSelected.FileHandler == null)
        {
            return;
        }

        string path = Path.Combine(ItemSelected.FileHandler.Path, ItemSelected.FileHandler.Name);

        string name = ProjectItemFileSystem.GetValidFileName(
            path,
            _newFileName,
            Util.GetExtensionByType(ItemSelected.Type));

        ProjectItem newElement = new()
        {
            DisplayName = name,
            IsFolder = false,
            Parent = ItemSelected,
            IsRoot = false,
            Type = ItemSelected.Type
        };

        ItemSelected.Items.Add(newElement);

        SignalManager.Get<RegisterHistoryActionSignal>().Dispatch(new CreateNewElementHistoryAction(newElement));

        SignalManager.Get<CreateNewElementSignal>().Dispatch(newElement);

        ProjectItemFileSystem.CreateElement(newElement, path, name);
    }
}
