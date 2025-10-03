using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands.Utils;
using GBATool.Enums;
using GBATool.Models;
using GBATool.Signals;

namespace GBATool.ViewModels;

public class ProjectPropertiesDialogViewModel : ViewModel
{
    public DispatchSignalCommand<CloseDialogSignal> CloseDialogCommand { get; } = new();

    private bool _changed = false;
    private SpritePattern _selectedSpriteFormat = SpritePattern.Format1D;
    private OutputFormat _selectedOutputFormat = OutputFormat.Fasmarm;
    private string _projectTitle = string.Empty;
    private int _softwareVersion = 0;
    private string _projectInitials = string.Empty;
    private string _developerId = string.Empty;

    #region get/set
    public OutputFormat SelectedOutputFormat
    { 
        get { return _selectedOutputFormat; }
        set
        {
            _selectedOutputFormat = value;
            OnPropertyChanged(nameof(SelectedOutputFormat));

            _changed = true;
        }
    }

    public SpritePattern SelectedSpriteFormat
    {
        get { return _selectedSpriteFormat; }
        set
        {
            _selectedSpriteFormat = value;
            OnPropertyChanged(nameof(SelectedSpriteFormat));

            _changed = true;
        }
    }

    public string ProjectTitle
    {
        get { return _projectTitle; }
        set
        {
            _projectTitle = value;
            OnPropertyChanged(nameof(ProjectTitle));

            _changed = true;
        }
    }

    public int SoftwareVersion
    {
        get { return _softwareVersion; }
        set
        {
            _softwareVersion = value;
            OnPropertyChanged(nameof(SoftwareVersion));

            _changed = true;
        }
    }

    public string ProjectInitials
    {
        get { return _projectInitials; }
        set
        {
            _projectInitials = value;
            OnPropertyChanged(nameof(ProjectInitials));

            _changed = true;
        }
    }

    public string DeveloperId
    {
        get { return _developerId; }
        set
        {
            _developerId = value;
            OnPropertyChanged(nameof(DeveloperId));

            _changed = true;
        }
    }
    #endregion

    public ProjectPropertiesDialogViewModel()
    {
        ReadProjectData();

        SignalManager.Get<CloseDialogSignal>().Listener += OnCloseDialog;

        _changed = false;
    }

    private void OnCloseDialog()
    {
        if (_changed)
        {
            // Save all changes
            ProjectModel project = ModelManager.Get<ProjectModel>();

            if (project.SpritePatternFormat != SelectedSpriteFormat)
            {
                project.SpritePatternFormat = SelectedSpriteFormat;

                SignalManager.Get<ReloadBankImageSignal>().Dispatch();
            }

            project.OutputFormat = SelectedOutputFormat;
            project.ProjectTitle = ProjectTitle;
            project.SoftwareVersion = (byte)SoftwareVersion;
            project.ProjectInitials = ProjectInitials;
            project.DeveloperId = DeveloperId;

            project.Save();
        }

        SignalManager.Get<CloseDialogSignal>().Listener -= OnCloseDialog;
    }

    private void ReadProjectData()
    {
        ProjectModel project = ModelManager.Get<ProjectModel>();

        SelectedSpriteFormat = project.SpritePatternFormat;
        ProjectTitle = project.ProjectTitle;
        SoftwareVersion = project.SoftwareVersion;
        ProjectInitials = project.ProjectInitials;
        DeveloperId = project.DeveloperId;
        SelectedOutputFormat = project.OutputFormat;
    }
}
