using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Building;
using GBATool.Enums;
using GBATool.Models;
using GBATool.Signals;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GBATool.Commands.Project;

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

        OutputInfo("Build started");

        IBuilding header = BuildHeader.Get(projectModel.Build.OutputFormatHeader);

        if (header.GetFormat() != OutputFormat.None)
        {
            OutputInfo("Generate header...");
        }

        bool ok = await header.Generate();
        if (!ok)
        {
            OutputError("Problems generating header");
            OutputError(header.GetErrors());
            goto finish;
        }
        OutputWarnings(header.GetWarnings());

        IBuilding banks = BuildMemoryBanks.Get(projectModel.Build.OutputFormatScreenBlock);

        if (banks.GetFormat() != OutputFormat.None)
        {
            OutputInfo("Building banks...");
        }

        ok = await banks.Generate();
        if (!ok)
        {
            OutputError("Problems generating banks");
            OutputError(banks.GetErrors());
            goto finish;
        }
        OutputWarnings(banks.GetWarnings());

        IBuilding metaSprites = BuildMetaSprites.Get(projectModel.Build.OutputFormatCharacters);

        if (metaSprites.GetFormat() != OutputFormat.None)
        {
            OutputInfo("Building meta sprites...");
        }

        ok = await metaSprites.Generate();
        if (!ok)
        {
            OutputError("Problems generating meta sprites");
            OutputError(metaSprites.GetErrors());
            goto finish;
        }
        OutputWarnings(metaSprites.GetWarnings());

        IBuilding palettes = BuildPalettes.Get(projectModel.Build.OutputFormatPalettes);

        if (palettes.GetFormat() != OutputFormat.None)
        {
            OutputInfo("Building palettes...");
        }

        ok = await palettes.Generate();
        if (!ok)
        {
            OutputError("Problems generating palettes");
            OutputError(palettes.GetErrors());
            goto finish;
        }
        OutputWarnings(palettes.GetWarnings());

        IBuilding backgrounds = BuildPalettes.Get(projectModel.Build.OutputFormatBackground);

        if (backgrounds.GetFormat() != OutputFormat.None)
        {
            OutputInfo("Building backgrounds...");
        }

        ok = await backgrounds.Generate();
        if (!ok)
        {
            OutputError("Problems generating backgrounds");
            OutputError(backgrounds.GetErrors());
            goto finish;
        }
        OutputWarnings(backgrounds.GetWarnings());

        OutputInfo("Build completed", "Green");

        finish:
        _building = false;

        RaiseCanExecuteChanged();

        SignalManager.Get<ProjectBuildCompleteSignal>().Dispatch();
    }

    private static void OutputInfo(string message, string color = "")
    {
        SignalManager.Get<WriteBuildOutputSignal>().Dispatch(message, OutputMessageType.Information, color);
    }

    private static void OutputError(string message)
    {
        SignalManager.Get<WriteBuildOutputSignal>().Dispatch(message, OutputMessageType.Error, "");
    }

    private static void OutputWarning(string message)
    {
        SignalManager.Get<WriteBuildOutputSignal>().Dispatch(message, OutputMessageType.Warning, "");
    }

    private static void OutputError(string[] messages)
    {
        Dictionary<int, (string, int)> filteredMessages = [];

        for (int i = 0; i < messages.Length; i++)
        {
            string message = messages[i];

            int hash = message.GetHashCode();

            // filtering the messages when there are too many with the same message
            if (!filteredMessages.TryGetValue(hash, out (string, int) obj))
            {
                filteredMessages.Add(hash, (message, 0));
            }
            else
            {
                obj.Item2++;
                filteredMessages[hash] = obj;
            }
        }

        foreach (var item in filteredMessages)
        {
            StringBuilder sb = new();
            _ = sb.Append(item.Value.Item1);

            if (item.Value.Item2 > 0)
            {
                _ = sb.Append($" ({item.Value.Item2} times)");
            }

            OutputError(sb.ToString());
        }
    }

    private static void OutputWarnings(string[] messages)
    {
        Dictionary<int, (string, int)> filteredMessages = [];

        for (int i = 0; i < messages.Length; i++)
        {
            string message = messages[i];

            if (string.IsNullOrEmpty(message))
            {
                continue;
            }

            int hash = message.GetHashCode();

            // filtering the messages when there are too many with the same message
            if (!filteredMessages.TryGetValue(hash, out (string, int) obj))
            {
                filteredMessages.Add(hash, (message, 0));
            }
            else
            {
                obj.Item2++;
                filteredMessages[hash] = obj;
            }
        }

        foreach (var item in filteredMessages)
        {
            StringBuilder sb = new();
            _ = sb.Append(item.Value.Item1);

            if (item.Value.Item2 > 0)
            {
                _ = sb.Append($" ({item.Value.Item2} times)");
            }

            OutputWarning(sb.ToString());
        }
    }
}
