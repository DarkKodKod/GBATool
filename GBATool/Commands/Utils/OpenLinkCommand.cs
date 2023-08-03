using ArchitectureLibrary.Commands;
using System.Diagnostics;

namespace GBATool.Commands
{
    public class OpenLinkCommand : Command
    {
        public override void Execute(object parameter)
        {
            if (parameter is string url)
            {
                Process.Start(url);
            }
        }
    }
}
