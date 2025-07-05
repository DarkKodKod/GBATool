﻿using ArchitectureLibrary.Signals;
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
    private string _tabID = string.Empty;
    private string _animationID = string.Empty;
    private CharacterModel? _characterModel;
    private FileHandler? _fileHandler;
    private bool _isPlaying;
    private bool _isPaused;
    private ImageSource? _frameImage;
    private int _frameIndex;
    private string _frameID = string.Empty;
    private bool _dontSave = false;
    private DispatcherTimer? _dispatcherTimer;
    private float _imageAspectRatio = 1.0f;

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
        #endregion

        _dontSave = true;

        var model = CharacterModel;

        if (model != null)
        {
            if (model.Animations.TryGetValue(TabID, out CharacterAnimation? animation))
            {
                foreach (var item in animation.Frames)
                {
                    SignalManager.Get<NewAnimationFrameSignal>().Dispatch(animation.ID, item.Value.ID);
                }

                _animationID = animation.ID;

                Speed = animation.Speed;
            }

            IsPlaying = false;

            FrameIndex = 0;
        }

        LoadFrameImage();

        _dispatcherTimer = new DispatcherTimer(DispatcherPriority.Input)
        {
            Interval = TimeSpan.FromSeconds(Speed)
        };
        _dispatcherTimer.Tick += Update;
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
        #endregion

        IsPlaying = false;
        IsPaused = false;

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

        int frameIndex = 0;
        foreach (var item in animation.Frames)
        {
            if (FrameIndex == frameIndex)
            {
                FrameID = item.Value.ID;
                break;
            }

            frameIndex++;
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
            SendInformationToTheViewAboutTheMetaSprite(animation, firstFrameWithImage.Width, firstFrameWithImage.Height, minAspectRation);
        }
    }

    private void SendInformationToTheViewAboutTheMetaSprite(CharacterAnimation animation, double imageWidth, double imageHeight, double scale)
    {
        double offsetX = 0;
        double offsetY = 0;

        FrameModel frameModel = animation.Frames[FrameID];

        foreach (KeyValuePair<string, CharacterSprite> item in frameModel.Tiles)
        {
            if (item.Value.Position.X < animation.RelativeOrigin.X &&
                (animation.RelativeOrigin.X - item.Value.Position.X) > offsetX)
            {
                offsetX = animation.RelativeOrigin.X - item.Value.Position.X;
            }

            if (item.Value.Position.Y < animation.RelativeOrigin.Y &&
                (animation.RelativeOrigin.Y - item.Value.Position.Y) > offsetY)
            {
                offsetY = animation.RelativeOrigin.Y - item.Value.Position.Y;
            }
        }

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

        NextFrame();
    }

    private void OnStopCharacterAnimation(string tabId)
    {
        if (!IsActive || TabID != tabId)
        {
            return;
        }

        IsPlaying = false;
        IsPaused = false;
    }

    private void OnPlayCharacterAnimation(string tabId)
    {
        if (!IsActive || TabID != tabId)
        {
            return;
        }

        if (IsPlaying)
        {
            return;
        }

        IsPlaying = true;
        IsPaused = false;
    }

    private void Update(object? sender, object e)
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
                FrameIndex = 0;
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

        FrameIndex--;

        if (FrameIndex < 0)
        {
            FrameIndex = animation.Frames.Count - 1;
        }

        LoadFrameImage();
    }
}
