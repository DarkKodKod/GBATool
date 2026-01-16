using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Models;
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
    private Visibility _collisionVisibility;
    private int _mouseSelectionOriginX;
    private int _mouseSelectionOriginY;
    private int _mouseSelectionWidth;
    private int _mouseSelectionHeight;
    private readonly Dictionary<string, Rectangle> _selectedRectangles = [];

    public event PropertyChangedEventHandler? PropertyChanged;

    #region get/set
    public Canvas FrameCanvas => canvasGrid;

    public Canvas OnionCanvas => onionSelectedSprites;

    public Canvas CollisionCanvas => collisionCanvas;

    public Visibility MouseSelectionActive
    {
        get => _mouseSelectionActive;
        set
        {
            _mouseSelectionActive = value;

            OnPropertyChanged(nameof(MouseSelectionActive));
        }
    }

    public Visibility CollisionVisibility
    {
        get => _collisionVisibility;
        set
        {
            _collisionVisibility = value;

            OnPropertyChanged(nameof(CollisionVisibility));
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
        SignalManager.Get<SelectFrameElementsSignal>().Listener += OnSelectFrameSprites;
        SignalManager.Get<ResetFrameSpritesSelectionAreaSignal>().Listener += OnResetFrameSpritesSelectionArea;
        SignalManager.Get<DeleteElementsFromCharacterFrameSignal>().Listener += OnDeleteElementsFromCharacterFrame;
        SignalManager.Get<SpriteFrameHideSelectionSignal>().Listener += OnSpriteFrameHideSelection;
        SignalManager.Get<ElementFrameShowSelectionSignal>().Listener += OnElementFrameShowSelection;
        SignalManager.Get<OptionShowCollisionsSignal>().Listener += OnOptionShowCollisions;
        #endregion

        SetCrossPosition(0, 0);

        MouseSelectionActive = Visibility.Collapsed;
        CollisionVisibility = ModelManager.Get<GBAToolConfigurationModel>().ShowCollisions ? Visibility.Visible : Visibility.Collapsed;

        parentOfSelectedSprites.Children.Clear();
        collisionCanvas.Children.Clear();
        onionSelectedSprites.Children.Clear();
        _selectedRectangles.Clear();
    }

    public void OnDeactivate()
    {
        #region Signals
        SignalManager.Get<UpdateVerticalAxisSignal>().Listener -= OnUpdateVerticalAxis;
        SignalManager.Get<UpdateOriginPositionSignal>().Listener -= OnUpdateOriginPosition;
        SignalManager.Get<UpdateSpriteBaseSignal>().Listener -= OnUpdateSpriteBase;
        SignalManager.Get<SelectFrameElementsSignal>().Listener -= OnSelectFrameSprites;
        SignalManager.Get<ResetFrameSpritesSelectionAreaSignal>().Listener -= OnResetFrameSpritesSelectionArea;
        SignalManager.Get<DeleteElementsFromCharacterFrameSignal>().Listener -= OnDeleteElementsFromCharacterFrame;
        SignalManager.Get<SpriteFrameHideSelectionSignal>().Listener -= OnSpriteFrameHideSelection;
        SignalManager.Get<ElementFrameShowSelectionSignal>().Listener -= OnElementFrameShowSelection;
        SignalManager.Get<OptionShowCollisionsSignal>().Listener -= OnOptionShowCollisions;
        #endregion

        MouseSelectionActive = Visibility.Collapsed;
    }

    private void OnOptionShowCollisions(bool visible, string[] collisionIDs)
    {
        CollisionVisibility = visible ? Visibility.Visible : Visibility.Collapsed;

        if (!visible)
        {
            OnDeleteElementsFromCharacterFrame(collisionIDs);
        }
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

    private void OnDeleteElementsFromCharacterFrame(string[] elementIDs)
    {
        for (int i = 0; i < elementIDs.Length; i++)
        {
            if (_selectedRectangles.TryGetValue(elementIDs[i], out Rectangle? rect))
            {
                if (rect != null)
                {
                    parentOfSelectedSprites.Children.Remove(rect);

                    _selectedRectangles.Remove(elementIDs[i]);
                }
            }
        }
    }

    private void OnSpriteFrameHideSelection()
    {
        foreach (KeyValuePair<string, Rectangle> item in _selectedRectangles)
        {
            item.Value.Visibility = Visibility.Collapsed;
        }
    }

    private void OnElementFrameShowSelection(FrameElementDragObjectVO[] elements)
    {
        for (int i = 0; i < elements.Length; i++)
        {
            if (_selectedRectangles.TryGetValue(elements[i].DragableObject.ID, out Rectangle? rectangle))
            {
                rectangle.Visibility = Visibility.Visible;

                if (elements[i].DragableObject is SpriteControlVO spriteVO)
                {
                    Canvas.SetTop(rectangle, (int)Canvas.GetTop(spriteVO.Image));
                    Canvas.SetLeft(rectangle, (int)Canvas.GetLeft(spriteVO.Image));
                }
                else if (elements[i].DragableObject is CollisionControlVO collisionVO)
                {
                    Canvas.SetTop(rectangle, (int)Canvas.GetTop(collisionVO.Rectangle));
                    Canvas.SetLeft(rectangle, (int)Canvas.GetLeft(collisionVO.Rectangle));
                }
            }
        }
    }

    private void OnSelectFrameSprites(SpriteControlVO[] sprites, CollisionControlVO[] collisions)
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

        for (int i = 0; i < collisions.Length; i++)
        {
            Rectangle rectangle = new()
            {
                Visibility = Visibility.Visible,
                Width = collisions[i].Width,
                Height = collisions[i].Height,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 0, 255)),
                IsHitTestVisible = false,
                StrokeThickness = 0.4
            };

            parentOfSelectedSprites.Children.Add(rectangle);

            int imagePosY = (int)Canvas.GetTop(collisions[i].Rectangle);
            int imagePosX = (int)Canvas.GetLeft(collisions[i].Rectangle);

            Canvas.SetTop(rectangle, imagePosY);
            Canvas.SetLeft(rectangle, imagePosX);

            _selectedRectangles.Add(collisions[i].ID, rectangle);
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
