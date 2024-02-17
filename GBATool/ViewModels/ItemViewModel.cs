using ArchitectureLibrary.ViewModel;
using GBATool.Models;

namespace GBATool.ViewModels;

public class ItemViewModel : ViewModel
{
    public ProjectItem? ProjectItem { get; set; } = null;

    public T? GetModel<T>() where T : AFileModel
    {
        return ProjectItem?.FileHandler?.FileModel is T model ? model : null;
    }
}
