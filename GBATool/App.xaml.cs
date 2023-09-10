using ArchitectureLibrary.History;
using GBATool.FileSystem;
using System.Windows;

namespace GBATool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ProjectItemFileSystem.Initialize();
            HistoryManager.Initialize();
        }
    }
}
