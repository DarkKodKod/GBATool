using ArchitectureLibrary.Signals;
using GBATool.Commands.Input;
using GBATool.Enums;
using GBATool.Models;
using GBATool.Signals;
using GBATool.VOs;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace GBATool.ViewModels;

public class MapViewModel : ItemViewModel
{
    private Visibility _gridVisibility = Visibility.Visible;
    private ImageSource? _mapImage;
    private bool _doNotSave;
    private int _canvasHeght = 256;
    private int _canvasWidth = 256;
    private float _scale = 3.0f;
    private MapType _mapType = MapType.Regular;
    private Priority _priority = Priority.Highest;
    private BckgrRegularSize _bckgrRegularSize = BckgrRegularSize.Regular32x32;
    private BckgrAffineSize _bckgrAffineSize = BckgrAffineSize.Affine16x16;
    private List<Tile> _tiles = [];
    private bool _enableMosaic;
    private bool _affineWrapping;
    private List<string> _paletteIDs = [];
    private ScreenBaseBlock _screenBaseBlock = ScreenBaseBlock.Block0;
    private CharacterBaseBlock _characterBaseBlock = CharacterBaseBlock.Block0;
    private string _bankID = string.Empty;

    public MapModel? GetModel()
    {
        return ProjectItem?.FileHandler?.FileModel is MapModel model ? model : null;
    }

    #region Commands
    public DragEventCommand<DragLeaveEventSignal> DragLeaveEventCommand { get; } = new();
    public DragEventCommand<DropEventSignal> DropEventCommand { get; } = new();
    public DragEventCommand<DragOverEventSignal> DragOverEventCommand { get; } = new();
    public MouseButtonEventCommand<MouseDownEventSignal> MouseDownEventCommand { get; } = new();
    public MouseButtonEventCommand<MouseUpEventSignal> MouseUpEventCommand { get; } = new();
    public MouseEventCommand<MouseMoveEventSignal> MouseMoveEventCommand { get; } = new();
    #endregion

    #region get/set
    public Visibility GridVisibility
    {
        get => _gridVisibility;
        set
        {
            _gridVisibility = value;

            OnPropertyChanged(nameof(GridVisibility));
        }
    }

    public ImageSource? MapImage
    {
        get => _mapImage;
        set
        {
            _mapImage = value;

            OnPropertyChanged(nameof(MapImage));
        }
    }

    public int CanvasHeight
    {
        get => _canvasHeght;
        set
        {
            _canvasHeght = value;

            OnPropertyChanged(nameof(CanvasHeight));
        }
    }

    public int CanvasWidth
    {
        get => _canvasWidth;
        set
        {
            _canvasWidth = value;

            OnPropertyChanged(nameof(CanvasWidth));
        }
    }

    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;

            OnPropertyChanged(nameof(Scale));
        }
    }

    public MapType MapType
    {
        get => _mapType;
        set
        {
            if (_mapType != value)
            {
                _mapType = value;

                UpdateAndSaveMapType(value);
            }

            OnPropertyChanged(nameof(MapType));
        }
    }
    public Priority Priority
    {
        get => _priority;
        set
        {
            if (_priority != value)
            {
                _priority = value;

                UpdateAndSavePriority(value);
            }

            OnPropertyChanged(nameof(Priority));
        }
    }

    public BckgrRegularSize BckgrRegularSize
    {
        get => _bckgrRegularSize;
        set
        {
            if (_bckgrRegularSize != value)
            {
                switch (BckgrRegularSize)
                {
                    case BckgrRegularSize.Regular64x32:
                        CanvasHeight = 256;
                        CanvasWidth = 512;
                        break;
                    case BckgrRegularSize.Regular32x64:
                        CanvasWidth = 256;
                        CanvasHeight = 512;
                        break;
                    case BckgrRegularSize.Regular64x64:
                        CanvasHeight = CanvasWidth = 512;
                        break;
                    case BckgrRegularSize.Regular32x32:
                    default:
                        CanvasHeight = CanvasWidth = 256;
                        break;
                }

                UpdateAndSaveBackgroundRegularSize(value);

                _bckgrRegularSize = value;
            }

            OnPropertyChanged(nameof(BckgrRegularSize));
        }
    }

    public BckgrAffineSize BckgrAffineSize
    {
        get => _bckgrAffineSize;
        set
        {
            if (_bckgrAffineSize != value)
            {
                _bckgrAffineSize = value;

                UpdateAndSaveBackgroundAffineSize(value);
            }

            OnPropertyChanged(nameof(BckgrAffineSize));
        }
    }

    public List<Tile> Tiles
    {
        get => _tiles;
        set
        {
            _tiles = value;

            OnPropertyChanged(nameof(Tiles));
        }
    }

    public bool EnableMosaic
    {
        get => _enableMosaic;
        set
        {
            if (_enableMosaic != value)
            {
                _enableMosaic = value;

                UpdateAndSaveEnableMosaic(value);
            }

            OnPropertyChanged(nameof(EnableMosaic));
        }
    }

    public bool AffineWrapping
    {
        get => _affineWrapping;
        set
        {
            if (_affineWrapping != value)
            {
                _affineWrapping = value;

                UpdateAndSaveAffineWrapping(value);
            }

            OnPropertyChanged(nameof(AffineWrapping));
        }
    }

    public List<string> PaletteIDs
    {
        get => _paletteIDs;
        set
        {
            _paletteIDs = value;

            OnPropertyChanged(nameof(PaletteIDs));
        }
    }

    public ScreenBaseBlock ScreenBaseBlock
    {
        get => _screenBaseBlock;
        set
        {
            if (_screenBaseBlock != value)
            {
                _screenBaseBlock = value;

                UpdateAndSaveScreenBaseBlock(value);
            }

            OnPropertyChanged(nameof(ScreenBaseBlock));
        }
    }

    public CharacterBaseBlock CharacterBaseBlock
    {
        get => _characterBaseBlock;
        set
        {
            if (_characterBaseBlock != value)
            {
                _characterBaseBlock = value;

                UpdateAndSaveCharacterBaseBlock(value);
            }

            OnPropertyChanged(nameof(CharacterBaseBlock));
        }
    }

    public string BankID
    {
        get => _bankID;
        set
        {
            if (_bankID != value)
            {
                _bankID = value;

                UpdateAndSaveBankID(value);
            }

            OnPropertyChanged(nameof(BankID));
        }
    }
    #endregion

    public override void OnActivate()
    {
        base.OnActivate();

        #region Signals
        SignalManager.Get<DragLeaveEventSignal>().Listener += OnDragLeaveEvent;
        SignalManager.Get<DropEventSignal>().Listener += OnDropEventS;
        SignalManager.Get<DragOverEventSignal>().Listener += OnDragOverEvent;
        SignalManager.Get<MouseDownEventSignal>().Listener += OnMouseDownEvent;
        SignalManager.Get<MouseUpEventSignal>().Listener += OnMouseUpEvent;
        SignalManager.Get<MouseMoveEventSignal>().Listener += OnMouseMoveEvent;
        #endregion

        MapModel? model = GetModel();

        if (model == null)
        {
            return;
        }

        _doNotSave = true;

        MapType = model.MapType;
        Priority = model.Priority;
        BckgrRegularSize = model.BckgrRegularSize;
        BckgrAffineSize = model.BckgrAffineSize;
        Tiles = model.Tiles.ToList();
        EnableMosaic = model.EnableMosaic;
        AffineWrapping = model.AffineWrapping;
        PaletteIDs = model.PaletteIDs.ToList();
        ScreenBaseBlock = model.ScreenBaseBlock;
        CharacterBaseBlock = model.CharacterBaseBlock;
        BankID = model.BankID;

        _doNotSave = false;

        Save();
    }

    private void Save()
    {
        MapModel? map = GetModel();

        if (map == null)
        {
            return;
        }

        for (int i = 0; i < map.Tiles.Length; i++)
        {
            map.Tiles[i].PaletteIndex = 3;
            map.Tiles[i].FlipVertical = true;
            map.Tiles[i].SpriteTileID = "gato";
        }

        ProjectItem?.FileHandler?.Save();
    }

    public override void OnDeactivate()
    {
        #region Signals
        SignalManager.Get<DragLeaveEventSignal>().Listener -= OnDragLeaveEvent;
        SignalManager.Get<DropEventSignal>().Listener -= OnDropEventS;
        SignalManager.Get<DragOverEventSignal>().Listener -= OnDragOverEvent;
        SignalManager.Get<MouseDownEventSignal>().Listener -= OnMouseDownEvent;
        SignalManager.Get<MouseUpEventSignal>().Listener -= OnMouseUpEvent;
        SignalManager.Get<MouseMoveEventSignal>().Listener -= OnMouseMoveEvent;
        #endregion

        base.OnDeactivate();
    }

    private void UpdateAndSaveBankID(string value)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.BankID = value;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSaveCharacterBaseBlock(CharacterBaseBlock value)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.CharacterBaseBlock = value;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSaveScreenBaseBlock(ScreenBaseBlock value)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.ScreenBaseBlock = value;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSaveAffineWrapping(bool value)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.AffineWrapping = value;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSaveEnableMosaic(bool enableMosaic)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.EnableMosaic = enableMosaic;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSaveBackgroundAffineSize(BckgrAffineSize value)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.BckgrAffineSize = value;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSaveBackgroundRegularSize(BckgrRegularSize value)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.BckgrRegularSize = value;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSavePriority(Priority value)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.Priority = value;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSaveMapType(MapType value)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.MapType = value;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void OnMouseMoveEvent(MouseEventVO vO)
    {
        //
    }

    private void OnMouseUpEvent(MouseButtonVO vO)
    {
        //
    }

    private void OnMouseDownEvent(MouseButtonVO vO)
    {
        //
    }

    private void OnDragOverEvent(DragLeaveVO vO)
    {
        //
    }

    private void OnDropEventS(DragLeaveVO vO)
    {
        //
    }

    private void OnDragLeaveEvent(DragLeaveVO vO)
    {
        //
    }
}
