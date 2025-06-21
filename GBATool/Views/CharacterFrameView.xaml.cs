using GBATool.Commands.Character;
using GBATool.Models;
using GBATool.Utils;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBATool.Views;

/// <summary>
/// Interaction logic for CharacterFrameView.xaml
/// </summary>
public partial class CharacterFrameView : UserControl, INotifyPropertyChanged
{
    private string _animationID = string.Empty;
    private string _frameID = string.Empty;
    private int _frameIndex;
    private ImageSource? _frameImage;
    private CharacterModel? _characterModel;
    private Visibility _imageVisibility = Visibility.Visible;
    private float _imageAspectRatio = 1.0f;

    #region get/set
    public CharacterModel? CharacterModel
    {
        get { return _characterModel; }
        set
        {
            _characterModel = value;

            OnPropertyChanged(nameof(CharacterModel));
        }
    }

    public string AnimationID
    {
        get { return _animationID; }
        set
        {
            _animationID = value;

            OnPropertyChanged(nameof(AnimationID));
        }
    }

    public string FrameID
    {
        get { return _frameID; }
        set
        {
            _frameID = value;

            OnPropertyChanged(nameof(FrameID));
        }
    }

    public Visibility ImageVisibility
    {
        get { return _imageVisibility; }
        set
        {
            _imageVisibility = value;

            OnPropertyChanged(nameof(ImageVisibility));
        }
    }

    public FileHandler FileHandler { get; set; }

    public int FrameIndex
    {
        get { return _frameIndex; }
        set
        {
            _frameIndex = value;

            OnPropertyChanged(nameof(FrameIndex));
        }
    }

    public ImageSource? FrameImage
    {
        get
        {
            return _frameImage;
        }
        set
        {
            _frameImage = value;

            OnPropertyChanged(nameof(FrameImage));
        }
    }

    public float ImageAspectRatio
    {
        get { return _imageAspectRatio; }
        set
        {
            _imageAspectRatio = value;

            OnPropertyChanged(nameof(ImageAspectRatio));
        }
    }
    #endregion

    #region Commands
    public SwitchCharacterFrameViewCommand SwitchCharacterFrameViewCommand { get; } = new();
    public DeleteAnimationFrameCommand DeleteAnimationFrameCommand { get; } = new();
    #endregion

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propname)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }

    public CharacterFrameView(string animationID, string frameID, int frameIndex, FileHandler fileHandler, CharacterModel model)
    {
        InitializeComponent();

        AnimationID = animationID;
        FrameID = frameID;
        FrameIndex = frameIndex;
        FileHandler = fileHandler;
        CharacterModel = model;

        OnActivate();
    }

    public void OnActivate()
    {
        LoadFrameImage();
    }

    private void LoadFrameImage()
    {
        if (CharacterModel == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(AnimationID) || string.IsNullOrEmpty(FrameID))
        {
            return;
        }

        ImageVisibility = CharacterModel.Animations[AnimationID].Frames[FrameID].IsHeldFrame ? Visibility.Hidden : Visibility.Visible;

        WriteableBitmap? image = CharacterUtils.GetFrameImageFromCache(CharacterModel, AnimationID, FrameID);

        if (image == null)
        {
            return;
        }

        FrameImage = image;

        double aspectWidth = 64.0 / image.Width;
        double aspectHeight = 64.0 / image.Height;
        double minAspectRation = Math.Min(aspectWidth, aspectHeight);

        ImageAspectRatio = (float)minAspectRation;
    }
}
