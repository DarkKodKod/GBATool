using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands.Character;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace GBATool.ViewModels;

public class CharacterAnimationViewModel : ViewModel
{
    private float _speed = 0.1f;
    private bool _showCollisionBox = false;
    private string _tabID = string.Empty;
    private string _animationID = string.Empty;
    private CharacterModel? _characterModel;
    private FileHandler? _fileHandler;
    private bool _isPlaying;
    private bool _isPaused;
    private double _rectangleTop = 0.0;
    private double _rectangleLeft = 0.0;
    private ImageSource? _frameImage;
    private int _frameIndex;
    private string _frameID = string.Empty;
    private bool _dontSave = false;
    private DispatcherTimer? _dispatcherTimer;
    private int _collisionWidth;
    private int _collisionHeight;
    private int _collisionOffsetX;
    private int _collisionOffsetY;
    private Visibility _rectangleVisibility = Visibility.Hidden;
    private double _rectangleWidth;
    private double _rectangleHeight;
    private readonly Dictionary<string, ImageSource> _bitmapImages = [];

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
        get { return _isPlaying; }
        set
        {
            _isPlaying = value;

            OnPropertyChanged("IsPlaying");
        }
    }

    public double RectangleLeft
    {
        get { return _rectangleLeft; }
        set
        {
            _rectangleLeft = value;

            OnPropertyChanged("RectangleLeft");
        }
    }

    public double RectangleWidth
    {
        get { return _rectangleWidth; }
        set
        {
            _rectangleWidth = value;

            OnPropertyChanged("RectangleWidth");
        }
    }

    public double RectangleHeight
    {
        get { return _rectangleHeight; }
        set
        {
            _rectangleHeight = value;

            OnPropertyChanged("RectangleHeight");
        }
    }

    public double RectangleTop
    {
        get { return _rectangleTop; }
        set
        {
            _rectangleTop = value;

            OnPropertyChanged("RectangleTop");
        }
    }

    public bool IsPaused
    {
        get { return _isPaused; }
        set
        {
            _isPaused = value;

            OnPropertyChanged("IsPaused");
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

    public int FrameIndex
    {
        get
        {
            return _frameIndex;
        }
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

    public FileHandler? FileHandler
    {
        get { return _fileHandler; }
        set
        {
            _fileHandler = value;

            OnPropertyChanged("FileHandler");
        }
    }

    public CharacterModel? CharacterModel
    {
        get { return _characterModel; }
        set
        {
            _characterModel = value;

            OnPropertyChanged("CharacterModel");
        }
    }

    public string TabID
    {
        get { return _tabID; }
        set
        {
            _tabID = value;

            OnPropertyChanged("TabID");
        }
    }

    public int CollisionWidth
    {
        get { return _collisionWidth; }
        set
        {
            _collisionWidth = value;

            //            if (_animationIndex != -1)
            //            {
            //                CharacterModel? model = CharacterModel;

            //                if (model != null)
            {
                //                    CollisionInfo colInfo = model.Animations[_animationIndex].CollisionInfo;
                //
                //                    colInfo.Width = value;
                //
                //                    model.Animations[_animationIndex].CollisionInfo = colInfo;

                //                   RectangleWidth = value;

                if (!_dontSave)
                {
                    FileHandler?.Save();
                }
            }
            //           }

            OnPropertyChanged("CollisionWidth");
        }
    }

    public int CollisionHeight
    {
        get { return _collisionHeight; }
        set
        {
            _collisionHeight = value;

            //            if (_animationIndex != -1)
            //            {
            //                CharacterModel? model = CharacterModel;
            //
            //                if (model != null)
            //                {
            //                    CollisionInfo colInfo = model.Animations[_animationIndex].CollisionInfo;
            //
            //                    colInfo.Height = value;
            //
            //                    model.Animations[_animationIndex].CollisionInfo = colInfo;

            RectangleHeight = value;

            if (!_dontSave)
            {
                FileHandler?.Save();
            }
            //                }
            //            }

            OnPropertyChanged("CollisionHeight");
        }
    }

    public int CollisionOffsetX
    {
        get { return _collisionOffsetX; }
        set
        {
            _collisionOffsetX = value;

            //            if (_animationIndex != -1)
            //            {
            //                if (CharacterModel != null)
            //                {
            //                    CollisionInfo colInfo = CharacterModel.Animations[_animationIndex].CollisionInfo;
            //
            //                    colInfo.OffsetX = value;
            //
            //                    CharacterModel.Animations[_animationIndex].CollisionInfo = colInfo;
            //                }
            //
            //                RectangleLeft = value;
            //
            if (!_dontSave)
            {
                FileHandler?.Save();
            }
            //            }

            OnPropertyChanged("CollisionOffsetX");
        }
    }

    public int CollisionOffsetY
    {
        get { return _collisionOffsetY; }
        set
        {
            _collisionOffsetY = value;

            //            if (_animationIndex != -1)
            //            {
            //                if (CharacterModel != null)
            //                {
            //                    CollisionInfo colInfo = CharacterModel.Animations[_animationIndex].CollisionInfo;
            //
            //                    colInfo.OffsetY = value;
            //
            //                    CharacterModel.Animations[_animationIndex].CollisionInfo = colInfo;
            //                }
            //
            //                RectangleTop = value;
            //
            if (!_dontSave)
            {
                FileHandler?.Save();
            }
            //            }

            OnPropertyChanged("CollisionOffsetY");
        }
    }

    public float Speed
    {
        get { return _speed; }
        set
        {
            if (_speed != value)
            {
                _speed = value;

                //                if (_dispatcherTimer != null)
                //                {
                //                    _dispatcherTimer.Interval = TimeSpan.FromSeconds(Speed);
                //                }
                //
                //                if (_animationIndex != -1)
                //                {
                //                    if (CharacterModel != null)
                //                    {
                //                        CharacterModel.Animations[_animationIndex].Speed = value;

                if (!_dontSave)
                {
                    FileHandler?.Save();
                }
                //                    }
                //                }

                OnPropertyChanged("Speed");
            }
        }
    }

    public Visibility RectangleVisibility
    {
        get { return _rectangleVisibility; }
        set
        {
            _rectangleVisibility = value;

            OnPropertyChanged("RectangleVisibility");
        }
    }

    public bool ShowCollisionBox
    {
        get { return _showCollisionBox; }
        set
        {
            if (_showCollisionBox != value)
            {
                _showCollisionBox = value;

                RectangleVisibility = value == true ? Visibility.Visible : Visibility.Hidden;

                OnPropertyChanged("ShowCollisionBox");
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

                CollisionInfo cInfo = animation.CollisionInfo;

                CollisionWidth = cInfo.Width;
                CollisionHeight = cInfo.Height;
                CollisionOffsetX = cInfo.OffsetX;
                CollisionOffsetY = cInfo.OffsetY;
            }

            IsPlaying = false;

            FrameIndex = 0;
        }

        LoadFrameImage();

        _dispatcherTimer = new DispatcherTimer
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

        if (_dispatcherTimer != null)
        {
            _dispatcherTimer.Stop();
        }
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

        if (!_bitmapImages.TryGetValue(FrameID, out _))
        {
            ImageVO? vo = CharacterUtils.CreateImage(model, _animationID, FrameID);

            if (vo == null || vo.Image == null)
            {
                return;
            }

            _bitmapImages.Add(FrameID, vo.Image);
        }

        FrameImage = _bitmapImages[FrameID];
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

        _bitmapImages.Clear();

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
        FrameIndex--;

        CharacterModel? model = CharacterModel;

        if (FrameIndex < 0)
        {
            if (model != null)
            {
                if (model.Animations.TryGetValue(_animationID, out CharacterAnimation? animation))
                {
                    for (int i = animation.Frames.Count - 1; i >= 0; --i)
                    {
                        //                        if (animation.Frames[i].Tiles != null)
                        //                        {
                        //                            FrameIndex = i;
                        //                            break;
                        //                        }
                    }
                }
            }
        }

        LoadFrameImage();
    }
}
