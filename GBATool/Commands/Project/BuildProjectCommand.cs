using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Building;
using GBATool.Enums;
using GBATool.Models;
using GBATool.Signals;
using System.IO;
using System.Threading.Tasks;

namespace GBATool.Commands
{
    public class BuildProjectCommand : Command
    {
        private bool _building = false;

        public override bool CanExecute(object? parameter)
        {
            return !_building;
        }

        public override async Task ExecuteAsync(object? parameter)
        {
            if (_building)
            {
                return;
            }

            _building = true;

            ProjectModel projectModel = ModelManager.Get<ProjectModel>();

            if (!CheckValidFolder(projectModel.Build.GeneratedSourcePath))
            {
                OutputError("Invalid source folder");

                _building = false;
                return;
            }

            if (!CheckValidFolder(projectModel.Build.GeneratedAssetsPath))
            {
                OutputError("Invalid assets folder");

                _building = false;
                return;
            }

            OutputInfo("Build started");

            OutputInfo("Generate header...");
            bool ok = await Header.Generate(projectModel.Build.GeneratedSourcePath);
            if (ok == false)
            {
                OutputError("Problems generating header");
                _building = false;
                return;
            }

            OutputInfo("Building banks...");
            OutputInfo("Building tiles definitions...");
            OutputInfo("Building backgrounds...");
            OutputInfo("Building meta sprites...");
            OutputInfo("Building palettes...");
            OutputInfo("Build completed", "Green");

            _building = false;

            RaiseCanExecuteChanged();
        }

        private static bool CheckValidFolder(string path)
        {
            try
            {
                string result = Path.GetFullPath(path);

                return Directory.Exists(result);
            }
            catch
            {
                return false;
            }
        }

        private static void OutputInfo(string message, string color = "")
        {
            SignalManager.Get<WriteBuildOutputSignal>().Dispatch(message, OutputMessageType.Information, color);
        }

        private static void OutputError(string message)
        {
            SignalManager.Get<WriteBuildOutputSignal>().Dispatch(message, OutputMessageType.Error, "");
        }
    }
}
