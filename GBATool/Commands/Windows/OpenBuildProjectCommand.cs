﻿using ArchitectureLibrary.Commands;
using GBATool.FileSystem;
using GBATool.Views;
using System.Windows;

namespace GBATool.Commands
{
    public class OpenBuildProjectCommand : Command
    {
        public override bool CanExecute(object parameter)
        {
            if (ProjectFiles.ObjectsLoading > 0)
            {
                return false;
            }

            if (parameter == null)
            {
                return false;
            }

            object[] values = (object[])parameter;
            Window window = (Window)values[0];

            if (window == null)
            {
                return false;
            }

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

            BuildProjectDialog dialog = new()
            {
                Owner = window
            };

            dialog.ShowDialog();
        }
    }
}