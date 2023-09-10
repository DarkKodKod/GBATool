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
            #endregion
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
