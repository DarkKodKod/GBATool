using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Commands.Banks;
using GBATool.Commands.Utils;
using GBATool.Commands.Windows;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.VOs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBATool.ViewModels;

public class BanksViewModel : ItemViewModel
{
    private string _selectedSpritePatternFormat = string.Empty;
    private string _tileSetPath = string.Empty;
    private string _tileSetId = string.Empty;
    private string _palettePath = string.Empty;
    private string _paletteId = string.Empty;
    private int _selectedTileSet;
    private FileModelVO[]? _tileSets;
    private ObservableCollection<SpriteModel> _bankSprites = [];
    private BankModel? _bankModel = null;
    private string _modelName = string.Empty;
    private bool _use256Colors = false;
    private bool _isBackground = false;
    private Color _transparentColor = Color.FromRgb(0, 0, 0);
    private bool _doNotSave = false;

    #region Commands
    public ImageMouseDownCommand ImageMouseDownCommand { get; } = new();
    public FileModelVOSelectionChangedCommand FileModelVOSelectionChangedCommand { get; } = new();
    public MoveSpriteToBankCommand MoveSpriteToBankCommand { get; } = new();
    public GoToProjectItemCommand GoToProjectItemCommand { get; } = new();
    public DeleteBankSpriteCommand DeleteBankSpriteCommand { get; } = new();
    public ObtainTransparentColorCommand ObtainTransparentColorCommand { get; } = new();
    public GeneratePaletteFromBankCommand GeneratePaletteFromBankCommand { get; } = new();
    public MoveUpSelectedSpriteElement MoveUpSelectedSpriteElement { get; } = new();
    public MoveDownSelectedSpriteElementCommand MoveDownSelectedSpriteElement { get; } = new();
    public OpenColorPickerCommand ActivatePickColorCommand { get; } = new();
    #endregion

    #region get/set
    public string TileSetPath
    {
        get => _tileSetPath;
        set
        {
            _tileSetPath = value;

            OnPropertyChanged("TileSetPath");
        }
    }

    public string TileSetId
    {
        get => _tileSetId;
        set
        {
            _tileSetId = value;

            OnPropertyChanged("TileSetId");
        }
    }

    public string PalettePath
    {
        get => _palettePath;
        set
        {
            _palettePath = value;

            OnPropertyChanged("PalettePath");
        }
    }

    public string PaletteId
    {
        get => _paletteId;
        set
        {
            _paletteId = value;

            OnPropertyChanged("PaletteId");
        }
    }

    public ObservableCollection<SpriteModel> BankSprites
    {
        get => _bankSprites;
        set
        {
            _bankSprites = value;

            OnPropertyChanged("BankSprites");
        }
    }

    public string SelectedSpritePatternFormat
    {
        get => _selectedSpritePatternFormat;
        private set
        {
            _selectedSpritePatternFormat = value;

            OnPropertyChanged("SelectedSpritePatternFormat");
        }
    }

    public int SelectedTileSet
    {
        get => _selectedTileSet;
        set
        {
            _selectedTileSet = value;

            OnPropertyChanged("SelectedTileSet");
        }
    }

    public string ModelName
    {
        get => _modelName;
        set
        {
            _modelName = value;

            OnPropertyChanged("ModelName");
        }
    }

    public FileModelVO[]? TileSets
    {
        get => _tileSets;
        set
        {
            _tileSets = value;

            OnPropertyChanged("TileSets");
        }
    }

    public BankModel? BankModel
    {
        get => _bankModel;
        set
        {
            _bankModel = value;

            OnPropertyChanged("BankModel");
        }
    }

    public Color TransparentColor
    {
        get => _transparentColor;
        set
        {
            if (_transparentColor != value)
            {
                _transparentColor = value;

                UpdateAndSaveTransparentColor();
            }

            OnPropertyChanged("TransparentColor");
        }
    }

    public bool Use256Colors
    {
        get => _use256Colors;
        set
        {
            if (_use256Colors != value)
            {
                _use256Colors = value;

                SignalManager.Get<AdjustCanvasBankSizeSignal>().Dispatch(value);

                UpdateAndSaveUse256Colors();

                SignalManager.Get<ReloadBankViewImageSignal>().Dispatch();
            }

            OnPropertyChanged("Use256Colors");
        }
    }

    public bool IsNotBackground { get => !IsBackground; }

    public bool IsBackground
    {
        get => _isBackground;
        set
        {
            if (_isBackground != value)
            {
                _isBackground = value;

                UpdateAndSaveIsBackground();

                if (ModelManager.Get<ProjectModel>().SpritePatternFormat != SpritePattern.Format1D)
                {
                    SignalManager.Get<ReloadBankViewImageSignal>().Dispatch();
                }
            }

            OnPropertyChanged("IsBackground");
            OnPropertyChanged("IsNotBackground");
        }
    }
    #endregion

    public BanksViewModel()
    {
        TileSets = [.. ProjectFiles.GetModels<TileSetModel>()];
    }

    public override void OnActivate()
    {
        base.OnActivate();

        #region Signals
        SignalManager.Get<SelectTileSetSignal>().Listener += OnSelectTileSet;
        SignalManager.Get<FileModelVOSelectionChangedSignal>().Listener += OnFileModelVOSelectionChanged;
        SignalManager.Get<MoveDownSelectedSpriteElementSignal>().Listener += OnMoveDownSelectedSpriteElement;
        SignalManager.Get<MoveUpSelectedSpriteElementSignal>().Listener += OnMoveUpSelectedSpriteElement;
        SignalManager.Get<PaletteCreatedSuccessfullySignal>().Listener += OnPaletteCreatedSuccessfully;
        SignalManager.Get<UpdateBankViewerParentWithImageMetadataSignal>().Listener += OnUpdateBankViewerParentWithImageMetadata;
        SignalManager.Get<ReloadBankImageSignal>().Listener += OnReloadBankImage;
        SignalManager.Get<CleanupBankSpritesSignal>().Listener += OnCleanupBankSprites;
        SignalManager.Get<SetColorFromColorPickerSignal>().Listener += OnSetColorFromColorPicker;
        SignalManager.Get<ReturnTransparentColorFromBankSignal>().Listener += OnReturnTransparentColorFromBank;
        #endregion

        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        SelectedSpritePatternFormat = projectModel.SpritePatternFormat.Description();

        BankModel? model = GetModel<BankModel>();

        if (model == null)
        {
            return;
        }

        BankModel = model;

        SignalManager.Get<SetBankModelToBankViewerSignal>().Dispatch(model);

        SetPaletteId(model.PaletteId);

        _doNotSave = true;

        Use256Colors = model.Use256Colors;
        IsBackground = model.IsBackground;
        TransparentColor = Util.GetColorFromInt(model.TransparentColor);

        _doNotSave = false;

        LoadTileSetSprites();

        model.CleanUpDeletedSprites();

        string? name = ProjectItem?.FileHandler?.Name;

        if (!string.IsNullOrEmpty(name))
        {
            ModelName = name;
        }
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        #region Signals
        SignalManager.Get<SelectTileSetSignal>().Listener -= OnSelectTileSet;
        SignalManager.Get<FileModelVOSelectionChangedSignal>().Listener -= OnFileModelVOSelectionChanged;
        SignalManager.Get<MoveDownSelectedSpriteElementSignal>().Listener -= OnMoveDownSelectedSpriteElement;
        SignalManager.Get<MoveUpSelectedSpriteElementSignal>().Listener -= OnMoveUpSelectedSpriteElement;
        SignalManager.Get<PaletteCreatedSuccessfullySignal>().Listener -= OnPaletteCreatedSuccessfully;
        SignalManager.Get<UpdateBankViewerParentWithImageMetadataSignal>().Listener -= OnUpdateBankViewerParentWithImageMetadata;
        SignalManager.Get<ReloadBankImageSignal>().Listener -= OnReloadBankImage;
        SignalManager.Get<CleanupBankSpritesSignal>().Listener -= OnCleanupBankSprites;
        SignalManager.Get<SetColorFromColorPickerSignal>().Listener -= OnSetColorFromColorPicker;
        SignalManager.Get<ReturnTransparentColorFromBankSignal>().Listener -= OnReturnTransparentColorFromBank;
        #endregion
    }

    private void OnCleanupBankSprites()
    {
        BankSprites.Clear();
    }

    private void OnReturnTransparentColorFromBank(Color color)
    {
        TransparentColor = color;
    }

    private void OnSetColorFromColorPicker(Control _, Color color)
    {
        TransparentColor = color;
    }

    private void UpdateAndSaveIsBackground()
    {
        if (_doNotSave)
            return;

        if (_bankModel == null)
            return;

        _bankModel.IsBackground = IsBackground;

        ProjectItem?.FileHandler?.Save();
    }

    private void UpdateAndSaveTransparentColor()
    {
        if (_doNotSave)
            return;

        if (_bankModel == null)
            return;

        _bankModel.TransparentColor = Util.GetIntFromColor(TransparentColor);

        ProjectItem?.FileHandler?.Save();
    }

    private void UpdateAndSaveUse256Colors()
    {
        if (_doNotSave)
            return;

        if (_bankModel == null)
        {
            return;
        }

        _bankModel.Use256Colors = Use256Colors;

        ProjectItem?.FileHandler?.Save();
    }

    private void OnUpdateBankViewerParentWithImageMetadata(BankImageMetaData? metaData)
    {
        if (metaData == null)
            return;

        BankSprites = [.. metaData.bankSprites];

        FileModelVO[] tileSets = ProjectFiles.GetModels<TileSetModel>().ToArray();

        foreach (string tileSetId in metaData.UniqueTileSet)
        {
            // Add the link object
            foreach (FileModelVO tileset in tileSets)
            {
                if (tileset.Model?.GUID == tileSetId)
                {
                    SignalManager.Get<AddNewTileSetLinkSignal>().Dispatch(new BankLinkVO() { Caption = tileset.Name, Id = tileSetId });
                    break;
                }
            }
        }

        if (_doNotSave)
            return;

        ProjectItem?.FileHandler?.Save();
    }

    private void OnReloadBankImage()
    {
        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        SelectedSpritePatternFormat = projectModel.SpritePatternFormat.Description();
    }

    private void OnPaletteCreatedSuccessfully(PaletteModel paletteModel)
    {
        SetPaletteId(paletteModel.GUID);

        BankModel? model = GetModel<BankModel>();

        if (model == null)
        {
            return;
        }

        model.PaletteId = paletteModel.GUID;

        ProjectItem?.FileHandler?.Save();
    }

    private void SetPaletteId(string id)
    {
        PaletteId = id;

        if (!string.IsNullOrEmpty(PaletteId))
        {
            FileModelVO? fileModelVO = ProjectFiles.GetFileModel(PaletteId);

            if (fileModelVO != null && fileModelVO.Path != null && fileModelVO.Name != null)
            {
                ProjectModel projectModel = ModelManager.Get<ProjectModel>();

                string itemPath = System.IO.Path.Combine(fileModelVO.Path, fileModelVO.Name);

                PalettePath = itemPath[projectModel.ProjectPath.Length..];
            }
        }
    }

    private void OnMoveUpSelectedSpriteElement(int itemAtIndex)
    {
        BankSprites.Move(itemAtIndex, itemAtIndex - 1);

        BankModel? model = GetModel<BankModel>();

        if (model == null)
        {
            return;
        }

        List<SpriteRef> spriteReflist = [];

        foreach (SpriteModel sprite in BankSprites)
        {
            spriteReflist.Add(new SpriteRef { SpriteID = sprite.ID, TileSetID = sprite.TileSetID });
        }

        model.ReplaceSpriteList(spriteReflist);

        SignalManager.Get<ReloadBankImageSignal>().Dispatch();
    }

    private void OnMoveDownSelectedSpriteElement(int itemAtIndex)
    {
        BankSprites.Move(itemAtIndex, itemAtIndex + 1);

        BankModel? model = GetModel<BankModel>();

        if (model == null)
        {
            return;
        }

        List<SpriteRef> spriteReflist = [];

        foreach (SpriteModel sprite in BankSprites)
        {
            spriteReflist.Add(new SpriteRef { SpriteID = sprite.ID, TileSetID = sprite.TileSetID });
        }

        model.ReplaceSpriteList(spriteReflist);

        SignalManager.Get<ReloadBankImageSignal>().Dispatch();
    }

    private void OnFileModelVOSelectionChanged(FileModelVO fileModel)
    {
        if (!IsActive)
        {
            return;
        }

        SignalManager.Get<CleanUpSpriteListSignal>().Dispatch();

        LoadTileSetSprites();
    }

    private void OnSelectTileSet(string id)
    {
        if (TileSets == null)
        {
            return;
        }

        int index = 0;

        foreach (FileModelVO tileset in TileSets)
        {
            if (tileset?.Model?.GUID == id)
            {
                SelectedTileSet = index;
                break;
            }

            index++;
        }
    }

    private void LoadTileSetSprites()
    {
        if (TileSets == null || TileSets.Length == 0)
        {
            return;
        }

        if (TileSets[SelectedTileSet].Model is not TileSetModel model)
        {
            return;
        }

        BitmapImage? image = TileSetModel.LoadBitmap(model);

        if (image == null)
        {
            return;
        }

        FileModelVO? fileModelVO = ProjectFiles.GetFileModel(model.GUID);

        if (fileModelVO != null && fileModelVO.Path != null && fileModelVO.Name != null)
        {
            ProjectModel projectModel = ModelManager.Get<ProjectModel>();

            string itemPath = System.IO.Path.Combine(fileModelVO.Path, fileModelVO.Name);

            TileSetPath = itemPath[projectModel.ProjectPath.Length..];
        }

        TileSetId = model.GUID;

        WriteableBitmap writeableBmp = BitmapFactory.ConvertToPbgra32Format(image);

        foreach (SpriteModel tileSetSprite in model.Sprites)
        {
            if (string.IsNullOrEmpty(tileSetSprite.ID))
            {
                continue;
            }

            int width = 0;
            int height = 0;

            SpriteUtils.ConvertToWidthHeight(tileSetSprite.Shape, tileSetSprite.Size, ref width, ref height);

            WriteableBitmap cropped = writeableBmp.Crop(tileSetSprite.PosX, tileSetSprite.PosY, width, height);

            // Scaling here otherwise is too small for display
            width *= 4;
            height *= 4;

            SpriteVO sprite = new()
            {
                SpriteID = tileSetSprite.ID,
                Bitmap = cropped,
                Width = width,
                Height = height,
                TileSetID = tileSetSprite.TileSetID
            };

            SignalManager.Get<AddSpriteSignal>().Dispatch(sprite);
        }

        SignalManager.Get<UpdateSpriteListSignal>().Dispatch();
    }
}
