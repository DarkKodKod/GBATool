using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Building;
using GBATool.Enums;
using GBATool.Models;
using GBATool.Signals;
using System.IO;
using System.Threading.Tasks;

namespace GBATool.Commands;

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
            goto finish;
        }

        if (!CheckValidFolder(projectModel.Build.GeneratedAssetsPath))
        {
            OutputError("Invalid assets folder");
            goto finish;
        }

        OutputInfo("Build started");

        OutputInfo("Generate header...");
        bool ok = await Header.Generate(projectModel.Build.GeneratedSourcePath);
        if (ok == false)
        {
            OutputError("Problems generating header");
            OutputError(Header.GetErrors());
            goto finish;
        }

        OutputInfo("Building banks...");
        ok = await MemoryBanks.Generate(projectModel.Build.GeneratedAssetsPath);
        if (ok == false)
        {
            OutputError("Problems generating banks");
            OutputError(MemoryBanks.GetErrors());
            goto finish;
        }

        OutputInfo("Building tiles definitions...");
        ok = await TilesDefinitions.Generate(projectModel.Build.GeneratedAssetsPath);
        if (ok == false)
        {
            OutputError("Problems generating tiles definitions");
            OutputError(TilesDefinitions.GetErrors());
            goto finish;
        }

        OutputInfo("Building backgrounds...");
        ok = await Backgrounds.Generate(projectModel.Build.GeneratedAssetsPath);
        if (ok == false)
        {
            OutputError("Problems generating backgrounds");
            OutputError(Backgrounds.GetErrors());
            goto finish;
        }

        OutputInfo("Building meta sprites...");
        ok = await MetaSprites.Generate(projectModel.Build.GeneratedAssetsPath);
        if (ok == false)
        {
            OutputError("Problems generating meta sprites");
            OutputError(MetaSprites.GetErrors());
            goto finish;
        }

        OutputInfo("Building palettes...");
        ok = await Palettes.Generate(projectModel.Build.GeneratedAssetsPath);
        if (ok == false)
        {
            OutputError("Problems generating palettes");
            OutputError(Palettes.GetErrors());
            goto finish;
        }

        OutputInfo("Build completed", "Green");

        finish:
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
