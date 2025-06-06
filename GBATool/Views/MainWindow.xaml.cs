﻿using ArchitectureLibrary.Signals;
using GBATool.Commands.Menu;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;
using GBATool.Views;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace GBATool
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct MonitorInfoEx
    {
        public int cbSize;
        public Rect rcMonitor;
        public Rect rcWork;
        public UInt32 dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string szDeviceName;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region DLL Imports
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [DllImport("User32")]
        public static extern IntPtr MonitorFromWindow(IntPtr hWnd, int dwFlags);

        [DllImport("user32", EntryPoint = "GetMonitorInfo", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool GetMonitorInfoEx(IntPtr hMonitor, ref MonitorInfoEx lpmi);
        #endregion

        private ProjectItemType _currentViewType = ProjectItemType.None;

        private LoadingDialog? _loadingDialog = null;
        private readonly FieldInfo? _menuDropAlignmentField;

        public MainWindow()
        {
            InitializeComponent();

            // Hack to correct the Menu display orientation
            _menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
            System.Diagnostics.Debug.Assert(_menuDropAlignmentField != null);

            EnsureStandardPopupAlignment();
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;

            SignalManager.Get<SetUpWindowPropertiesSignal>().Listener += OnSetUpWindowProperties;
            SignalManager.Get<CreateNewElementSignal>().Listener += OnCreateNewElement;
            SignalManager.Get<UpdateFolderSignal>().Listener += OnUpdateFolder;
            SignalManager.Get<LoadProjectItemSignal>().Listener += OnLoadProjectItem;
            SignalManager.Get<DeleteElementSignal>().Listener += OnDeleteElement;
            SignalManager.Get<CloseProjectSuccessSignal>().Listener += OnCloseProjectSuccess;
            SignalManager.Get<ShowLoadingDialogSignal>().Listener += OnShowLoadingDialog;
            SignalManager.Get<FinishedLoadingProjectSignal>().Listener += OnFinishedLoadingProject;
            SignalManager.Get<GotoProjectItemSignal>().Listener += OnGotoProjectItem;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Window window = GetWindow(this);
            WindowInteropHelper wih = new(window);
            IntPtr hWnd = wih.Handle;

            const int MONITOR_DEFAULTTOPRIMARY = 1;
            MonitorInfoEx mi = new();
            mi.cbSize = Marshal.SizeOf(mi);
            GetMonitorInfoEx(MonitorFromWindow(hWnd, MONITOR_DEFAULTTOPRIMARY), ref mi);

            GetWindowRect(hWnd, out Rect appBounds);

            double windowHeight = appBounds.Right - appBounds.Left;
            double windowWidth = appBounds.Bottom - appBounds.Top;

            double monitorHeight = mi.rcMonitor.Right - mi.rcMonitor.Left;
            double monitorWidth = mi.rcMonitor.Bottom - mi.rcMonitor.Top;

            bool fullScreen = !((windowHeight == monitorHeight) && (windowWidth == monitorWidth));

            SignalManager.Get<SizeChangedSignal>().Dispatch(e, fullScreen);
        }

        private void OnSetUpWindowProperties(WindowVO vo)
        {
            WindowState = vo.IsFullScreen ? WindowState.Maximized : WindowState.Normal;

            Height = vo.SizeY;
            Width = vo.SizeX;

            CenterWindowOnScreen();
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = Width;
            double windowHeight = Height;

            Left = (screenWidth / 2) - (windowWidth / 2);
            Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void SystemParameters_StaticPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            EnsureStandardPopupAlignment();
        }

        private void EnsureStandardPopupAlignment()
        {
            if (SystemParameters.MenuDropAlignment && _menuDropAlignmentField != null)
            {
                _menuDropAlignmentField.SetValue(null, false);
            }
        }

        private void OnLoadProjectItem(ProjectItem item)
        {
            if (item.IsRoot || item.IsFolder)
            {
                return;
            }

            if (dpItemPanel.Children.Count > 0)
            {
                if (dpItemPanel.Children[0] is UserControl oldGui)
                {
                    if (oldGui.DataContext is ItemViewModel oldModel)
                    {
                        oldModel.OnDeactivate();
                    }
                }

                if (dpItemPanel.Children[0] is ICleanable cloneable)
                {
                    cloneable.CleanUp();
                }

                dpItemPanel.Children.Clear();
            }

            UserControl? view = null;

            switch (item.Type)
            {
                case ProjectItemType.Bank: view = new Banks(); break;
                case ProjectItemType.Character: view = new Character(); break;
                case ProjectItemType.Map: view = new Map(); break;
                case ProjectItemType.TileSet: view = new TileSet(); break;
                case ProjectItemType.Palette: view = new Palette(); break;
                case ProjectItemType.World: view = new World(); break;
                case ProjectItemType.Entity: view = new Entity(); break;
            }

            if (view != null)
            {
                dpItemPanel.Children.Add(view);

                if (view.DataContext is ItemViewModel viewModel)
                {
                    viewModel.ProjectItem = item;

                    viewModel.OnActivate();
                }
            }

            _currentViewType = item.Type;

            dpItemPanel.UpdateLayout();
        }

        private void OnDeleteElement(ProjectItem item)
        {
            if (_currentViewType == item.Type)
            {
                if (dpItemPanel.Children.Count > 0)
                {
                    if (dpItemPanel.Children[0] is UserControl oldGui)
                    {
                        if (oldGui.DataContext is ItemViewModel oldModel)
                        {
                            oldModel.OnDeactivate();
                        }
                    }

                    dpItemPanel.Children.Clear();
                }

                _currentViewType = ProjectItemType.None;

                dpItemPanel.UpdateLayout();
            }
        }

        private void OnShowLoadingDialog()
        {
            _loadingDialog ??= new()
            {
                Owner = this
            };
            _loadingDialog.Show();
        }

        private void OnFinishedLoadingProject()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _loadingDialog?.Close();
            }));
        }

        private void OnCloseProjectSuccess()
        {
            dpItemPanel.Children.Clear();

            _currentViewType = ProjectItemType.None;

            dpItemPanel.UpdateLayout();
        }

        private void OnGotoProjectItem(string elementID)
        {
            ProjectItem? FindTviFromObjectRecursive(ItemsControl ic)
            {
                ProjectItem? projectItem = null;

                foreach (ProjectItem pi in ic.Items)
                {
                    if (pi.IsRoot || pi.IsFolder)
                    {
                        TreeViewItem? tvi = (TreeViewItem?)ic.ItemContainerGenerator.ContainerFromItem(pi);

                        if (tvi != null)
                        {
                            if (tvi.IsExpanded == false)
                            {
                                tvi.ExpandSubtree();
                            }

                            projectItem = FindTviFromObjectRecursive(tvi);
                        }
                    }
                    else
                    {
                        projectItem = pi;
                    }

                    if (projectItem?.FileHandler?.FileModel?.GUID == elementID)
                    {
                        return projectItem;
                    }
                }

                return null;
            }

            if (FindTviFromObjectRecursive(tvProjectItems) is ProjectItem projectItem)
            {
                projectItem.IsSelected = true;
            }
        }

        static TreeViewItem? VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && source is not TreeViewItem)
            {
                source = VisualTreeHelper.GetParent(source);
            }

            return source as TreeViewItem;
        }

        private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem? treeViewItem = VisualUpwardSearch((DependencyObject)e.OriginalSource);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private void MainWindowView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            WindowInteropHelper wih = new(window);
            IntPtr hWnd = wih.Handle;

            const int MONITOR_DEFAULTTOPRIMARY = 1;
            MonitorInfoEx mi = new();
            mi.cbSize = Marshal.SizeOf(mi);
            GetMonitorInfoEx(MonitorFromWindow(hWnd, MONITOR_DEFAULTTOPRIMARY), ref mi);

            GetWindowRect(hWnd, out Rect appBounds);

            double windowHeight = appBounds.Right - appBounds.Left;
            double windowWidth = appBounds.Bottom - appBounds.Top;

            double monitorHeight = mi.rcMonitor.Right - mi.rcMonitor.Left;
            double monitorWidth = mi.rcMonitor.Bottom - mi.rcMonitor.Top;

            bool fullScreen = !((windowHeight == monitorHeight) && (windowWidth == monitorWidth));

            SignalManager.Get<SizeChangedSignal>().Dispatch(e, fullScreen);
        }

        private void OnUpdateFolder(ProjectItem item)
        {
            TreeViewItem? treeItem = (TreeViewItem)(tvProjectItems.ItemContainerGenerator.ContainerFromItem(item));

            if (treeItem != null)
            {
                if (item.Items.Count == 0)
                {
                    treeItem.IsExpanded = false;
                }
            }
            else
            {
                UpdateTreeLayout(item);
            }
        }

        private void OnCreateNewElement(ProjectItem item)
        {
            TreeViewItem? parentItem = (TreeViewItem)(tvProjectItems.ItemContainerGenerator.ContainerFromItem(item.Parent));

            if (parentItem != null)
            {
                parentItem.IsExpanded = true;
            }
            else
            {
                UpdateTreeLayout(item);
            }
        }

        private void UpdateTreeLayout(ProjectItem item)
        {
            IEnumerable<ProjectItem> nodes = (IEnumerable<ProjectItem>)tvProjectItems.ItemsSource;
            if (nodes == null)
            {
                return;
            }

            Stack<ProjectItem> queue = new();
            queue.Push(item);

            ProjectItem? parent = item.Parent;

            while (parent != null)
            {
                queue.Push(parent);
                parent = parent.Parent;
            }

            ItemContainerGenerator generator = tvProjectItems.ItemContainerGenerator;

            while (queue.Count > 0)
            {
                ProjectItem dequeue = queue.Pop();

                tvProjectItems.UpdateLayout();

                TreeViewItem? treeViewItem = (TreeViewItem)generator.ContainerFromItem(dequeue);

                if (treeViewItem != null)
                {
                    bool areThereMoreElement = queue.Count > 0 || item.Items.Count > 0;

                    treeViewItem.IsExpanded = areThereMoreElement;

                    generator = treeViewItem.ItemContainerGenerator;
                }
            }
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            StackPanel stackPanel = (StackPanel)sender;

            if (stackPanel.DataContext is ProjectItem item && !item.IsLoaded)
            {
                item.IsLoaded = true;
                item.IsSelected = true;

                using EnableRenameElementCommand command = new();
                command.Execute(item);
            }
        }

        private void EditableTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TextBox tb && tb.IsVisible)
            {
                tb.Focus();
                tb.SelectAll();

                if (tb.DataContext is ProjectItem item)
                {
                    // back up - for possible cancelling
                    item.OldCaptionValue = tb.Text;
                }
            }
        }

        private void EditableTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.DataContext is ProjectItem item)
            {
                item.IsInEditMode = false;
            }
        }

        private void EditableTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox? tb = sender as TextBox;

            if (tb?.DataContext is ProjectItem item)
            {
                if (e.Key == Key.Enter)
                {
                    item.IsInEditMode = false;

                    string name = tb.Text;

                    if (name != item.DisplayName && item.FileHandler != null)
                    {
                        // now check if the name is not already taken
                        if (item.IsFolder)
                        {
                            name = ProjectItemFileSystem.GetValidFolderName(item.FileHandler.Path, tb.Text);
                        }
                        else
                        {
                            string extension = Util.GetExtensionByType(item.Type);

                            name = ProjectItemFileSystem.GetValidFileName(item.FileHandler.Path, tb.Text, extension);
                        }

                        tb.Text = name;
                    }
                }

                if (e.Key == Key.Escape)
                {
                    tb.Text = item.OldCaptionValue;

                    item.IsInEditMode = false;
                }
            }
        }

        private void EditableTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox? tb = sender as TextBox;

            string? previousText = tb?.Text;

            if (tb?.SelectedText.Length > 0)
            {
                previousText = tb?.Text.Replace(tb.SelectedText, string.Empty);
            }

            e.Handled = !Util.ValidFileName(previousText + e.Text);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }
    }
}
