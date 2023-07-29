using ArchitectureLibrary.Signals;
using GBATool.VOs;
using System.Windows;

namespace GBATool.Signals
{
    // Generics
    public class SetUpWindowPropertiesSignal : Signal<WindowVO> { }

    // MainWindowViewModel
    public class SizeChangedSignal : Signal<SizeChangedEventArgs, bool> { }
    public class LoadConfigSuccessSignal : Signal { }
}
