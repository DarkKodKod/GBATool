using ArchitectureLibrary.Signals;
using GBATool.Commands;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
using GBATool.VOs;
using System.Windows.Media;

namespace GBATool.ViewModels
{
    public class BanksViewModel : ItemViewModel
    {
        private string _selectedSpritePatternFormat = string.Empty;
        private int _selectedTileSet;
        private FileModelVO[]? _tileSets;
        private ImageSource? _pTImage;

        #region Commands
        public ImageMouseDownCommand ImageMouseDownCommand { get; } = new();
        public FileModelVOSelectionChangedCommand FileModelVOSelectionChangedCommand { get; } = new();
        public MoveSpriteToBankCommand MoveSpriteToBankCommand { get; } = new();
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

            LoadTileSetImage();
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

            LoadTileSetImage();
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

        private void LoadTileSetImage()
        {
            if (TileSets == null || TileSets.Length == 0)
            {
                return;
            }

            if (TileSets[SelectedTileSet].Model is not TileSetModel model)
            {
                return;
            }

            // TODO:
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
