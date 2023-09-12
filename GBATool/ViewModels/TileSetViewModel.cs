using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Commands;
using GBATool.Models;
using GBATool.Signals;
using GBATool.VOs;
using System.IO;
using System.Windows.Media;

namespace GBATool.ViewModels
{
    public class TileSetViewModel : ItemViewModel
    {
        private string _imagePath = string.Empty;
        private ImageSource? _imgSource;
        private double _actualWidth;
        private double _actualHeight;

        #region Commands
        public PreviewMouseWheelCommand PreviewMouseWheelCommand { get; } = new();
        public BrowseFileCommand BrowseFileCommand { get; } = new();
        public DispatchSignalCommand<SpriteSelectCursorSignal> SpriteSelectCursorCommand { get; } = new();
        public DispatchSignalCommand<SpriteSize16x16Signal> SpriteSize16x16Command { get; } = new();
        public DispatchSignalCommand<SpriteSize16x32Signal> SpriteSize16x32Command { get; } = new();
        public DispatchSignalCommand<SpriteSize16x8Signal> SpriteSize16x8Command { get; } = new();
        public DispatchSignalCommand<SpriteSize32x16Signal> SpriteSize32x16Command { get; } = new();
        public DispatchSignalCommand<SpriteSize32x32Signal> SpriteSize32x32Command { get; } = new();
        public DispatchSignalCommand<SpriteSize32x64Signal> SpriteSize32x64Command { get; } = new();
        public DispatchSignalCommand<SpriteSize32x8Signal> SpriteSize32x8Command { get; } = new();
        public DispatchSignalCommand<SpriteSize64x32Signal> SpriteSize64x32Command { get; } = new();
        public DispatchSignalCommand<SpriteSize64x64Signal> SpriteSize64x64Command { get; } = new();
        public DispatchSignalCommand<SpriteSize8x16Signal> SpriteSize8x16Command { get; } = new();
        public DispatchSignalCommand<SpriteSize8x32Signal> SpriteSize8x32Command { get; } = new();
        public DispatchSignalCommand<SpriteSize8x8Signal> SpriteSize8x8Command { get; } = new();
        #endregion

        public TileSetModel? GetModel()
        {
            return ProjectItem?.FileHandler?.FileModel is TileSetModel model ? model : null;
        }

        #region get/set
        public string[] Filters { get; } = new string[14];

        public bool NewFile { get; } = false;

        public ImageSource? ImgSource
        {
            get => _imgSource;
            set
            {
                _imgSource = value;

                OnPropertyChanged("ImgSource");
            }
        }

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;

                OnPropertyChanged("ImagePath");

                UpdateImage();
            }
        }

        public double ActualHeight
        {
            get => _actualHeight;
            set
            {
                _actualHeight = value;

                OnPropertyChanged("ActualHeight");
            }
        }

        public double ActualWidth
        {
            get => _actualWidth;
            set
            {
                _actualWidth = value;

                OnPropertyChanged("ActualWidth");
            }
        }
        #endregion

        public TileSetViewModel()
        {
            FillOutFilters();
        }

        private void FillOutFilters()
        {
            Filters[0] = "Image";
            Filters[1] = "*.png;*.bmp;*.gif;*.jpg;*.jpeg;*.jpe;*.jfif;*.tif;*.tiff*.tga";
            Filters[2] = "PNG";
            Filters[3] = "*.png";
            Filters[4] = "BMP";
            Filters[5] = "*.bmp";
            Filters[6] = "GIF";
            Filters[7] = "*.gif";
            Filters[8] = "JPEG";
            Filters[9] = "*.jpg;*.jpeg;*.jpe;*.jfif";
            Filters[10] = "TIFF";
            Filters[11] = "*.tif;*.tiff";
            Filters[12] = "TGA";
            Filters[13] = "*.tga";

            OnPropertyChanged("Filters");
        }

        private void OnBrowseFileSuccess(string filePath, bool newFile)
        {
            if (!IsActive)
            {
                return;
            }

            // Only act when is not a new file, only updates the current one
            if (newFile)
            {
                return;
            }

            ImgSource = null;

            ImagePath = filePath;

            using ImportImageCommand command = new();
            object?[] parameters = new object[2];
            parameters[0] = filePath;
            parameters[1] = ProjectItem;

            command.Execute(parameters);
        }

        private void OnMouseWheel(MouseWheelVO vo)
        {
            if (!IsActive)
            {
                return;
            }

            const double ScaleRate = 1.1;

            if (vo.Delta > 0)
            {
                ActualWidth *= ScaleRate;
                ActualHeight *= ScaleRate;
            }
            else
            {
                ActualWidth /= ScaleRate;
                ActualHeight /= ScaleRate;
            }
        }

        public override void OnActivate()
        {
            base.OnActivate();

            TileSetModel? model = GetModel();

            if (model == null)
            {
                return;
            }

            #region Signals
            SignalManager.Get<BrowseFileSuccessSignal>().Listener += OnBrowseFileSuccess;
            SignalManager.Get<MouseWheelSignal>().Listener += OnMouseWheel;
            SignalManager.Get<UpdateTileSetImageSignal>().Listener += OnUpdateTileSetImage;
            SignalManager.Get<SpriteSelectCursorSignal>().Listener += OnSpriteSelectCursor;
            SignalManager.Get<SpriteSize16x16Signal>().Listener += OnSpriteSize16x16;
            SignalManager.Get<SpriteSize16x32Signal>().Listener += OnSpriteSize16x32;
            SignalManager.Get<SpriteSize16x8Signal>().Listener += OnSpriteSize16x8;
            SignalManager.Get<SpriteSize32x16Signal>().Listener += OnSpriteSize32x16;
            SignalManager.Get<SpriteSize32x32Signal>().Listener += OnSpriteSize32x32;
            SignalManager.Get<SpriteSize32x64Signal>().Listener += OnSpriteSize32x64;
            SignalManager.Get<SpriteSize32x8Signal>().Listener += OnSpriteSize32x8;
            SignalManager.Get<SpriteSize64x32Signal>().Listener += OnSpriteSize64x32;
            SignalManager.Get<SpriteSize64x64Signal>().Listener += OnSpriteSize64x64;
            SignalManager.Get<SpriteSize8x16Signal>().Listener += OnSpriteSize8x16;
            SignalManager.Get<SpriteSize8x32Signal>().Listener += OnSpriteSize8x32;
            SignalManager.Get<SpriteSize8x8Signal>().Listener += OnSpriteSize8x8;
            #endregion

            if (!string.IsNullOrEmpty(model.ImagePath))
            {
                ProjectModel projectModel = ModelManager.Get<ProjectModel>();

                string path = Path.Combine(projectModel.ProjectPath, model.ImagePath);

                ImagePath = path;
            }

            ActualWidth = model.ImageWidth;
            ActualHeight = model.ImageHeight;

            UpdateImage();
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();

            #region Signals
            SignalManager.Get<BrowseFileSuccessSignal>().Listener -= OnBrowseFileSuccess;
            SignalManager.Get<MouseWheelSignal>().Listener -= OnMouseWheel;
            SignalManager.Get<UpdateTileSetImageSignal>().Listener -= OnUpdateTileSetImage;
            SignalManager.Get<SpriteSelectCursorSignal>().Listener -= OnSpriteSelectCursor;
            SignalManager.Get<SpriteSize16x16Signal>().Listener -= OnSpriteSize16x16;
            SignalManager.Get<SpriteSize16x32Signal>().Listener -= OnSpriteSize16x32;
            SignalManager.Get<SpriteSize16x8Signal>().Listener -= OnSpriteSize16x8;
            SignalManager.Get<SpriteSize32x16Signal>().Listener -= OnSpriteSize32x16;
            SignalManager.Get<SpriteSize32x32Signal>().Listener -= OnSpriteSize32x32;
            SignalManager.Get<SpriteSize32x64Signal>().Listener -= OnSpriteSize32x64;
            SignalManager.Get<SpriteSize32x8Signal>().Listener -= OnSpriteSize32x8;
            SignalManager.Get<SpriteSize64x32Signal>().Listener -= OnSpriteSize64x32;
            SignalManager.Get<SpriteSize64x64Signal>().Listener -= OnSpriteSize64x64;
            SignalManager.Get<SpriteSize8x16Signal>().Listener -= OnSpriteSize8x16;
            SignalManager.Get<SpriteSize8x32Signal>().Listener -= OnSpriteSize8x32;
            SignalManager.Get<SpriteSize8x8Signal>().Listener -= OnSpriteSize8x8;
            #endregion
        }

        private void OnUpdateTileSetImage()
        {
            if (!IsActive)
            {
                return;
            }

            TileSetModel? model = GetModel();

            if (model == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(model.ImagePath))
            {
                ProjectModel projectModel = ModelManager.Get<ProjectModel>();

                string path = Path.Combine(projectModel.ProjectPath, model.ImagePath);

                ImagePath = path;
            }

            ActualWidth = model.ImageWidth;
            ActualHeight = model.ImageHeight;

            UpdateImage(true);
        }

        private void OnSpriteSelectCursor()
        {

        }

        private void OnSpriteSize16x16()
        {

        }

        private void OnSpriteSize16x32()
        {

        }

        private void OnSpriteSize16x8()
        {

        }

        private void OnSpriteSize32x16()
        {

        }

        private void OnSpriteSize32x32()
        {

        }

        private void OnSpriteSize32x64()
        {

        }

        private void OnSpriteSize32x8()
        {

        }

        private void OnSpriteSize64x32()
        {

        }

        private void OnSpriteSize64x64()
        {

        }

        private void OnSpriteSize8x16()
        {

        }

        private void OnSpriteSize8x32()
        {

        }

        private void OnSpriteSize8x8()
        {

        }

        private void UpdateImage(bool forceRedraw = false)
        {
            TileSetModel? model = GetModel();

            if (model == null)
            {
                return;
            }

            ImgSource = TileSetModel.LoadBitmap(model, forceRedraw);
        }
    }
}
