using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Signals;
using GBATool.ViewModels;
using GBATool.VOs;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

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

        private void OnCreateNewElement(ProjectItem item)
        {
            /*TreeViewItem parentItem = (TreeViewItem)(tvProjectItems.ItemContainerGenerator.ContainerFromItem(item.Parent));

            if (parentItem != null)
            {
                parentItem.IsExpanded = true;
            }
            else
            {
                UpdateTreeLayout(item);
            }*/
        }
    }
}
