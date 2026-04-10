using ArchitectureLibrary.Signals;
using GBATool.Commands.Banks;
using GBATool.Commands.Input;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
using GBATool.VOs;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace GBATool.ViewModels;

public class BankIndex : INotifyPropertyChanged
{
    private int _index;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propname)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }

    public int PaletteIndex { get; init; }

    public int Index
    {
        get => _index;
        set
        {
            _index = value;

            OnPropertyChanged(nameof(Index));

            SignalManager.Get<ChangeMapPaletteSignal>().Dispatch(value, PaletteIndex);
        }
    }
}

public class MapViewModel : ItemViewModel
{
    private Visibility _gridVisibility = Visibility.Visible;
    private ImageSource? _mapImage = null;
    private FileModelVO[]? _banks;
    private int _selectedBank = -1;
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
    private BindingList<BankIndex> _paletteIDs = [];
    private ScreenBaseBlock _screenBaseBlock = ScreenBaseBlock.Block0;
    private CharacterBaseBlock _characterBaseBlock = CharacterBaseBlock.Block0;
    private string _bankID = string.Empty;
    private List<FileModelVO> _palettes = [];

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
    public FileModelVOSelectionChangedCommand FileModelVOSelectionChangedCommand { get; } = new();
    #endregion

    #region get/set
    public FileModelVO[]? Banks
    {
        get => _banks;
        set
        {
            _banks = value;

            OnPropertyChanged(nameof(Banks));
        }
    }

    public int SelectedBank
    {
        get => _selectedBank;
        set
        {
            _selectedBank = value;

            OnPropertyChanged(nameof(SelectedBank));
        }
    }

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

    public List<FileModelVO> Palettes
    {
        get => _palettes;
        set
        {
            _palettes = value;

            OnPropertyChanged(nameof(Palettes));
        }
    }

    public BindingList<BankIndex> PaletteIDs
    {
        get => _paletteIDs;
        set
        {
            if (_paletteIDs != value)
            {
                _paletteIDs = value;

                OnPropertyChanged(nameof(PaletteIDs));
            }
        }
    }
    #endregion

    public MapViewModel()
    {
        FileModelVO[] filemodelVo = [.. ProjectFiles.GetModels<BankModel>()];

        IEnumerable<FileModelVO> banks = filemodelVo;

        Banks = new FileModelVO[banks.Count()];

        int index = 0;

        foreach (FileModelVO item in banks)
        {
            item.Index = index;

            Banks[index] = item;

            index++;
        }

        List<FileModelVO> list =
        [
            new()
            {
                Index = -1,
                Name = "None",
                Model = null
            },
            .. ProjectFiles.GetModels<PaletteModel>(),
        ];

        Palettes = [.. list];

        PaletteIDs.ListChanged += (s, e) =>
        {
            OnPropertyChanged("Item[]");
        };

        for (int i = 0; i < 16; i++)
        {
            PaletteIDs.Add(new() { PaletteIndex = i });
        }
    }

    public override void OnActivate()
    {
        base.OnActivate();

        #region Signals
        SignalManager.Get<FileModelVOSelectionChangedSignal>().Listener += OnFileModelVOSelectionChanged;
        SignalManager.Get<DragLeaveEventSignal>().Listener += OnDragLeaveEvent;
        SignalManager.Get<DropEventSignal>().Listener += OnDropEventS;
        SignalManager.Get<DragOverEventSignal>().Listener += OnDragOverEvent;
        SignalManager.Get<MouseDownEventSignal>().Listener += OnMouseDownEvent;
        SignalManager.Get<MouseUpEventSignal>().Listener += OnMouseUpEvent;
        SignalManager.Get<MouseMoveEventSignal>().Listener += OnMouseMoveEvent;
        SignalManager.Get<ChangeMapPaletteSignal>().Listener += OnChangeMapPalette;
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
        Tiles = [.. model.Tiles0];
        EnableMosaic = model.EnableMosaic;
        AffineWrapping = model.AffineWrapping;
        ScreenBaseBlock = model.ScreenBaseBlock;
        CharacterBaseBlock = model.CharacterBaseBlock;
        BankID = model.BankID;

        for (int i = 0; i < model.PaletteIDs.Length; ++i)
        {
            string paletteID = model.PaletteIDs[i];

            int index = Palettes.FindIndex(o => o.Model?.GUID == paletteID);

            if (index > 0)
                index--;

            // index -1 is valid because the PaletteIDs array accepts -1 as the first empty element.
            PaletteIDs[i].Index = index;
        }

        SelectBank(BankID);

        _doNotSave = false;
    }

    public override void OnDeactivate()
    {
        #region Signals
        SignalManager.Get<FileModelVOSelectionChangedSignal>().Listener -= OnFileModelVOSelectionChanged;
        SignalManager.Get<DragLeaveEventSignal>().Listener -= OnDragLeaveEvent;
        SignalManager.Get<DropEventSignal>().Listener -= OnDropEventS;
        SignalManager.Get<DragOverEventSignal>().Listener -= OnDragOverEvent;
        SignalManager.Get<MouseDownEventSignal>().Listener -= OnMouseDownEvent;
        SignalManager.Get<MouseUpEventSignal>().Listener -= OnMouseUpEvent;
        SignalManager.Get<MouseMoveEventSignal>().Listener -= OnMouseMoveEvent;
        SignalManager.Get<ChangeMapPaletteSignal>().Listener -= OnChangeMapPalette;
        #endregion

        base.OnDeactivate();
    }

    private void SelectBank(string bankID)
    {
        for (int i = 0; i < Banks?.Length; i++)
        {
            if (Banks[i].Model is not BankModel bank)
            {
                continue;
            }

            if (bank.GUID == bankID)
            {
                SelectedBank = i;
                return;
            }
        }
    }

    private void OnFileModelVOSelectionChanged(FileModelVO fileModel)
    {
        if (fileModel.Model is BankModel)
        {
            SignalManager.Get<CleanUpSpriteListSignal>().Dispatch();

            if (Banks == null || Banks.Length == 0)
            {
                return;
            }

            if (Banks[SelectedBank].Model is not BankModel model)
            {
                return;
            }

            BankID = model.GUID;

            SignalManager.Get<SetBankModelToBankViewerSignal>().Dispatch(model);
            SignalManager.Get<RemoveSpriteSelectionFromBank>().Dispatch();
        }
    }

    private void OnChangeMapPalette(int newPaletteIndex, int paletteObjectIndex)
    {
        if (!Enumerable.Range(0, 15).Contains(paletteObjectIndex))
        {
            return;
        }

        MapModel? model = GetModel();

        if (model == null)
        {
            return;
        }

        FileModelVO? palette = null;

        if (newPaletteIndex == -1)
        {
            palette = new FileModelVO() { Model = new PaletteModel() };
        }
        else
        {
            foreach (FileModelVO item in Palettes)
            {
                if (item.Index == newPaletteIndex)
                {
                    palette = item;
                    break;
                }
            }
        }
             
        if (palette?.Model is not PaletteModel paletteModel)
        {
            return;
        }

        model.PaletteIDs[paletteObjectIndex] = paletteModel.GUID ?? string.Empty;

        SignalManager.Get<PaletteColorArrayChangeSignal>().Dispatch(paletteModel.Colors, paletteObjectIndex);

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
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
