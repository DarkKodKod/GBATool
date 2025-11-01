using ArchitectureLibrary.Signals;
using GBATool.Commands.Utils;
using GBATool.Signals;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace GBATool.Views;

/// <summary>
/// Interaction logic for FrameView.xaml
/// </summary>
public partial class FrameView : UserControl, INotifyPropertyChanged
{
    private int _verticalLineXPos;
    private int _spriteBase;
    private string _crossData = string.Empty;
    private string _originGuide = string.Empty;
    private Visibility _mouseSelectionActive;
    private int _mouseSelectionOriginX;
    private int _mouseSelectionOriginY;
    private int _mouseSelectionWidth;
    private int _mouseSelectionHeight;
    public event PropertyChangedEventHandler? PropertyChanged;

    #region Commands
    public ImageMouseDownCommand ImageMouseDownCommand { get; } = new();
    #endregion

    #region get/set
    public Canvas Canvas { get => canvas; }

    public Visibility MouseSelectionActive
    {
        get => _mouseSelectionActive;
        set
        {
            _mouseSelectionActive = value;

            OnPropertyChanged(nameof(MouseSelectionActive));
        }
    }

    public int MouseSelectionOriginX
    {
        get => _mouseSelectionOriginX;
        set
        {
            _mouseSelectionOriginX = value;

            OnPropertyChanged(nameof(MouseSelectionOriginX));
        }
    }

    public int MouseSelectionOriginY
    {
        get => _mouseSelectionOriginY;
        set
        {
            _mouseSelectionOriginY = value;

            OnPropertyChanged(nameof(MouseSelectionOriginY));
        }
    }

    public int MouseSelectionWidth
    {
        get => _mouseSelectionWidth;
        set
        {
            _mouseSelectionWidth = value;

            OnPropertyChanged(nameof(MouseSelectionWidth));
        }
    }

    public int MouseSelectionHeight
    {
        get => _mouseSelectionHeight;
        set
        {
            _mouseSelectionHeight = value;

            OnPropertyChanged(nameof(MouseSelectionHeight));
        }
    }

    public string OriginGuide
    {
        get => _originGuide;
        set
        {
            _originGuide = value;

            OnPropertyChanged(nameof(OriginGuide));
        }
    }

    public string CrossData
    {
        get => _crossData;
        set
        {
            _crossData = value;

            OnPropertyChanged(nameof(CrossData));
        }
    }

    public int SpriteBase
    {
        get => _spriteBase;
        set
        {
            _spriteBase = value;

            OnPropertyChanged(nameof(SpriteBase));
        }
    }

    public int VerticalLineXPos
    {
        get => _verticalLineXPos;
        set
        {
            _verticalLineXPos = value;

            OnPropertyChanged(nameof(VerticalLineXPos));
        }
    }
    #endregion

    public FrameView()
    {
        InitializeComponent();
    }

    public void OnActivate()
    {
        #region Signals
        SignalManager.Get<UpdateVerticalAxisSignal>().Listener += OnUpdateVerticalAxis;
        SignalManager.Get<UpdateOriginPositionSignal>().Listener += OnUpdateOriginPosition;
        SignalManager.Get<UpdateSpriteBaseSignal>().Listener += OnUpdateSpriteBase;
        #endregion

        SetCrossPosition(0, 0);

        MouseSelectionActive = Visibility.Hidden;
    }

    public void OnDeactivate()
    {
        #region Signals
        SignalManager.Get<UpdateVerticalAxisSignal>().Listener -= OnUpdateVerticalAxis;
        SignalManager.Get<UpdateOriginPositionSignal>().Listener -= OnUpdateOriginPosition;
        SignalManager.Get<UpdateSpriteBaseSignal>().Listener -= OnUpdateSpriteBase;
        #endregion

        MouseSelectionActive = Visibility.Hidden;
    }

    private void SetCrossPosition(int centerPosX, int centerPosY)
    {
        const int lineTotalLength = 2;
        const int lineLength = lineTotalLength / 2;

        int firstInitLineX = centerPosX - lineLength;
        int firstInitLineY = centerPosY - lineLength;
        int firstEndLineX = centerPosX + lineLength;
        int firstEndLineY = centerPosY + lineLength;

        string firstLine = $"M{firstInitLineX},{firstInitLineY}L{firstEndLineX},{firstEndLineY}";
        string secondLine = $"M{firstEndLineX},{firstInitLineY}L{firstInitLineX},{firstEndLineY}";

        CrossData = string.Concat(firstLine, secondLine);

        OriginGuide = $"M{centerPosX},{centerPosY}L200,{centerPosY}M{centerPosX},{centerPosY}L{centerPosX},200";
    }

    private void OnUpdateVerticalAxis(int value)
    {
        VerticalLineXPos = value;
    }

    private void OnUpdateSpriteBase(int value)
    {
        SpriteBase = value;
    }

    private void OnUpdateOriginPosition(int posX, int posY)
    {
        SetCrossPosition(posX, posY);
    }

    protected virtual void OnPropertyChanged(string propname)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }
}
