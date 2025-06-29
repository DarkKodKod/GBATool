﻿using ArchitectureLibrary.Clipboard;
using ArchitectureLibrary.History.Signals;
using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Enums;
using GBATool.HistoryActions;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.Utils.CustomTypeConverters;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GBATool.ViewModels;

[TypeConverter(typeof(ProjectItemTypeConverter))]
public class ProjectItem : ViewModel, IClipboardable
{
    public ProjectItem()
    { }

    public ProjectItem(string? content)
    {
        if (content == null)
        {
            return;
        }

        Task task = ParseContentAsync(content);
        task.Wait();

    }
    public async Task ParseContentAsync(string content)
    {
        ValueTask<ProjectItem?> task = ParseAndCreateObject(content);

        ProjectItem? item = await task.ConfigureAwait(false);

        if (item != null)
        {
            Copy(item);
        }
    }

    public ProjectItemType Type { get; set; }
    public bool IsRoot { get; set; } = false;
    public bool IsFolder { get; set; } = false;
    public ProjectItem? Parent = null;
    public ObservableCollection<ProjectItem> Items { get; set; } = [];
    public string OldCaptionValue { get; set; } = "";
    public FileHandler? FileHandler { get; set; }
    public bool RenamedFromAction { private get; set; } = false;

    public string GetContent()
    {
        StringBuilder sb = new();
        sb.Append("Type:" + Type.ToString());
        sb.Append(';');
        sb.Append("DisplayName:" + DisplayName);
        sb.Append(';');
        sb.Append("IsRoot:" + (IsRoot ? "true" : "false"));
        sb.Append(';');
        sb.Append("IsFolder:" + (IsFolder ? "true" : "false"));
        sb.Append(';');
        sb.Append("ModelFilePath:");
        sb.Append(IsFolder ? "null" : FileHandler?.Path + Path.DirectorySeparatorChar + FileHandler?.Name + FileHandler?.FileModel?.FileExtension);
        sb.Append(';');

        sb.Append("Items:");

        if (Items.Count == 0)
        {
            sb.Append("null");
            sb.Append(';');
        }
        else
        {
            sb.Append(Items.Count);
            sb.Append(';');
            foreach (ProjectItem item in Items)
            {
                sb.Append('{');
                sb.Append(item.GetContent());
                sb.Append('}');
            }
        }

        return sb.ToString();
    }

    private bool _isSelected;
    private object? _selectedItem = null;
    private bool _isInEditMode;
    private string _displayName = "";

    public bool IsLoaded { get; set; } = false;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (value != _isSelected)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));

                if (_isSelected)
                {
                    SelectedItem = this;
                }
                else
                {
                    _selectedItem = null;
                    OnUnSelectItemChanged();
                }
            }
        }
    }

    public string DisplayName
    {
        get => _displayName;
        set
        {
            if (_displayName != value)
            {
                bool changedName = !string.IsNullOrEmpty(_displayName);

                string oldName = _displayName;

                _displayName = value;

                OnPropertyChanged(nameof(DisplayName));

                if (changedName)
                {
                    if (!RenamedFromAction)
                    {
                        SignalManager.Get<RegisterHistoryActionSignal>().Dispatch(new RenameProjectItemHistoryAction(this, oldName));
                    }

                    SignalManager.Get<RenameFileSignal>().Dispatch(this);
                }
            }

            RenamedFromAction = false;
        }
    }

    public bool IsInEditMode
    {
        get => _isInEditMode;
        set
        {
            if (_isInEditMode != value)
            {
                _isInEditMode = value;

                OnPropertyChanged(nameof(IsInEditMode));
            }
        }
    }

    public object? SelectedItem
    {
        get => _selectedItem;
        private set
        {
            if (_selectedItem != value)
            {
                _selectedItem = value;
                OnSelectedItemChanged();
            }
        }
    }

    private async ValueTask<ProjectItem?> ParseAndCreateObject(string content)
    {
        ProjectItem item = new()
        {
            IsLoaded = true
        };

        int index = 0;
        while (index < content.Length)
        {
            int found = content.IndexOf(":", index);
            if (found == -1)
            {
                break;
            }

            string component = content[index..found];

            index = found + 1;

            found = content.IndexOf(";", index);

            string value = content[index..found];

            switch (component)
            {
                case "Type":
                    item.Type = (ProjectItemType)Enum.Parse(typeof(ProjectItemType), value);
                    break;
                case "DisplayName":
                    item.DisplayName = value;
                    break;
                case "IsRoot":
                    item.IsRoot = value == "true";
                    break;
                case "IsFolder":
                    item.IsFolder = value == "true";
                    break;
                case "ModelFilePath":
                    {
                        if (value != "null")
                        {
                            string? name = Path.GetFileNameWithoutExtension(value);
                            string? path = Path.GetDirectoryName(value);

                            if (path != null && name != null)
                            {
                                item.FileHandler = new()
                                {
                                    Name = name,
                                    Path = path,
                                    FileModel = await FileUtils.ReadFileAndLoadModelAsync(value, item.Type).ConfigureAwait(false)
                                };
                            }
                        }
                    }
                    break;
                case "Items":
                    Items = [];

                    if (value != "null")
                    {
                        found = content.IndexOf(";", index);

                        string braketsContent = content.Substring(found + 1, content.Length - found - 1);

                        int countOpenBrakets = 0;
                        int countObjectsCreated = 0;

                        using (StringReader reader = new(braketsContent))
                        {
                            StringBuilder sb = new();

                            int intChar;
                            while ((intChar = reader.Read()) != -1)
                            {
                                bool includeChar = true;
                                char chr = Convert.ToChar(intChar);

                                if (chr == '{')
                                {
                                    if (countOpenBrakets == 0)
                                    {
                                        includeChar = false;
                                    }

                                    countOpenBrakets++;
                                }

                                if (chr == '}')
                                {
                                    countOpenBrakets--;

                                    if (countOpenBrakets == 0)
                                    {
                                        includeChar = false;
                                    }

                                    // last braket?
                                    if (countOpenBrakets == 0)
                                    {
                                        ProjectItem? itm = await ParseAndCreateObject(sb.ToString()).ConfigureAwait(false);

                                        if (itm != null)
                                        {
                                            itm.Parent = item;
                                            item.Items.Add(itm);
                                        }

                                        sb.Clear();

                                        countObjectsCreated++;
                                    }
                                }

                                if (includeChar)
                                {
                                    sb.Append(chr);
                                }
                            }
                        }

                        if (countObjectsCreated != Convert.ToInt32(value))
                        {
                            return null;
                        }

                        found = content.Length;
                    }
                    break;
            }

            index = found + 1;
        }

        return item;
    }

    public static DataTemplate GetHeaderTemplate()
    {
        //create the data template
        DataTemplate dataTemplate = new();

        //create stack pane;
        FrameworkElementFactory stackPanel = new(typeof(StackPanel));
        stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        stackPanel.SetValue(StackPanel.BackgroundProperty, new SolidColorBrush(Colors.Black));

        // create text
        FrameworkElementFactory label = new(typeof(TextBlock));
        label.SetBinding(TextBlock.TextProperty, new Binding() { Path = new PropertyPath("DisplayName") });
        label.SetValue(TextBlock.MarginProperty, new Thickness(2));
        label.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        label.SetValue(TextBlock.ToolTipProperty, new Binding());
        label.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Colors.White));
        stackPanel.AppendChild(label);

        //set the visual tree of the data template
        dataTemplate.VisualTree = stackPanel;

        return dataTemplate;
    }

    private void Copy(ProjectItem item)
    {
        Type = item.Type;
        DisplayName = item.DisplayName;
        IsRoot = item.IsRoot;
        IsFolder = item.IsFolder;
        Parent = item.Parent;
        Items = new ObservableCollection<ProjectItem>(item.Items);
        FileHandler = item.FileHandler;
    }

    private void OnUnSelectItemChanged() => SignalManager.Get<ProjectItemUnselectedSignal>().Dispatch(this);
    private void OnSelectedItemChanged() => SignalManager.Get<ProjectItemSelectedSignal>().Dispatch(this);
}
