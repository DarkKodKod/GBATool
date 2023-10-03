using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Commands;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.VOs;
using System.IO;
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

        #region Commands
        public ImageMouseDownCommand ImageMouseDownCommand { get; } = new();
        public FileModelVOSelectionChangedCommand FileModelVOSelectionChangedCommand { get; } = new();
        public MoveSpriteToBankCommand MoveSpriteToBankCommand { get; } = new();
        public GoToProjectItemCommand GoToProjectItemCommand { get; } = new();
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
            #endregion

            ProjectModel projectModel = ModelManager.Get<ProjectModel>();

            SelectedSpritePatternFormat = projectModel.SpritePatternFormat == SpritePattern.Format1D ? "1D" : "2D";

            LoadTileSetSprites();
            LoadImage();
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();

            #region Signals
            SignalManager.Get<SelectTileSetSignal>().Listener -= OnSelectTileSet;
            SignalManager.Get<FileModelVOSelectionChangedSignal>().Listener -= OnFileModelVOSelectionChanged;
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

                string itemPath = Path.Combine(fileModelVO.Path, fileModelVO.Name);

                TileSetPath = itemPath.Remove(0, projectModel.ProjectPath.Length);
            }

            TileSetId = model.GUID;

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
            return ProjectItem?.FileHandler?.FileModel is BankModel model ? model : null;
        }

        private void LoadImage()
        {
            BankModel? model = GetModel();

            if (model == null)
            {
                return;
            }

            //WriteableBitmap bitmap = BanksUtils.CreateImage(model, ref _bitmapCache);

            //PTImage = bitmap;
        }
    }
}
