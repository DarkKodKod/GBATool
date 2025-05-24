using GBATool.Commands.Character;
using GBATool.Models;
using GBATool.Utils;
using GBATool.VOs;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

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

    #region get/set
    public CharacterModel? CharacterModel
    {
        get { return _characterModel; }
        set
        {
            _characterModel = value;

            OnPropertyChanged("CharacterModel");
        }
    }

    public string AnimationID
    {
        get { return _animationID; }
        set
        {
            _animationID = value;

            OnPropertyChanged("AnimationID");
        }
    }

    public string FrameID
    {
        get { return _frameID; }
        set
        {
            _frameID = value;

            OnPropertyChanged("FrameID");
        }
    }

    public FileHandler FileHandler { get; set; }

    public int FrameIndex
    {
        get { return _frameIndex; }
        set
        {
            _frameIndex = value;

            OnPropertyChanged("FrameIndex");
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

            OnPropertyChanged("FrameImage");
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

        ImageVO? vo = CharacterUtils.CreateImage(CharacterModel, AnimationID, FrameID);

        if (vo == null || vo.Image == null)
        {
            return;
        }

        FrameImage = vo.Image;
    }
}
