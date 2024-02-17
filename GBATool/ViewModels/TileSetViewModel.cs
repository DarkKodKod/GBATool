using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Commands;
using GBATool.Enums;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.VOs;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBATool.ViewModels;

public class TileSetViewModel : ItemViewModel
{
    private string _imagePath = string.Empty;
    private ImageSource? _imgSource;
    private double _actualWidth;
    private double _actualHeight;
    private bool _isSelecting = true;
    private SpriteShape _shape = SpriteShape.Shape00;
    private SpriteSize _size = SpriteSize.Size00;
    private string _imageScale = "100%";
    private int _originalWidth;
    private Visibility _gridVisibility = Visibility.Hidden;
    private Visibility _spriteRectVisibility = Visibility.Hidden;
    private double _spriteRectLeft;
    private double _spriteRectWidth;
    private double _spriteRectHeight;
    private double _spriteRectTop;
    private string _alias = string.Empty;
    private SpriteModel? _selectedSprite;

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
    public ImageMouseDownCommand ImageMouseDownCommand { get; } = new();
    #endregion

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

    public string Alias
    {
        get => _alias;
        set
        {
            if (_alias != value)
            {
                _alias = value;

                OnPropertyChanged("Alias");

                UpdateAndSaveAlias(value);
            }
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

    public bool IsSelecting
    {
        get => _isSelecting;
        set
        {
            _isSelecting = value;

            OnPropertyChanged("IsSelecting");
        }
    }

    public SpriteShape Shape
    {
        get => _shape;
        set
        {
            _shape = value;

            OnPropertyChanged("Shape");
        }
    }

    public SpriteSize Size
    {
        get => _size;
        set
        {
            _size = value;

            OnPropertyChanged("Size");
        }
    }

    public string ImageScale
    {
        get => _imageScale;
        set
        {
            _imageScale = value;

            OnPropertyChanged("ImageScale");
        }
    }

    public Visibility GridVisibility
    {
        get => _gridVisibility;
        set
        {
            _gridVisibility = value;

            OnPropertyChanged("GridVisibility");
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

        int scale = ((int)ActualWidth * 100) / _originalWidth;

        ImageScale = scale.ToString() + "%";

        GridVisibility = scale >= 1000 ? Visibility.Visible : Visibility.Hidden;
    }

    public override void OnActivate()
    {
        base.OnActivate();

        TileSetModel? model = GetModel<TileSetModel>();

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
        SignalManager.Get<MouseImageSelectedSignal>().Listener += OnMouseImageSelected;
        SignalManager.Get<SelectSpriteSignal>().Listener += OnSelectSprite;
        SignalManager.Get<ConfirmSpriteDeletionSignal>().Listener += ConfirmSpriteDeletion;
        #endregion

        if (!string.IsNullOrEmpty(model.ImagePath))
        {
            ProjectModel projectModel = ModelManager.Get<ProjectModel>();

            string path = System.IO.Path.Combine(projectModel.ProjectPath, model.ImagePath);

            ImagePath = path;
        }

        ActualWidth = model.ImageWidth;
        ActualHeight = model.ImageHeight;
        _originalWidth = model.ImageWidth;

        UpdateImage();

        LoadSprites();
    }

    private void UpdateAndSaveAlias(string spriteAlias)
    {
        TileSetModel? model = GetModel<TileSetModel>();

        if (model == null || _selectedSprite?.ID == null)
        {
            return;
        }

        bool ret = model.RenameAliasSprite(_selectedSprite?.ID, spriteAlias);

        if (ret == true)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void OnSelectSprite(SpriteVO sprite)
    {
        TileSetModel? model = GetModel<TileSetModel>();

        if (model == null)
        {
            return;
        }

        _selectedSprite = null;
        Alias = "";

        foreach (SpriteModel item in model.Sprites)
        {
            if (item.ID == sprite.SpriteID)
            {
                SpriteRectVisibility = Visibility.Visible;

                int width = 0;
                int height = 0;
                SpriteUtils.ConvertToWidthHeight(item.Shape, item.Size, ref width, ref height);

                SpriteRectLeft = item.PosX;
                SpriteRectWidth = width;
                SpriteRectHeight = height;
                SpriteRectTop = item.PosY;

                _selectedSprite = item;

                if (string.IsNullOrEmpty(item.Alias))
                {
                    Alias = item.ID;
                }
                else
                {
                    Alias = item.Alias;
                }

                return;
            }
        }
    }

    private void ConfirmSpriteDeletion(SpriteVO sprite)
    {
        SpriteRectVisibility = Visibility.Hidden;

        DeleteSprite(sprite);
    }

    private void OnMouseImageSelected(System.Windows.Controls.Image image, Point point)
    {
        if (IsSelecting)
        {
            return;
        }

        TileSetModel? model = GetModel<TileSetModel>();

        if (model == null)
        {
            return;
        }

        int width = 0;
        int height = 0;

        SpriteUtils.ConvertToWidthHeight(Shape, Size, ref width, ref height);

        WriteableBitmap writeableBmp = BitmapFactory.ConvertToPbgra32Format(image.Source as BitmapSource);

        int x = (int)Math.Floor(point.X);
        int y = (int)Math.Floor(point.Y);

        WriteableBitmap cropped = writeableBmp.Crop(x, y, width, height);

        if (cropped.PixelWidth != width || cropped.PixelHeight != height)
        {
            MessageBox.Show("The sprite you are trying to create is bigger than the area left from the given pixel", "Error", MessageBoxButton.OK);
            return;
        }

        SpriteModel? find = model.Sprites.Find((sprite) =>
            (new SpriteModel()
            {
                PosX = x,
                PosY = y,
                Shape = Shape,
                Size = Size,
                TileSetID = model.GUID
            }) == sprite);

        if (!string.IsNullOrEmpty(find?.ID))
        {
            MessageBox.Show("The exact same sprite already exists", "Error", MessageBoxButton.OK);
            return;
        }

        SpriteVO sprite = AddSpriteToTheList(width, height, Guid.NewGuid().ToString(), cropped);

        if (sprite.SpriteID == null)
        {
            return;
        }

        SignalManager.Get<UpdateSpriteListSignal>().Dispatch();

        SaveNewSprite(sprite.SpriteID, x, y, width, height, model);
    }

    private void SaveNewSprite(string spriteID, int posX, int posY, int width, int height, TileSetModel model)
    {
        SpriteShape shape = SpriteShape.Shape00;
        SpriteSize size = SpriteSize.Size00;

        SpriteUtils.ConvertToShapeSize(width, height, ref shape, ref size);

        bool ret = model.StoreNewSprite(spriteID, posX, posY, shape, size, model.GUID);

        if (ret == true)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void DeleteSprite(SpriteVO sprite)
    {
        TileSetModel? model = GetModel<TileSetModel>();

        if (model == null || sprite.SpriteID == null)
        {
            return;
        }

        bool ret = model.RemoveSprite(sprite.SpriteID);

        if (ret == true)
        {
            _selectedSprite = null;
            Alias = "";

            ProjectItem?.FileHandler?.Save();
        }
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
        SignalManager.Get<MouseImageSelectedSignal>().Listener -= OnMouseImageSelected;
        SignalManager.Get<SelectSpriteSignal>().Listener -= OnSelectSprite;
        SignalManager.Get<ConfirmSpriteDeletionSignal>().Listener -= ConfirmSpriteDeletion;
        #endregion
    }

    private void LoadSprites()
    {
        TileSetModel? model = GetModel<TileSetModel>();

        if (model == null)
        {
            return;
        }

        foreach (SpriteModel sprite in model.Sprites)
        {
            if (string.IsNullOrEmpty(sprite.ID))
            {
                continue;
            }

            int width = 0;
            int height = 0;

            SpriteUtils.ConvertToWidthHeight(sprite.Shape, sprite.Size, ref width, ref height);

            WriteableBitmap writeableBmp = BitmapFactory.ConvertToPbgra32Format(ImgSource as BitmapSource);

            WriteableBitmap cropped = writeableBmp.Crop(sprite.PosX, sprite.PosY, width, height);

            AddSpriteToTheList(width, height, sprite.ID, cropped);
        }
    }

    private static SpriteVO AddSpriteToTheList(int width, int height, string id, BitmapSource bitmap)
    {
        // Scaling here otherwise is too small for display
        width *= 4;
        height *= 4;

        SpriteVO sprite = new()
        {
            SpriteID = id,
            Bitmap = bitmap,
            Width = width,
            Height = height
        };

        SignalManager.Get<AddSpriteSignal>().Dispatch(sprite);

        return sprite;
    }

    private void OnUpdateTileSetImage()
    {
        if (!IsActive)
        {
            return;
        }

        TileSetModel? model = GetModel<TileSetModel>();

        if (model == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(model.ImagePath))
        {
            ProjectModel projectModel = ModelManager.Get<ProjectModel>();

            string path = System.IO.Path.Combine(projectModel.ProjectPath, model.ImagePath);

            ImagePath = path;
        }

        ActualWidth = model.ImageWidth;
        ActualHeight = model.ImageHeight;
        _originalWidth = model.ImageWidth;

        UpdateImage(true);
    }

    private void OnSpriteSelectCursor()
    {
        if (!IsActive)
        {
            return;
        }

        IsSelecting = true;
        Shape = SpriteShape.Shape00;
        Size = SpriteSize.Size00;
    }

    private void OnSpriteSize16x16()
    {
        if (!IsActive)
        {
            return;
        }

        IsSelecting = false;
        Shape = SpriteShape.Shape00;
        Size = SpriteSize.Size01;
    }

    private void OnSpriteSize16x32()
    {
        if (!IsActive)
        {
            return;
        }

        IsSelecting = false;
        Shape = SpriteShape.Shape10;
        Size = SpriteSize.Size10;
    }

    private void OnSpriteSize16x8()
    {
        if (!IsActive)
        {
            return;
        }

        IsSelecting = false;
        Shape = SpriteShape.Shape01;
        Size = SpriteSize.Size00;
    }

    private void OnSpriteSize32x16()
    {
        if (!IsActive)
        {
            return;
        }

        IsSelecting = false;
        Shape = SpriteShape.Shape01;
        Size = SpriteSize.Size10;
    }

    private void OnSpriteSize32x32()
    {
        if (!IsActive)
        {
            return;
        }

        IsSelecting = false;
        Shape = SpriteShape.Shape00;
        Size = SpriteSize.Size10;
    }

    private void OnSpriteSize32x64()
    {
        if (!IsActive)
        {
            return;
        }

        IsSelecting = false;
        Shape = SpriteShape.Shape10;
        Size = SpriteSize.Size11;
    }

    private void OnSpriteSize32x8()
    {
        if (!IsActive)
        {
            return;
        }

        IsSelecting = false;
        Shape = SpriteShape.Shape01;
        Size = SpriteSize.Size01;
    }

    private void OnSpriteSize64x32()
    {
        if (!IsActive)
        {
            return;
        }

        IsSelecting = false;
        Shape = SpriteShape.Shape01;
        Size = SpriteSize.Size11;
    }

    private void OnSpriteSize64x64()
    {
        if (!IsActive)
        {
            return;
        }

        IsSelecting = false;
        Shape = SpriteShape.Shape00;
        Size = SpriteSize.Size11;
    }

    private void OnSpriteSize8x16()
    {
        if (!IsActive)
        {
            return;
        }

        IsSelecting = false;
        Shape = SpriteShape.Shape10;
        Size = SpriteSize.Size00;
    }

    private void OnSpriteSize8x32()
    {
        if (!IsActive)
        {
            return;
        }

        IsSelecting = false;
        Shape = SpriteShape.Shape10;
        Size = SpriteSize.Size01;
    }

    private void OnSpriteSize8x8()
    {
        if (!IsActive)
        {
            return;
        }

        IsSelecting = false;
        Shape = SpriteShape.Shape00;
        Size = SpriteSize.Size00;
    }

    private void UpdateImage(bool forceRedraw = false)
    {
        TileSetModel? model = GetModel<TileSetModel>();

        if (model == null)
        {
            return;
        }

        ImgSource = TileSetModel.LoadBitmap(model, forceRedraw);
    }
}
