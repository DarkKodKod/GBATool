using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands.Character;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GBATool.ViewModels;

public class CharacterAnimationViewModel : ViewModel
{
    private float _speed = 0.1f;
    private int _repeat = 1;
    private bool _looping = true;
    private string _tabID = string.Empty;
    private string _animationID = string.Empty;
    private CharacterModel? _characterModel;
    private FileHandler? _fileHandler;
    private bool _isPlaying;
    private bool _isPaused;
    private ImageSource? _frameImage;
    private int _frameIndex;
    private string _frameID = string.Empty;
    private bool _dontSave;
    private DispatcherTimer? _dispatcherTimer;
    private float _imageAspectRatio = 1.0f;
    private bool _repeatEnable;
    private int _repeatCount;

    private const double CanvasWidth = 300.0;
    private const double CanvasHeight = 300.0;

    #region Commands
    public PauseCharacterAnimationCommand PauseCharacterAnimationCommand { get; } = new();
    public NextFrameCharacterAnimationCommand NextFrameCharacterAnimationCommand { get; } = new();
    public StopCharacterAnimationCommand StopCharacterAnimationCommand { get; } = new();
    public PlayCharacterAnimationCommand PlayCharacterAnimationCommand { get; } = new();
    public PreviousFrameCharacterAnimationCommand PreviousFrameCharacterAnimationCommand { get; } = new();
    public NewAnimationFrameCommand NewAnimationFrameCommand { get; } = new();
    #endregion

    #region get/set
    public bool IsPlaying
    {
        get => _isPlaying;
        set
        {
            _isPlaying = value;

            OnPropertyChanged(nameof(IsPlaying));
        }
    }

    public bool IsPaused
    {
        get => _isPaused;
        set
        {
            _isPaused = value;

            OnPropertyChanged(nameof(IsPaused));
        }
    }

    public float ImageAspectRatio
    {
        get => _imageAspectRatio;
        set
        {
            _imageAspectRatio = value;

            OnPropertyChanged(nameof(ImageAspectRatio));
        }
    }

    public string FrameID
    {
        get => _frameID;
        set
        {
            _frameID = value;

            OnPropertyChanged(nameof(FrameID));
        }
    }

    public int FrameIndex
    {
        get => _frameIndex;
        set
        {
            _frameIndex = value;

            OnPropertyChanged(nameof(FrameIndex));
        }
    }

    public ImageSource? FrameImage
    {
        get => _frameImage;
        set
        {
            _frameImage = value;

            OnPropertyChanged(nameof(FrameImage));
        }
    }

    public FileHandler? FileHandler
    {
        get => _fileHandler;
        set
        {
            _fileHandler = value;

            OnPropertyChanged(nameof(FileHandler));
        }
    }

    public CharacterModel? CharacterModel
    {
        get => _characterModel;
        set
        {
            _characterModel = value;

            OnPropertyChanged(nameof(CharacterModel));
        }
    }

    public string TabID
    {
        get => _tabID;
        set
        {
            _tabID = value;

            OnPropertyChanged(nameof(TabID));
        }
    }

    public float Speed
    {
        get => _speed;
        set
        {
            if (_speed != value)
            {
                _speed = value;

                UpdateSpeedValue(value);

                OnPropertyChanged(nameof(Speed));
            }
        }
    }

    public int Repeat
    {
        get => _repeat;
        set
        {
            if (_repeat != value)
            {
                _repeat = value;

                UpdateRepeatValue(value);

                OnPropertyChanged(nameof(Repeat));
            }
        }
    }

    public bool RepeatEnable
    {
        get => _repeatEnable;
        set
        {
            _repeatEnable = value;

            OnPropertyChanged(nameof(RepeatEnable));
        }
    }

    public bool Looping
    {
        get => _looping;
        set
        {
            if (_looping != value)
            {
                _looping = value;

                RepeatEnable = !value;

                UpdateLoopingValue(value);

                OnPropertyChanged(nameof(Looping));
            }
        }
    }
    #endregion

    public override void OnActivate()
    {
        base.OnActivate();

        #region Signals
        SignalManager.Get<PauseCharacterAnimationSignal>().Listener += OnPauseCharacterAnimation;
        SignalManager.Get<NextFrameCharacterAnimationSignal>().Listener += OnNextFrameCharacterAnimation;
        SignalManager.Get<StopCharacterAnimationSignal>().Listener += OnStopCharacterAnimation;
        SignalManager.Get<PlayCharacterAnimationSignal>().Listener += OnPlayCharacterAnimation;
        SignalManager.Get<PreviousFrameCharacterAnimationSignal>().Listener += OnPreviousFrameCharacterAnimation;
        SignalManager.Get<SwitchAnimationTabSignal>().Listener += OnSwitchAnimationTab;
        #endregion

        _dontSave = true;

        var model = CharacterModel;

        if (model != null)
        {
            if (model.Animations.TryGetValue(TabID, out CharacterAnimation? animation))
            {
                foreach (var item in animation.Frames)
                {
                    SignalManager.Get<NewAnimationFrameSignal>().Dispatch(animation.ID, item.Value.ID, -1, item.Value.IsHeldFrame);
                }

                _animationID = animation.ID;

                Speed = animation.Speed;
                Repeat = animation.Repeat;
                Looping = animation.Looping;
            }

            IsPlaying = false;

            FrameIndex = 0;
            _repeatCount = 0;
        }

        LoadFrameImage();

        _dispatcherTimer = new DispatcherTimer(DispatcherPriority.Input)
        {
            Interval = TimeSpan.FromSeconds(Speed)
        };
        _dispatcherTimer.Tick += (s, args) => Update();
        _dispatcherTimer.Start();

        _dontSave = false;
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        #region Signals
        SignalManager.Get<PauseCharacterAnimationSignal>().Listener -= OnPauseCharacterAnimation;
        SignalManager.Get<NextFrameCharacterAnimationSignal>().Listener -= OnNextFrameCharacterAnimation;
        SignalManager.Get<StopCharacterAnimationSignal>().Listener -= OnStopCharacterAnimation;
        SignalManager.Get<PlayCharacterAnimationSignal>().Listener -= OnPlayCharacterAnimation;
        SignalManager.Get<PreviousFrameCharacterAnimationSignal>().Listener -= OnPreviousFrameCharacterAnimation;
        SignalManager.Get<SwitchAnimationTabSignal>().Listener -= OnSwitchAnimationTab;
        #endregion

        StopAnimation();

        _dispatcherTimer?.Stop();
    }

    public void LoadFrameImage()
    {
        CharacterModel? model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(_animationID))
        {
            return;
        }

        if (!model.Animations.TryGetValue(_animationID, out CharacterAnimation? animation))
        {
            return;
        }

        bool isHeldFrame = false;

        int frameIndex = 0;
        foreach (KeyValuePair<string, FrameModel> item in animation.Frames)
        {
            if (FrameIndex == frameIndex)
            {
                FrameID = item.Value.ID;
                isHeldFrame = item.Value.IsHeldFrame;
                break;
            }

            frameIndex++;
        }

        if (isHeldFrame)
        {
            return;
        }

        if (string.IsNullOrEmpty(FrameID))
        {
            return;
        }

        WriteableBitmap? image = CharacterUtils.GetFrameImageFromCache(model, _animationID, FrameID);

        if (image == null)
        {
            return;
        }

        FrameImage = image;

        WriteableBitmap? firstFrameWithImage = GetImageFromTheFirstValidFrame(model, animation, frameIndex);

        double aspectWidth = CanvasWidth / image.Width;
        double aspectHeight = CanvasHeight / image.Height;

        if (firstFrameWithImage != null)
        {
            aspectWidth = CanvasWidth / firstFrameWithImage.Width;
            aspectHeight = CanvasHeight / firstFrameWithImage.Height;
        }

        double minAspectRation = Math.Min(aspectWidth, aspectHeight);

        ImageAspectRatio = (float)minAspectRation;

        if (firstFrameWithImage != null)
        {
            SendInformationToTheViewAboutTheMetaSprite(model, animation, firstFrameWithImage.Width, firstFrameWithImage.Height, minAspectRation);
        }
    }

    private void SendInformationToTheViewAboutTheMetaSprite(CharacterModel model, CharacterAnimation animation, double imageWidth, double imageHeight, double scale)
    {
        double? tmpOffsetX = null;
        double? tmpOffsetY = null;

        FrameModel frameModel = animation.Frames[FrameID];

        foreach (KeyValuePair<string, CharacterSprite> item in frameModel.Tiles)
        {
            if (tmpOffsetX == null || (model.RelativeOrigin.X - item.Value.Position.X) > tmpOffsetX)
            {
                tmpOffsetX = model.RelativeOrigin.X - item.Value.Position.X;
            }

            if (tmpOffsetY == null || (model.RelativeOrigin.Y - item.Value.Position.Y) > tmpOffsetY)
            {
                tmpOffsetY = model.RelativeOrigin.Y - item.Value.Position.Y;
            }
        }

        double offsetX = tmpOffsetX ?? 0;
        double offsetY = tmpOffsetY ?? 0;

        SignalManager.Get<InformationToCorrectlyDisplayTheMetaSpriteCenteredSignal>().Dispatch(
            offsetX * scale,
            offsetY * scale,
            imageWidth * scale,
            imageHeight * scale);
    }

    private WriteableBitmap? GetImageFromTheFirstValidFrame(CharacterModel model, CharacterAnimation animation, int currentFrameIndex)
    {
        foreach (KeyValuePair<string, FrameModel> frame in animation.Frames)
        {
            foreach (KeyValuePair<string, CharacterSprite> sprite in frame.Value.Tiles)
            {
                if (!string.IsNullOrEmpty(sprite.Key) &&
                    !string.IsNullOrEmpty(sprite.Value.SpriteID))
                {
                    return CharacterUtils.GetFrameImageFromCache(model, _animationID, frame.Key);
                }
            }
        }

        return null;
    }

    private void UpdateRepeatValue(int repeat)
    {
        CharacterModel? model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(_animationID))
        {
            return;
        }

        if (!model.Animations.TryGetValue(_animationID, out CharacterAnimation? animation))
        {
            return;
        }

        animation.Repeat = repeat;

        if (!_dontSave)
        {
            FileHandler?.Save();
        }
    }

    private void UpdateLoopingValue(bool looping)
    {
        CharacterModel? model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(_animationID))
        {
            return;
        }

        if (!model.Animations.TryGetValue(_animationID, out CharacterAnimation? animation))
        {
            return;
        }

        animation.Looping = looping;

        if (!_dontSave)
        {
            FileHandler?.Save();
        }
    }

    private void UpdateSpeedValue(float speed)
    {
        if (_dispatcherTimer != null)
        {
            _dispatcherTimer.Interval = TimeSpan.FromSeconds(speed);
        }

        CharacterModel? model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(_animationID))
        {
            return;
        }

        if (!model.Animations.TryGetValue(_animationID, out CharacterAnimation? animation))
        {
            return;
        }

        animation.Speed = speed;

        if (!_dontSave)
        {
            FileHandler?.Save();
        }
    }

    private void OnSwitchAnimationTab(string activeTab)
    {
        if (!IsActive)
        {
            return;
        }

        if (_dispatcherTimer == null)
        {
            return;
        }

        if (TabID != activeTab)
        {
            StopAnimation();

            _dispatcherTimer.Stop();
        }
        else
        {
            if (!_dispatcherTimer.IsEnabled)
            {
                _dispatcherTimer.Start();
            }
        }
    }

    private void OnPauseCharacterAnimation(string tabId)
    {
        if (!IsActive || TabID != tabId)
        {
            return;
        }

        if (IsPaused)
        {
            return;
        }

        IsPaused = true;
        IsPlaying = false;
    }

    private void OnNextFrameCharacterAnimation(string tabId)
    {
        if (!IsActive || TabID != tabId)
        {
            return;
        }

        IsPaused = true;
        IsPlaying = false;
        _repeatCount = 0;

        NextFrame();
    }

    private void OnStopCharacterAnimation(string tabId)
    {
        if (!IsActive || TabID != tabId)
        {
            return;
        }

        StopAnimation();
    }

    private void OnPlayCharacterAnimation(string tabId)
    {
        if (!IsActive || TabID != tabId)
        {
            return;
        }

        CharacterModel? model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (IsPlaying)
        {
            return;
        }

        IsPlaying = true;
        IsPaused = false;

        if (model.Animations.TryGetValue(_animationID, out CharacterAnimation? animation))
        {
            if (FrameIndex >= animation.Frames.Count)
            {
                _repeatCount = 0;
                FrameIndex = 0;
                LoadFrameImage();
            }
        }
    }

    private void Update()
    {
        if (IsPlaying)
        {
            NextFrame();
        }
    }

    private void OnPreviousFrameCharacterAnimation(string tabId)
    {
        if (!IsActive || TabID != tabId)
        {
            return;
        }

        IsPlaying = false;
        IsPaused = true;
        _repeatCount = 0;

        PreviousFrame();
    }

    private void NextFrame()
    {
        FrameIndex++;

        CharacterModel? model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (model.Animations.TryGetValue(_animationID, out CharacterAnimation? animation))
        {
            if (FrameIndex >= animation.Frames.Count)
            {
                _repeatCount++;

                if (Looping || IsPaused || _repeatCount < Repeat)
                {
                    FrameIndex = 0;
                }
                else
                {
                    StopAnimation();
                    return;
                }
            }

            LoadFrameImage();
        }
    }

    private void PreviousFrame()
    {
        CharacterModel? model = CharacterModel;

        if (model == null)
            return;

        if (!model.Animations.TryGetValue(_animationID, out CharacterAnimation? animation))
        {
            return;
        }

        if (FrameIndex >= animation.Frames.Count)
        {
            FrameIndex = animation.Frames.Count - 1;
        }

        FrameIndex--;

        if (FrameIndex < 0)
        {
            FrameIndex = animation.Frames.Count - 1;
        }

        LoadFrameImage();
    }

    private void StopAnimation()
    {
        IsPlaying = false;
        IsPaused = false;
        _repeatCount = 0;
    }
}
