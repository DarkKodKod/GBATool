using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Models;
using GBATool.Signals;
using System.IO;

namespace GBATool.Commands
{
    public class BuildProjectCommand : Command
    {
        private bool _building = false;

        public override bool CanExecute(object? parameter)
        {
            return !_building;
        }

        public override void Execute(object? parameter)
        {
            if (_building)
            {
                return;
            }

            _building = true;

            if (!CheckValidOutputFolder())
            {
                OutputError("Invalid output folder");

                _building = false;
                return;
            }

            OutputInfo("Build started");

            OutputInfo("Building banks...");
            OutputInfo("Building tiles definitions...");
            OutputInfo("Building backgrounds...");
            OutputInfo("Building meta sprites...");
            OutputInfo("Building palettes...");
            OutputInfo("Build completed", "Green");

            _building = false;

            RaiseCanExecuteChanged();
        }

        private static bool CheckValidOutputFolder()
        {
            ProjectModel projectModel = ModelManager.Get<ProjectModel>();

            try
            {
                string result = Path.GetFullPath(projectModel.Build.OutputFilePath);

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
