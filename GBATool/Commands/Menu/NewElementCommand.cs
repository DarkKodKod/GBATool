using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Model;
using GBATool.Models;
using GBATool.Views;
using System.Windows;

namespace GBATool.Commands
{
    public class NewElementCommand : Command
    {
        public override bool CanExecute(object parameter)
        {
            ProjectModel model = ModelManager.Get<ProjectModel>();

            if (model != null && !string.IsNullOrEmpty(model.Name))
            {
                return true;
            }

            return false;
        }

        public override void Execute(object parameter)
        {
            Window? window = parameter as Window;

            ElementDialog dialog = new()
            {
                Owner = window
            };
            dialog.ShowDialog();
        }
    }
}
