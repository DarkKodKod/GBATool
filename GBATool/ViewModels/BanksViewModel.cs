using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Commands;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBATool.ViewModels
{
    public class BanksViewModel : ItemViewModel
    {
        private string _selectedSpritePatternFormat = string.Empty;
        private string _tileSetPath = string.Empty;
        private string _tileSetId = string.Empty;
        private int _selectedTileSet;
        private FileModelVO[]? _tileSets;
        private ImageSource? _pTImage;
        private bool _use256Colors = false;
        private bool _isBackground = false;
        private bool _doNotSave = false;
        private BankModel? _model;
        private SpriteModel? _selectedSprite;
        private SpriteModel? _selectedSpriteFromBank;
        private Dictionary<string, WriteableBitmap> _bitmapCache = new();
        private BankImageMetaData? _metaData;
        private Visibility _spriteRectVisibility = Visibility.Hidden;
        private double _spriteRectLeft;
        private double _spriteRectWidth;
        private double _spriteRectHeight;
        private double _spriteRectTop;
        private Visibility _spriteRectVisibility2 = Visibility.Hidden;
        private double _spriteRectLeft2;
        private double _spriteRectWidth2;
        private double _spriteRectHeight2;
        private double _spriteRectTop2;
        private Visibility _spriteRectVisibility3 = Visibility.Hidden;
        private double _spriteRectLeft3;
        private double _spriteRectWidth3;
        private double _spriteRectHeight3;
        private double _spriteRectTop3;
        private int _canvasHeght = 256;
        private int _canvasWidth = 256;
        private ObservableCollection<SpriteModel> _bankSprites = new();

        #region Commands
        public ImageMouseDownCommand ImageMouseDownCommand { get; } = new();
        public FileModelVOSelectionChangedCommand FileModelVOSelectionChangedCommand { get; } = new();
        public MoveSpriteToBankCommand MoveSpriteToBankCommand { get; } = new();
        public GoToProjectItemCommand GoToProjectItemCommand { get; } = new();
        public DeleteBankSpriteCommand DeleteBankSpriteCommand { get; } = new();
        public MoveUpSelectedSpriteElement MoveUpSelectedSpriteElement { get; } = new();
        public MoveDownSelectedSpriteElement MoveDownSelectedSpriteElement { get; } = new();
        #endregion

        #region get/set
        public int CanvasHeight
        {
            get => _canvasHeght;
            set
            {
                _canvasHeght = value;

                OnPropertyChanged("CanvasHeight");
            }
        }

        public int CanvasWidth
        {
            get => _canvasWidth;
            set
            {
                _canvasWidth = value;

                OnPropertyChanged("CanvasWidth");
            }
        }

        public ImageSource? PTImage
        {
            get => _pTImage;
            set
            {
                _pTImage = value;

                OnPropertyChanged("PTImage");
            }
        }

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

        public ObservableCollection<SpriteModel> BankSprites
        {
            get => _bankSprites;
            set
            {
                _bankSprites = value;

                OnPropertyChanged("BankSprites");
            }
        }

        public SpriteModel? SelectedSpriteFromBank
        {
            get => _selectedSpriteFromBank;
            set
            {
                HideRectangleSelection();

                _selectedSpriteFromBank = value;

                if (_selectedSpriteFromBank is not null)
                {
                    SelectSpriteFromBankImage(_selectedSpriteFromBank);
                }

                OnPropertyChanged("SelectedSpriteFromBank");
            }
        }

        public SpriteModel? SelectedSprite
        {
            get => _selectedSprite;
            set
            {
                _selectedSprite = value;

                OnPropertyChanged("SelectedSprite");
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

        public FileModelVO[]? TileSets
        {
            get => _tileSets;
            set
            {
                _tileSets = value;

                OnPropertyChanged("TileSets");
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

                    AdjustCanvasHeight();

                    UpdateAndSaveUse256Colors();

                    ReloadImage();
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
                        ReloadImage();
                    }
                }

                OnPropertyChanged("IsBackground");
                OnPropertyChanged("IsNotBackground");
            }
        }

        public BankModel? Model
        {
            get => _model;
            set
            {
                _model = value;

                OnPropertyChanged("Model");
            }
        }

        public Visibility SpriteRectVisibility
        {
            get { return _spriteRectVisibility; }
            set
            {
                _spriteRectVisibility = value;

                OnPropertyChanged("SpriteRectVisibility");
            }
        }

        public double SpriteRectLeft
        {
            get { return _spriteRectLeft; }
            set
            {
                _spriteRectLeft = value;

                OnPropertyChanged("SpriteRectLeft");
            }
        }

        public double SpriteRectWidth
        {
            get { return _spriteRectWidth; }
            set
            {
                _spriteRectWidth = value;

                OnPropertyChanged("SpriteRectWidth");
            }
        }

        public double SpriteRectHeight
        {
            get { return _spriteRectHeight; }
            set
            {
                _spriteRectHeight = value;

                OnPropertyChanged("SpriteRectHeight");
            }
        }

        public double SpriteRectTop
        {
            get { return _spriteRectTop; }
            set
            {
                _spriteRectTop = value;

                OnPropertyChanged("SpriteRectTop");
            }
        }

        public Visibility SpriteRectVisibility2
        {
            get { return _spriteRectVisibility2; }
            set
            {
                _spriteRectVisibility2 = value;

                OnPropertyChanged("SpriteRectVisibility2");
            }
        }

        public double SpriteRectLeft2
        {
            get { return _spriteRectLeft2; }
            set
            {
                _spriteRectLeft2 = value;

                OnPropertyChanged("SpriteRectLeft2");
            }
        }

        public double SpriteRectWidth2
        {
            get { return _spriteRectWidth2; }
            set
            {
                _spriteRectWidth2 = value;

                OnPropertyChanged("SpriteRectWidth2");
            }
        }

        public double SpriteRectHeight2
        {
            get { return _spriteRectHeight2; }
            set
            {
                _spriteRectHeight2 = value;

                OnPropertyChanged("SpriteRectHeight2");
            }
        }

        public double SpriteRectTop2
        {
            get { return _spriteRectTop2; }
            set
            {
                _spriteRectTop2 = value;

                OnPropertyChanged("SpriteRectTop2");
            }
        }

        public Visibility SpriteRectVisibility3
        {
            get { return _spriteRectVisibility3; }
            set
            {
                _spriteRectVisibility3 = value;

                OnPropertyChanged("SpriteRectVisibility3");
            }
        }

        public double SpriteRectLeft3
        {
            get { return _spriteRectLeft3; }
            set
            {
                _spriteRectLeft3 = value;

                OnPropertyChanged("SpriteRectLeft3");
            }
        }

        public double SpriteRectWidth3
        {
            get { return _spriteRectWidth3; }
            set
            {
                _spriteRectWidth3 = value;

                OnPropertyChanged("SpriteRectWidth3");
            }
        }

        public double SpriteRectHeight3
        {
            get { return _spriteRectHeight3; }
            set
            {
                _spriteRectHeight3 = value;

                OnPropertyChanged("SpriteRectHeight3");
            }
        }

        public double SpriteRectTop3
        {
            get { return _spriteRectTop3; }
            set
            {
                _spriteRectTop3 = value;

                OnPropertyChanged("SpriteRectTop3");
            }
        }
        #endregion

        public BanksViewModel()
        {
            UpdateDialogInfo();
        }

        public override void OnActivate()
        {
            base.OnActivate();

            #region Signals
            SignalManager.Get<SelectTileSetSignal>().Listener += OnSelectTileSet;
            SignalManager.Get<FileModelVOSelectionChangedSignal>().Listener += OnFileModelVOSelectionChanged;
            SignalManager.Get<SelectSpriteSignal>().Listener += OnSelectSprite;
            SignalManager.Get<BankImageUpdatedSignal>().Listener += OnBankImageUpdated;
            SignalManager.Get<BankSpriteDeletedSignal>().Listener += OnBankSpriteDeleted;
            SignalManager.Get<MouseImageSelectedSignal>().Listener += OnMouseImageSelected;
            SignalManager.Get<ReloadBankImageSignal>().Listener += OnReloadBankImage;
            SignalManager.Get<MoveDownSelectedSpriteElementSignal>().Listener += OnMoveDownSelectedSpriteElement;
            SignalManager.Get<MoveUpSelectedSpriteElementSignal>().Listener += OnMoveUpSelectedSpriteElement;
            #endregion

            ProjectModel projectModel = ModelManager.Get<ProjectModel>();

            SelectedSpritePatternFormat = projectModel.SpritePatternFormat.Description();

            BankModel? model = GetModel();

            if (model == null)
            {
                return;
            }

            _doNotSave = true;

            Use256Colors = model.Use256Colors;
            IsBackground = model.IsBackground;

            _doNotSave = false;

            CanvasWidth = 256;
            AdjustCanvasHeight();

            LoadTileSetSprites();
            LoadImage(model);

            model.CleanUpDeletedSprites();
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();

            #region Signals
            SignalManager.Get<SelectTileSetSignal>().Listener -= OnSelectTileSet;
            SignalManager.Get<FileModelVOSelectionChangedSignal>().Listener -= OnFileModelVOSelectionChanged;
            SignalManager.Get<SelectSpriteSignal>().Listener -= OnSelectSprite;
            SignalManager.Get<BankImageUpdatedSignal>().Listener -= OnBankImageUpdated;
            SignalManager.Get<BankSpriteDeletedSignal>().Listener -= OnBankSpriteDeleted;
            SignalManager.Get<MouseImageSelectedSignal>().Listener -= OnMouseImageSelected;
            SignalManager.Get<ReloadBankImageSignal>().Listener -= OnReloadBankImage;
            SignalManager.Get<MoveDownSelectedSpriteElementSignal>().Listener -= OnMoveDownSelectedSpriteElement;
            SignalManager.Get<MoveUpSelectedSpriteElementSignal>().Listener -= OnMoveUpSelectedSpriteElement;
            #endregion
        }

        private void OnMoveUpSelectedSpriteElement(SpriteModel model)
        {
            //
        }

        private void OnMoveDownSelectedSpriteElement(SpriteModel model)
        {
            //
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

        private void UpdateDialogInfo()
        {
            TileSets = ProjectFiles.GetModels<TileSetModel>().ToArray();
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

        private void UpdateAndSaveUse256Colors()
        {
            if (_doNotSave)
                return;

            BankModel? model = GetModel();

            if (model == null)
                return;

            model.Use256Colors = Use256Colors;

            ProjectItem?.FileHandler?.Save();
        }

        private void UpdateAndSaveIsBackground()
        {
            if (_doNotSave)
                return;

            BankModel? model = GetModel();

            if (model == null)
                return;

            model.IsBackground = IsBackground;

            ProjectItem?.FileHandler?.Save();
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

                TileSetPath = itemPath.Remove(0, projectModel.ProjectPath.Length);
            }

            TileSetId = model.GUID;
            SelectedSprite = null;

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
                    Height = height
                };

                SignalManager.Get<AddSpriteSignal>().Dispatch(sprite);
            }

            SignalManager.Get<UpdateSpriteListSignal>().Dispatch();
        }

        private void AdjustCanvasHeight()
        {
            if (Use256Colors)
            {
                CanvasHeight = 128;
            }
            else
            {
                CanvasHeight = 256;
            }
        }

        public BankModel? GetModel()
        {
            Model ??= ProjectItem?.FileHandler?.FileModel as BankModel;

            return Model;
        }

        private void ReloadImage()
        {
            if (_doNotSave)
                return;

            _bitmapCache.Clear();

            BankSprites.Clear();

            SignalManager.Get<CleanupTileSetLinksSignal>().Dispatch();

            OnBankImageUpdated();
        }

        private void LoadImage(BankModel model)
        {
            _metaData = BankUtils.CreateImage(model, ref _bitmapCache);

            PTImage = _metaData.image;
            BankSprites = new ObservableCollection<SpriteModel>(_metaData.bankSprites);

            FileModelVO[] tileSets = ProjectFiles.GetModels<TileSetModel>().ToArray();

            foreach (string tileSetId in _metaData.UniqueTileSet)
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
        }

        private void OnSelectSprite(SpriteVO sprite)
        {
            TileSetModel? model = ProjectFiles.GetModel<TileSetModel>(TileSetId);

            if (model == null)
            {
                return;
            }

            foreach (SpriteModel item in model.Sprites)
            {
                if (item.ID == sprite.SpriteID)
                {
                    SelectedSprite = item;
                    break;
                }
            }
        }

        private void OnBankImageUpdated()
        {
            ProjectItem?.FileHandler?.Save();

            BankModel? model = GetModel();

            if (model == null)
            {
                return;
            }

            LoadImage(model);

            UnselectSpriteFromBankImage();
        }

        private void OnBankSpriteDeleted(SpriteModel spriteToDelete)
        {
            BankModel? model = GetModel();

            if (model == null)
            {
                return;
            }

            bool ret = model.RemoveSprite(spriteToDelete);

            if (ret)
            {
                ReloadImage();
            }
        }

        private bool Is1DImage()
        {
            BankModel? model = GetModel();

            if (model == null)
            {
                return false;
            }

            ProjectModel projectModel = ModelManager.Get<ProjectModel>();

            bool is1DImage = model.IsBackground || projectModel.SpritePatternFormat == SpritePattern.Format1D;

            return is1DImage;
        }

        private void OnMouseImageSelected(Image _, Point point)
        {
            UnselectSpriteFromBankImage();

            if (Is1DImage())
            {
                SelectSpriteFromBankImage(point);
            }
            else
            {
                // 2d
            }
        }

        private void UnselectSpriteFromBankImage()
        {
            HideRectangleSelection();

            SelectedSpriteFromBank = null;
        }

        private void HideRectangleSelection()
        {
            SpriteRectVisibility = Visibility.Hidden;
            SpriteRectVisibility2 = Visibility.Hidden;
            SpriteRectVisibility3 = Visibility.Hidden;
        }

        private void SelectSpriteFromBankImage(SpriteModel? spriteModel)
        {
            if (_metaData == null || string.IsNullOrEmpty(spriteModel?.ID))
            {
                return;
            }

            if (Is1DImage())
            {
                int canvasWidthInCells = CanvasWidth / BankUtils.SizeOfCellInPixels;
                int firstIndex = 0;
                int spriteTilesTotal = 0;

                foreach (SpriteModel sprite in _metaData.bankSprites)
                {
                    spriteTilesTotal = SpriteUtils.Count8x8Tiles(sprite.Shape, sprite.Size);

                    if (sprite.ID == spriteModel?.ID)
                    {
                        break;
                    }

                    firstIndex += spriteTilesTotal;
                }

                ShowRectOverSprite1D(spriteTilesTotal, firstIndex, canvasWidthInCells);
            }
            else
            {
                // 2d
            }
        }

        private void SelectSpriteFromBankImage(Point point)
        {
            if (_metaData == null)
            {
                return;
            }

            int x = (int)Math.Floor(point.X / BankUtils.SizeOfCellInPixels) * BankUtils.SizeOfCellInPixels;
            int y = (int)Math.Floor(point.Y / BankUtils.SizeOfCellInPixels) * BankUtils.SizeOfCellInPixels;

            int canvasWidthInCells = CanvasWidth / BankUtils.SizeOfCellInPixels;
            int lengthHeight = CanvasHeight / BankUtils.SizeOfCellInPixels;
            int yPos = (y / BankUtils.SizeOfCellInPixels);
            int xPos = (x / BankUtils.SizeOfCellInPixels);

            int cellIndex = (canvasWidthInCells * yPos) + xPos;

            (int _, string spriteID, string tileSetId) = _metaData.SpriteIndices.Find(item => item.Item1 == cellIndex);

            if (string.IsNullOrEmpty(spriteID))
            {
                return;
            }

            // Going through the list again to find the first occurrence of this item
            (int firstIndex, string _, string _) = _metaData.SpriteIndices.Find(item => item.Item2 == spriteID);

            TileSetModel? tileSetModel = ProjectFiles.GetModel<TileSetModel>(tileSetId);

            if (tileSetModel == null)
            {
                return;
            }

            SpriteModel? sprite = tileSetModel.Sprites.Find((item) => item.ID == spriteID);

            if (string.IsNullOrEmpty(sprite?.ID))
            {
                return;
            }

            SelectedSpriteFromBank = sprite;
        }

        private void OnReloadBankImage()
        {
            ProjectModel projectModel = ModelManager.Get<ProjectModel>();

            SelectedSpritePatternFormat = projectModel.SpritePatternFormat.Description();

            ReloadImage();
        }

        private void ShowRectOverSprite1D(int spriteTilesTotal, int firstIndex, int canvasWidthInCells)
        {
            SpriteRectVisibility = Visibility.Visible;

            bool useNextRect = false;

            int left = (firstIndex % canvasWidthInCells) * BankUtils.SizeOfCellInPixels;
            int width = spriteTilesTotal * BankUtils.SizeOfCellInPixels;
            int top = (firstIndex / BankUtils.MaxTextureCellsWidth) * BankUtils.SizeOfCellInPixels;

            int whatIsLeft = 0;

            int maxCellsInPixels = BankUtils.MaxTextureCellsWidth * BankUtils.SizeOfCellInPixels;

            if (left + width > maxCellsInPixels)
            {
                int sub = maxCellsInPixels - left;

                whatIsLeft = width - sub;
                width = sub;

                useNextRect = true;
            }

            SpriteRectLeft = left;
            SpriteRectWidth = width;
            SpriteRectHeight = BankUtils.SizeOfCellInPixels;
            SpriteRectTop = top;

            if (useNextRect)
            {
                useNextRect = false;

                SpriteRectVisibility2 = Visibility.Visible;

                width = whatIsLeft;

                if (width > maxCellsInPixels)
                {
                    whatIsLeft = width - maxCellsInPixels;
                    width = maxCellsInPixels;

                    useNextRect = true;
                }

                SpriteRectLeft2 = 0;
                SpriteRectWidth2 = width;
                SpriteRectHeight2 = BankUtils.SizeOfCellInPixels;
                SpriteRectTop2 = top + BankUtils.SizeOfCellInPixels;

                if (useNextRect)
                {
                    SpriteRectVisibility3 = Visibility.Visible;

                    SpriteRectLeft3 = 0;
                    SpriteRectWidth3 = whatIsLeft;
                    SpriteRectHeight3 = BankUtils.SizeOfCellInPixels;
                    SpriteRectTop3 = top + (BankUtils.SizeOfCellInPixels * 2);
                }
            }
        }
    }
}
