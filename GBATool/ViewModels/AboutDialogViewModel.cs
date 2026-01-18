using ArchitectureLibrary.ViewModel;
using GBATool.Commands.Utils;
using GBATool.Utils;
using System.Windows;

namespace GBATool.ViewModels;

public class AboutDialogViewModel : ViewModel
{
    private const string _projectNameKey = "applicationTitle";
    private string _appTitle = string.Empty;
    private string _version = string.Empty;
    private string _modalTitle = string.Empty;

    #region get/set
    public string AppTitle
    {
        get => _appTitle;
        set
        {
            _appTitle = value;
            OnPropertyChanged(nameof(AppTitle));
        }
    }

    public string ModalTitle
    {
        get => _modalTitle;
        set
        {
            _modalTitle = value;
            OnPropertyChanged(nameof(ModalTitle));
        }
    }

    public string Version
    {
        get => _version;
        set
        {
            _version = value;
            OnPropertyChanged(nameof(Version));
        }
    }
    #endregion

    public OpenLinkCommand OpenLinkCommand { get; } = new();

    public AboutDialogViewModel()
    {
        AppTitle = (string)Application.Current.FindResource(_projectNameKey);
        ModalTitle = "About " + AppTitle;
        Version = Util.GetRunningVersion()?.ToString() ?? string.Empty;
    }
}
