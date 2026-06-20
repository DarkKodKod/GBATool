namespace ArchitectureLibrary.WPF.Controls;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public class ALScrollViewer : ScrollViewer
{
    public static readonly DependencyProperty DisableMouseWheelProperty =
        DependencyProperty.RegisterAttached(
            "DisableMouseWheel",
            typeof(bool),
            typeof(ALScrollViewer),
            new PropertyMetadata(default(bool)));

    public bool DisableMouseWheel
    {
        get => (bool)GetValue(DisableMouseWheelProperty);
        set => SetValue(DisableMouseWheelProperty, value);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (DisableMouseWheel)
        {
            return;
        }

        base.OnMouseWheel(e);
    }
}
