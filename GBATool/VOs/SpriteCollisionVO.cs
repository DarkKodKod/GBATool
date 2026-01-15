using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Signals;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace GBATool.VOs;

public class SpriteCollisionVO : INotifyPropertyChanged
{
    public const int Transparency = 110;

    private CollisionMask _collisionMask;
    private Visibility _visibility = Visibility.Collapsed;
    private int _customMask;
    private int _width;
    private int _height;
    private int _posX;
    private int _posY;
    private SolidColorBrush _color = new();

    public string ID { get; init; } = string.Empty;
    public string AnimationID { get; init; } = string.Empty;
    public string FrameID { get; init; } = string.Empty;
    public bool ActAsVO { get; set; } // This is because this object acts as VO and also as a property in a view

    public int Width
    {
        get => _width;
        set
        {
            if (_width == value)
                return;

            _width = value;

            if (!ActAsVO)
            {
                SignalManager.Get<UpdateSpriteCollisionInfoSignal>().Dispatch(this);
            }

            OnPropertyChanged(nameof(Width));
        }
    }

    public int Height
    {
        get => _height;
        set
        {
            if (_height == value)
                return;

            _height = value;

            if (!ActAsVO)
            {
                SignalManager.Get<UpdateSpriteCollisionInfoSignal>().Dispatch(this);
            }

            OnPropertyChanged(nameof(Height));
        }
    }

    public int PosX
    {
        get => _posX;
        set
        {
            if (_posX == value)
                return;

            _posX = value;

            if (!ActAsVO)
            {
                SignalManager.Get<UpdateSpriteCollisionInfoSignal>().Dispatch(this);
            }

            OnPropertyChanged(nameof(PosX));
        }
    }

    public int PosY
    {
        get => _posY;
        set
        {
            if (_posY == value)
                return;

            _posY = value;

            if (!ActAsVO)
            {
                SignalManager.Get<UpdateSpriteCollisionInfoSignal>().Dispatch(this);
            }

            OnPropertyChanged(nameof(PosY));
        }
    }

    public SolidColorBrush Color
    {
        get => _color;
        set
        {
            if (_color == value)
                return;

            _color = value;

            if (!ActAsVO)
            {
                SignalManager.Get<UpdateSpriteCollisionInfoSignal>().Dispatch(this);
            }

            OnPropertyChanged(nameof(Color));
        }
    }

    public CollisionMask Mask
    {
        get => _collisionMask;
        set
        {
            if (_collisionMask == value)
                return;

            _collisionMask = value;

            if (!ActAsVO)
            {
                SignalManager.Get<UpdateSpriteCollisionInfoSignal>().Dispatch(this);
            }

            OnPropertyChanged(nameof(Mask));

            Visibility = value == CollisionMask.Custom ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public int CustomMask
    {
        get => _customMask;
        set
        {
            if (_customMask == value)
                return;

            _customMask = value;

            if (!ActAsVO)
            {
                SignalManager.Get<UpdateSpriteCollisionInfoSignal>().Dispatch(this);
            }

            OnPropertyChanged(nameof(CustomMask));
        }
    }

    public Visibility Visibility
    {
        get => _visibility;
        private set
        {
            _visibility = value;

            OnPropertyChanged(nameof(Visibility));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propname)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }
}