using ArchitectureLibrary.Commands;
using GBATool.Views;
using System.Windows;

namespace GBATool.Commands
{
    public class OpenProjectPropertiesCommand : Command
    {
        public override bool CanExecute(object parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            object[] values = (object[])parameter;
            string projectName = (string)values[1];

            if (string.IsNullOrEmpty(projectName))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            object[] values = (object[])parameter;
            Window window = (Window)values[0];
            _ = (string)values[1];

            ProjectPropertiesDialog dialog = new()
            {
                Owner = window
            };
            dialog.ShowDialog();
        }
    }
}
