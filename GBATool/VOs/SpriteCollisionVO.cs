using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Signals;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace GBATool.VOs;

public class SpriteCollisionVO : INotifyPropertyChanged
{
    private CollisionMask _collisionMask;
    private Visibility _visibility;
    private int _customMask;
    private int _width;
    private int _height;
    private int _posX;
    private int _posY;
    private SolidColorBrush _color = new();

    public string ID { get; init; } = string.Empty;
    public string AnimationID { get; init; } = string.Empty;
    public string FrameID { get; init; } = string.Empty;

    public int Width
    {
        get => _width;
        set
        {
            if (_width != value)
            {
                _width = value;

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
            if (_height != value)
            {
                _height = value;

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
            if (_posX != value)
            {
                _posX = value;

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
            if (_posY != value)
            {
                _posY = value;

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
            if (_color != value)
            {
                _color = value;

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
            if (_collisionMask != value)
            {
                _collisionMask = value;

                SignalManager.Get<UpdateSpriteCollisionInfoSignal>().Dispatch(this);
            }

            Visibility = value == CollisionMask.Custom ? Visibility.Visible : Visibility.Collapsed;

            OnPropertyChanged(nameof(Mask));
        }
    }

    public int CustomMask
    {
        get => _customMask;
        set
        {
            if (_customMask != value)
            {
                _customMask = value;

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