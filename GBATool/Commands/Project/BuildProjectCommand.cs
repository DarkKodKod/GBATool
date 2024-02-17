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
        bool ok = await Header.Instance.Generate(projectModel.Build.GeneratedSourcePath);
        if (ok == false)
        {
            OutputError("Problems generating header");
            OutputError(Header.Instance.GetErrors());
            goto finish;
        }

        OutputInfo("Building banks...");
        ok = await MemoryBanks.Instance.Generate(projectModel.Build.GeneratedAssetsPath);
        if (ok == false)
        {
            OutputError("Problems generating banks");
            OutputError(MemoryBanks.Instance.GetErrors());
            goto finish;
        }

        OutputInfo("Building tiles definitions...");
        ok = await TilesDefinitions.Instance.Generate(projectModel.Build.GeneratedAssetsPath);
        if (ok == false)
        {
            OutputError("Problems generating tiles definitions");
            OutputError(TilesDefinitions.Instance.GetErrors());
            goto finish;
        }

        OutputInfo("Building backgrounds...");
        ok = await Backgrounds.Instance.Generate(projectModel.Build.GeneratedAssetsPath);
        if (ok == false)
        {
            OutputError("Problems generating backgrounds");
            OutputError(Backgrounds.Instance.GetErrors());
            goto finish;
        }

        OutputInfo("Building meta sprites...");
        ok = await MetaSprites.Instance.Generate(projectModel.Build.GeneratedAssetsPath);
        if (ok == false)
        {
            OutputError("Problems generating meta sprites");
            OutputError(MetaSprites.Instance.GetErrors());
            goto finish;
        }

        OutputInfo("Building palettes...");
        ok = await Palettes.Instance.Generate(projectModel.Build.GeneratedAssetsPath);
        if (ok == false)
        {
            OutputError("Problems generating palettes");
            OutputError(Palettes.Instance.GetErrors());
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

    private static void OutputError(string[] messages)
    {
        for (int i = 0; i < messages.Length; i++)
        {
            SignalManager.Get<WriteBuildOutputSignal>().Dispatch(messages[i], OutputMessageType.Error, "");
        }
    }
}
