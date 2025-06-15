using GBATool.Commands.Utils;
using System.Windows.Controls;

namespace GBATool.Views;

/// <summary>
/// Interaction logic for FrameView.xaml
/// </summary>
public partial class FrameView : UserControl
{
    #region Commands
    public ImageMouseDownCommand ImageMouseDownCommand { get; } = new();
    #endregion

    #region get/set
    public Canvas Canvas { get => canvas; }
    #endregion

    public FrameView()
    {
        InitializeComponent();
    }
}
