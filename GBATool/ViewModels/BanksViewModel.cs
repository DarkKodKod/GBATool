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
        private BankModel? _model = null;
        private SpriteModel? _selectedSprite = null;
        private Dictionary<string, WriteableBitmap> _bitmapCache = new();
        private BankImageMetaData? _metaData = null;

        #region Commands
        public ImageMouseDownCommand ImageMouseDownCommand { get; } = new();
        public FileModelVOSelectionChangedCommand FileModelVOSelectionChangedCommand { get; } = new();
        public MoveSpriteToBankCommand MoveSpriteToBankCommand { get; } = new();
        public GoToProjectItemCommand GoToProjectItemCommand { get; } = new();
        public DeleteBankSpriteCommand DeleteBankSpriteCommand { get; } = new();
        #endregion

        #region get/set
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

                    UpdateAndSaveUse256Colors();
                }

                OnPropertyChanged("Use256Colors");
            }
        }

        public bool IsBackground
        {
            get => _isBackground;
            set
            {
                if (_isBackground != value)
                {
                    _isBackground = value;

                    UpdateAndSaveIsBackground();

                    ReloadImage();
                }

                OnPropertyChanged("IsBackground");
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

            LoadTileSetSprites();
            LoadImage(model);
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
            #endregion
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

            for (int i = 0; i < model.Sprites.Length; i++)
            {
                if (string.IsNullOrEmpty(model.Sprites[i].ID))
                {
                    continue;
                }

                int width = 0;
                int height = 0;

                SpriteUtils.ConvertToWidthHeight(model.Sprites[i].Shape, model.Sprites[i].Size, ref width, ref height);

                WriteableBitmap cropped = writeableBmp.Crop(model.Sprites[i].PosX, model.Sprites[i].PosY, width, height);

                // Scaling here otherwise is too small for display
                width *= 4;
                height *= 4;

                SpriteVO sprite = new()
                {
                    SpriteID = model.Sprites[i].ID,
                    Bitmap = cropped,
                    Width = width,
                    Height = height
                };

                SignalManager.Get<AddSpriteSignal>().Dispatch(sprite);
            }

            SignalManager.Get<UpdateSpriteListSignal>().Dispatch();
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

            ProjectModel projectModel = ModelManager.Get<ProjectModel>();

            if (projectModel.SpritePatternFormat == SpritePattern.Format1D)
            {
                // Dont do anything because we are already in the 1D format acording to project configuration
                return;
            }

            _bitmapCache.Clear();

            SignalManager.Get<CleanupTileSetLinksSignal>().Dispatch();

            OnBankImageUpdated();
        }

        private void LoadImage(BankModel model)
        {
            _metaData = BankUtils.CreateImage(model, ref _bitmapCache);

            PTImage = _metaData.image;

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
        }

        private void OnBankSpriteDeleted()
        {
            ReloadImage();
        }

        private void OnMouseImageSelected(Image image, Point point)
        {
            if (_metaData == null)
            {
                return;
            }

            int x = (int)Math.Floor(point.X / BankUtils.SizeOfCellInPixels) * BankUtils.SizeOfCellInPixels;
            int y = (int)Math.Floor(point.Y / BankUtils.SizeOfCellInPixels) * BankUtils.SizeOfCellInPixels;

            int lengthWidth = (int)(image.Width / BankUtils.SizeOfCellInPixels);
            int yPos = (y / BankUtils.SizeOfCellInPixels);
            int xPos = (x / BankUtils.SizeOfCellInPixels);

            int index = (lengthWidth * yPos) + xPos;

            string? selectedSpriteID = _metaData.SpriteIndices.Find(item => item.Item1 == index).Item2;

            if (selectedSpriteID == null)
            {
                return;
            }
        }
    }
}
