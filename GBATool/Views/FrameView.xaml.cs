using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.VOs;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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
    private readonly Dictionary<string, Rectangle> _selectedRectangles = [];

    public event PropertyChangedEventHandler? PropertyChanged;

    #region get/set
    public Canvas Canvas { get => canvasGrid; }

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
        SignalManager.Get<SelectFrameSpritesSignal>().Listener += OnSelectFrameSprites;
        SignalManager.Get<ResetFrameSpritesSelectionAreaSignal>().Listener += OnResetFrameSpritesSelectionArea;
        SignalManager.Get<DeleteSpritesFromCharacterFrameSignal>().Listener += OnDeleteSpriteFromCharacterFrame;
        #endregion

        SetCrossPosition(0, 0);

        MouseSelectionActive = Visibility.Collapsed;
        
        parentOfSelectedSprites.Children.Clear();
        _selectedRectangles.Clear();
    }

    public void OnDeactivate()
    {
        #region Signals
        SignalManager.Get<UpdateVerticalAxisSignal>().Listener -= OnUpdateVerticalAxis;
        SignalManager.Get<UpdateOriginPositionSignal>().Listener -= OnUpdateOriginPosition;
        SignalManager.Get<UpdateSpriteBaseSignal>().Listener -= OnUpdateSpriteBase;
        SignalManager.Get<SelectFrameSpritesSignal>().Listener -= OnSelectFrameSprites;
        SignalManager.Get<ResetFrameSpritesSelectionAreaSignal>().Listener -= OnResetFrameSpritesSelectionArea;
        SignalManager.Get<DeleteSpritesFromCharacterFrameSignal>().Listener -= OnDeleteSpriteFromCharacterFrame;
        #endregion

        MouseSelectionActive = Visibility.Collapsed;
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

    private void OnDeleteSpriteFromCharacterFrame(string[] spriteIDs)
    {
        for (int i = 0; i < spriteIDs.Length; i++)
        {
            if (_selectedRectangles.TryGetValue(spriteIDs[i], out Rectangle? rect))
            {
                if (rect != null)
                {
                    parentOfSelectedSprites.Children.Remove(rect);

                    _selectedRectangles.Remove(spriteIDs[i]);
                }
            }
        }
    }

    private void OnSelectFrameSprites(SpriteControlVO[] sprites)
    {
        parentOfSelectedSprites.Children.Clear();
        _selectedRectangles.Clear();

        for (int i = 0; i < sprites.Length; i++)
        {
            Rectangle rectangle = new()
            {
                Visibility = Visibility.Visible,
                Width = sprites[i].Width,
                Height = sprites[i].Height,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 0, 255)),
                IsHitTestVisible = false,
                StrokeThickness = 0.4
            };

            parentOfSelectedSprites.Children.Add(rectangle);

            int imagePosY = (int)Canvas.GetTop(sprites[i].Image);
            int imagePosX = (int)Canvas.GetLeft(sprites[i].Image);

            Canvas.SetTop(rectangle, imagePosY);
            Canvas.SetLeft(rectangle, imagePosX);

            _selectedRectangles.Add(sprites[i].ID, rectangle);
        }
    }

    private void OnResetFrameSpritesSelectionArea(Point position)
    {
        MouseSelectionActive = Visibility.Collapsed;
        MouseSelectionOriginX = (int)position.X;
        MouseSelectionOriginY = (int)position.Y;
        MouseSelectionWidth = 0;
        MouseSelectionHeight = 0;
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
