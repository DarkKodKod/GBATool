using ArchitectureLibrary.Commands;

namespace GBATool.Commands
{
    public class ViewHelpCommand : Command
    {
        public override void Execute(object? parameter)
        {
            using OpenLinkCommand openLink = new();
            openLink.Execute("https://github.com/DarkKodKod/GBATool");
        }
    }
}
