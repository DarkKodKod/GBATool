using GBATool.Enums;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace GBATool.VOs;

public class SpriteCollisionVO : INotifyPropertyChanged
{
    private CollisionMask _collisionMask;
    private Visibility _visibility;

    public string ID { get; init; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
    public SolidColorBrush Color { get; set; } = new();
    public CollisionMask Mask
    {
        get => _collisionMask;
        set
        {
            _collisionMask = value;

            Visibility = value == CollisionMask.Custom ? Visibility.Visible : Visibility.Collapsed;

            OnPropertyChanged(nameof(Mask));
        }
    }
    public int CustomMask { get; set; }
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