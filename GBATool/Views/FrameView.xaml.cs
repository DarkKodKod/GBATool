using GBATool.Commands.Utils;
using GBATool.Enums;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GBATool.Views;

/// <summary>
/// Interaction logic for FrameView.xaml
/// </summary>
public partial class FrameView : UserControl, INotifyPropertyChanged
{
    private ImageSource? _frameImage;
    private Visibility _rectangleVisibility = Visibility.Hidden;
    private double _rectangleTop = 0.0;
    private double _rectangleLeft = 0.0;
    private bool _flipX = false;
    private bool _flipY = false;
    private bool _backBackground = false;
    private EditFrameTools _editFrameTools;

    public event PropertyChangedEventHandler? PropertyChanged;

    #region Commands
    public ImageMouseDownCommand ImageMouseDownCommand { get; } = new();
    #endregion

    #region get/set
    public ImageSource? FrameImage
    {
        get => _frameImage;
        set
        {
            _frameImage = value;

            OnPropertyChanged("FrameImage");
        }
    }

    public int SelectedFrameTile { get; set; } = -1;


    public bool BackBackground
    {
        get => _backBackground;
        set
        {
            if (EditFrameTools != EditFrameTools.Select)
            {
                return;
            }

            if (_backBackground != value)
            {
                _backBackground = value;

                OnPropertyChanged("BackBackground");
            }
        }
    }

    public bool FlipX
    {
        get => _flipX;
        set
        {
            if (EditFrameTools != EditFrameTools.Select)
            {
                return;
            }

            if (_flipX != value)
            {
                _flipX = value;

                OnPropertyChanged("FlipX");
            }
        }
    }

    public bool FlipY
    {
        get => _flipY;
        set
        {
            if (EditFrameTools != EditFrameTools.Select)
            {
                return;
            }

            if (_flipY != value)
            {
                _flipY = value;

                OnPropertyChanged("FlipY");
            }
        }
    }

    public EditFrameTools EditFrameTools
    {
        get => _editFrameTools;
        set
        {
            if (_editFrameTools != value)
            {
                _editFrameTools = value;

                if (_editFrameTools != EditFrameTools.Select)
                {
                    RectangleVisibility = Visibility.Hidden;
                }

                OnPropertyChanged("EditFrameTools");
            }
        }
    }

    public double RectangleLeft
    {
        get => _rectangleLeft;
        set
        {
            _rectangleLeft = value;

            OnPropertyChanged("RectangleLeft");
        }
    }

    public double RectangleTop
    {
        get => _rectangleTop;
        set
        {
            _rectangleTop = value;

            OnPropertyChanged("RectangleTop");
        }
    }

    public Visibility RectangleVisibility
    {
        get { return _rectangleVisibility; }
        set
        {
            _rectangleVisibility = value;

            OnPropertyChanged("RectangleVisibility");
        }
    }
    #endregion

    public FrameView()
    {
        InitializeComponent();
    }

    public void OnActivate()
    {
        EditFrameTools = EditFrameTools.Select;

        #region Signals
        #endregion
    }

    public void OnDeactivate()
    {
        #region Signals
        #endregion
    }

    protected virtual void OnPropertyChanged(string propname)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }
}
