﻿using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GBATool.Commands.Menu;

public class DragOverCommand : Command
{
    private DateTime _startTime;
    private string _folderId = string.Empty;

    public override bool CanExecute(object? parameter)
    {
        if (parameter is not DragEventArgs dragEvent)
        {
            return false;
        }

        ProjectItem? draggingItem = dragEvent.Data.GetData(typeof(ProjectItem)) as ProjectItem;

        TreeViewItem? treeViewItem = Util.FindAncestor<TreeViewItem>((DependencyObject)dragEvent.OriginalSource);

        if (treeViewItem == null)
        {
            return false;
        }

        if (treeViewItem.DataContext is ProjectItem item && item.Type != draggingItem?.Type)
        {
            dragEvent.Handled = true;

            SignalManager.Get<DetachAdornersSignal>().Dispatch();

            dragEvent.Effects = DragDropEffects.None;

            return false;
        }

        return true;
    }

    public override void Execute(object? parameter)
    {
        if (parameter is not DragEventArgs dragEvent)
        {
            return;
        }

        TreeViewItem? treeViewItem = Util.FindAncestor<TreeViewItem>((DependencyObject)dragEvent.OriginalSource);

        if (treeViewItem != null)
        {
            if (treeViewItem.DataContext is ProjectItem element)
            {
                if (element.IsFolder && element.Items.Count > 0 && !treeViewItem.IsExpanded)
                {
                    if (element.FileHandler?.FileModel == null)
                    {
                        return;
                    }

                    if (_folderId != element.FileHandler.FileModel.GUID)
                    {
                        _folderId = element.FileHandler.FileModel.GUID;
                        _startTime = DateTime.UtcNow;
                    }
                    else
                    {
                        int milliseconds = Convert.ToInt32((DateTime.UtcNow - _startTime).TotalMilliseconds);

                        if (milliseconds >= 1000)
                        {
                            treeViewItem.IsExpanded = true;
                        }
                    }
                }
            }

            SignalManager.Get<UpdateAdornersSignal>().Dispatch(treeViewItem, dragEvent);
        }

        dragEvent.Handled = true;
    }
}
