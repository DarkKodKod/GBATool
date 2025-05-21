using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Model;
using GBATool.Models;

namespace GBATool.Commands.Project;

public class LoadConfigsCommand : Command
{
    public override void Execute(object? parameter)
    {
        ModelManager.Get<GBAToolConfigurationModel>().Load();
    }
}
