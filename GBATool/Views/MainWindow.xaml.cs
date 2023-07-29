using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.VOs;
using System;
using System.Runtime.InteropServices;
using System.Windows;
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
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [DllImport("User32")]
        public static extern IntPtr MonitorFromWindow(IntPtr hWnd, int dwFlags);

        [DllImport("user32", EntryPoint = "GetMonitorInfo", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool GetMonitorInfoEx(IntPtr hMonitor, ref MonitorInfoEx lpmi);

        public MainWindow()
        {
            InitializeComponent();

            SignalManager.Get<SetUpWindowPropertiesSignal>().Listener += OnSetUpWindowProperties;
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
    }
}
