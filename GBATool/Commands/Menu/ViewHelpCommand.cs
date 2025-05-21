using ArchitectureLibrary.Commands;
using GBATool.Commands.Utils;

namespace GBATool.Commands.Menu;

public class ViewHelpCommand : Command
{
    public override void Execute(object? parameter)
    {
        using OpenLinkCommand openLink = new();
        openLink.Execute("https://github.com/DarkKodKod/GBATool");
    }
}
