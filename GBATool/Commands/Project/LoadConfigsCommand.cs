using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Model;
using GBATool.Models;

namespace GBATool.Commands
{
    public class LoadConfigsCommand : Command
    {
        public override void Execute(object parameter)
        {
            ModelManager.Get<GBAToolConfigurationModel>().Load();
        }
    }
}
