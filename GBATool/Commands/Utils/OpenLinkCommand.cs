using ArchitectureLibrary.Commands;
using System;
using System.Diagnostics;

namespace GBATool.Commands
{
    public class OpenLinkCommand : Command
    {
        public override void Execute(object? parameter)
        {
            if (parameter is string url)
            {
                Process process = new();

                try
                {
                    // true is the default, but it is important not to set it to false
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = url;
                    process.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
