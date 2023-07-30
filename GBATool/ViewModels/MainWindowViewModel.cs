using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands;
using GBATool.Models;
using GBATool.Signals;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace GBATool.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        #region Commands
        public OpenProjectCommand OpenProjectCommand { get; } = new();
        //public CloseProjectCommand CloseProjectCommand { get; } = new();
        //public DispatchSignalCommand<ExitSuccessSignal> ExitCommand { get; } = new();
        //public NewProjectCommand NewProjectCommand { get; } = new();
        public NewElementCommand NewElementCommand { get; } = new();
        //public LoadConfigsCommand LoadConfigsCommand { get; } = new();
        public ShowAboutDialogCommand ShowAboutDialogCommand { get; } = new();
        //public OpenBuildProjectCommand OpenBuildProjectCommand { get; } = new();
        //public OpenProjectPropertiesCommand OpenProjectPropertiesCommand { get; } = new();
        //public ViewHelpCommand ViewHelpCommand { get; } = new();
        //public CopyElementCommand CopyElementCommand { get; } = new();
        //public PasteElementCommand PasteElementCommand { get; } = new();
        //public DuplicateElementCommand DuplicateElementCommand { get; } = new();
        //public DeleteElementCommand DeleteElementCommand { get; } = new();
        //public EnableRenameElementCommand EnableRenameElementCommand { get; } = new();
        //public CreateFolderCommand CreateFolderCommand { get; } = new();
        //public CreateElementFromMenuCommand CreateElementFromMenuCommand { get; } = new();
        //public PreviewMouseLeftButtonDownCommand PreviewMouseLeftButtonDownCommand { get; } = new();
        //public DispatchSignalCommand<MouseLeftButtonUpSignal> PreviewMouseLeftButtonUpCommand { get; } = new();
        //public PreviewMouseMoveCommand PreviewMouseMoveCommand { get; } = new();
        //public DragEnterCommand DragEnterCommand { get; } = new();
        //public DragOverCommand DragOverCommand { get; } = new();
        //public DragLeaveCommand DragLeaveCommand { get; } = new();
        //public DropCommand DropCommand { get; } = new();
        //public OpenImportImageDialogCommand OpenImportImageDialogCommand { get; } = new();
        //public WindowsGetFocusCommand WindowsGetFocusCommand { get; } = new();
        //public QueryContinueDragCommand QueryContinueDragCommand { get; } = new();
        //public LoadMappersCommand LoadMappersCommand { get; } = new();
        public UndoCommand UndoCommand { get; } = new();
        public RedoCommand RedoCommand { get; } = new();
        #endregion

        private List<RecentProjectModel> _recentProjects = new();
        private bool? _isFullscreen = null;

        #region get/set
        public List<RecentProjectModel> RecentProjects
        {
            get => _recentProjects;
            set
            {
                _recentProjects = value;
                OnPropertyChanged("RecentProjects");
            }
        }
        #endregion

        public MainWindowViewModel()
        {
            #region Signals
            SignalManager.Get<SizeChangedSignal>().Listener += OnSizeChanged;
            SignalManager.Get<LoadConfigSuccessSignal>().Listener += OnLoadConfigSuccess;
            #endregion
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

        private void OnLoadConfigSuccess()
        {
            GBAToolConfigurationModel model = ModelManager.Get<GBAToolConfigurationModel>();

            OnUpdateRecentProjects(model.RecentProjects);

            LoadDefaultProject();
        }

        private void OnUpdateRecentProjects(string[] recentProjects)
        {
            List<RecentProjectModel> list = new();

            int index = 1;

            foreach (string project in recentProjects)
            {
                if (!string.IsNullOrEmpty(project))
                {
                    // Extract the name of the folder as our project name
                    int startIndex = project.LastIndexOf(Path.DirectorySeparatorChar);
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
    }
}
