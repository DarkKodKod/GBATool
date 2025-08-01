﻿using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Commands.Utils;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBATool.Views;

/// <summary>
/// Interaction logic for BankViewerView.xaml
/// </summary>
public partial class BankViewerView : UserControl, INotifyPropertyChanged
{
    private float _scale = 2.0f;
    private ImageSource? _bankImage;
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
    private SpriteModel? _selectedSpriteFromBank = null;
    private int _canvasHeght = 256;
    private int _canvasWidth = 256;
    private SpriteModel? _cacheSelectedSpriteFromBank = null;
    private BankImageMetaData? _metaData = null;
    private BankModel? _bankModel = null;
    private SpriteModel? _selectedSprite = null;

    public event PropertyChangedEventHandler? PropertyChanged;

    public static readonly DependencyProperty Force2DViewProperty = DependencyProperty.Register(
        "Force2DView",
        typeof(bool),
        typeof(BankViewerView),
        new PropertyMetadata(false));

    #region Commands
    public ImageMouseDownCommand ImageMouseDownCommand { get; } = new();
    #endregion

    #region get/set
    public BankImageMetaData? MetaData { get => _metaData; }

    public Canvas Canvas { get => canvas; }

    public SpriteModel? SelectedSprite
    {
        get => _selectedSprite;
        set
        {
            _selectedSprite = value;

            OnPropertyChanged(nameof(SelectedSprite));
        }
    }

    public bool Force2DView
    {
        get => (bool)GetValue(Force2DViewProperty);
        set => SetValue(Force2DViewProperty, value);
    }

    public SpriteModel? SelectedSpriteFromBank
    {
        get => _selectedSpriteFromBank;
        set
        {
            HideRectangleSelection();

            _selectedSpriteFromBank = value;

            if (value is not null)
            {
                _cacheSelectedSpriteFromBank = value;

                ShowSelectionOverSprite(_selectedSpriteFromBank);
            }

            OnPropertyChanged(nameof(SelectedSpriteFromBank));
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

    public ImageSource? BankImage
    {
        get => _bankImage;
        set
        {
            _bankImage = value;

            OnPropertyChanged(nameof(BankImage));
        }
    }

    public Visibility SpriteRectVisibility
    {
        get { return _spriteRectVisibility; }
        set
        {
            _spriteRectVisibility = value;

            OnPropertyChanged(nameof(SpriteRectVisibility));
        }
    }

    public double SpriteRectLeft
    {
        get { return _spriteRectLeft; }
        set
        {
            _spriteRectLeft = value;

            OnPropertyChanged(nameof(SpriteRectLeft));
        }
    }

    public double SpriteRectWidth
    {
        get { return _spriteRectWidth; }
        set
        {
            _spriteRectWidth = value;

            OnPropertyChanged(nameof(SpriteRectWidth));
        }
    }

    public double SpriteRectHeight
    {
        get { return _spriteRectHeight; }
        set
        {
            _spriteRectHeight = value;

            OnPropertyChanged(nameof(SpriteRectHeight));
        }
    }

    public double SpriteRectTop
    {
        get { return _spriteRectTop; }
        set
        {
            _spriteRectTop = value;

            OnPropertyChanged(nameof(SpriteRectTop));
        }
    }

    public Visibility SpriteRectVisibility2
    {
        get { return _spriteRectVisibility2; }
        set
        {
            _spriteRectVisibility2 = value;

            OnPropertyChanged(nameof(SpriteRectVisibility2));
        }
    }

    public double SpriteRectLeft2
    {
        get { return _spriteRectLeft2; }
        set
        {
            _spriteRectLeft2 = value;

            OnPropertyChanged(nameof(SpriteRectLeft2));
        }
    }

    public double SpriteRectWidth2
    {
        get { return _spriteRectWidth2; }
        set
        {
            _spriteRectWidth2 = value;

            OnPropertyChanged(nameof(SpriteRectWidth2));
        }
    }

    public double SpriteRectHeight2
    {
        get { return _spriteRectHeight2; }
        set
        {
            _spriteRectHeight2 = value;

            OnPropertyChanged(nameof(SpriteRectHeight2));
        }
    }

    public double SpriteRectTop2
    {
        get { return _spriteRectTop2; }
        set
        {
            _spriteRectTop2 = value;

            OnPropertyChanged(nameof(SpriteRectTop2));
        }
    }

    public Visibility SpriteRectVisibility3
    {
        get { return _spriteRectVisibility3; }
        set
        {
            _spriteRectVisibility3 = value;

            OnPropertyChanged(nameof(SpriteRectVisibility3));
        }
    }

    public double SpriteRectLeft3
    {
        get { return _spriteRectLeft3; }
        set
        {
            _spriteRectLeft3 = value;

            OnPropertyChanged(nameof(SpriteRectLeft3));
        }
    }

    public double SpriteRectWidth3
    {
        get { return _spriteRectWidth3; }
        set
        {
            _spriteRectWidth3 = value;

            OnPropertyChanged(nameof(SpriteRectWidth3));
        }
    }

    public double SpriteRectHeight3
    {
        get { return _spriteRectHeight3; }
        set
        {
            _spriteRectHeight3 = value;

            OnPropertyChanged(nameof(SpriteRectHeight3));
        }
    }

    public double SpriteRectTop3
    {
        get { return _spriteRectTop3; }
        set
        {
            _spriteRectTop3 = value;

            OnPropertyChanged(nameof(SpriteRectTop3));
        }
    }
    #endregion

    public BankViewerView()
    {
        InitializeComponent();
    }

    public void OnActivate()
    {
        #region Signals
        SignalManager.Get<MouseImageSelectedSignal>().Listener += OnMouseImageSelected;
        SignalManager.Get<BankSpriteDeletedSignal>().Listener += OnBankSpriteDeleted;
        SignalManager.Get<ReloadBankImageSignal>().Listener += OnReloadBankImage;
        SignalManager.Get<BankImageUpdatedSignal>().Listener += OnBankImageUpdated;
        SignalManager.Get<ObtainTransparentColorSignal>().Listener += OnObtainTransparentColor;
        SignalManager.Get<GeneratePaletteFromBankSignal>().Listener += OnGeneratePaletteFromBank;
        SignalManager.Get<SetBankModelToBankViewerSignal>().Listener += OnSetBankModelToBankViewer;
        SignalManager.Get<CleanUpSpriteListSignal>().Listener += OnCleanUpSpriteList;
        SignalManager.Get<SelectSpriteSignal>().Listener += OnSelectSprite;
        SignalManager.Get<AdjustCanvasBankSizeSignal>().Listener += OnAdjustCanvasBankSize;
        SignalManager.Get<ReloadBankViewImageSignal>().Listener += OnReloadBankViewImage;
        SignalManager.Get<RemoveSpriteSelectionFromBank>().Listener += OnRemoveSpriteSelectionFromBank;
        #endregion

        SelectedSprite = null;
    }

    public void OnDeactivate()
    {
        #region Signals
        SignalManager.Get<MouseImageSelectedSignal>().Listener -= OnMouseImageSelected;
        SignalManager.Get<BankSpriteDeletedSignal>().Listener -= OnBankSpriteDeleted;
        SignalManager.Get<ReloadBankImageSignal>().Listener -= OnReloadBankImage;
        SignalManager.Get<BankImageUpdatedSignal>().Listener -= OnBankImageUpdated;
        SignalManager.Get<SelectSpriteSignal>().Listener -= OnSelectSprite;
        SignalManager.Get<ObtainTransparentColorSignal>().Listener -= OnObtainTransparentColor;
        SignalManager.Get<GeneratePaletteFromBankSignal>().Listener -= OnGeneratePaletteFromBank;
        SignalManager.Get<SetBankModelToBankViewerSignal>().Listener -= OnSetBankModelToBankViewer;
        SignalManager.Get<CleanUpSpriteListSignal>().Listener -= OnCleanUpSpriteList;
        SignalManager.Get<AdjustCanvasBankSizeSignal>().Listener -= OnAdjustCanvasBankSize;
        SignalManager.Get<ReloadBankViewImageSignal>().Listener -= OnReloadBankViewImage;
        SignalManager.Get<RemoveSpriteSelectionFromBank>().Listener -= OnRemoveSpriteSelectionFromBank;
        #endregion
    }

    private void OnRemoveSpriteSelectionFromBank()
    {
        _cacheSelectedSpriteFromBank = null;
        SelectedSpriteFromBank = null;
    }

    private void OnSelectSprite(SpriteVO sprite)
    {
        if (sprite == null)
            return;

        if (string.IsNullOrEmpty(sprite.TileSetID))
            return;

        TileSetModel? model = ProjectFiles.GetModel<TileSetModel>(sprite.TileSetID);

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

    private void OnCleanUpSpriteList()
    {
        SelectedSprite = null;
    }

    private void OnSetBankModelToBankViewer(BankModel? bankModel)
    {
        if (bankModel == null)
        {
            return;
        }

        _bankModel = bankModel;

        AdjustCanvasHeight(_bankModel.Use256Colors);

        LoadImage(bankModel);
    }

    private void OnReloadBankViewImage()
    {
        ReloadImage();
    }

    private void HideRectangleSelection()
    {
        SpriteRectVisibility = Visibility.Hidden;
        SpriteRectVisibility2 = Visibility.Hidden;
        SpriteRectVisibility3 = Visibility.Hidden;
    }

    private void OnMouseImageSelected(Image image, Point point)
    {
        if (image.Name != "imgBank")
        {
            return;
        }

        SelectedSpriteFromBank = null;
        _cacheSelectedSpriteFromBank = null;

        SelectSpriteFromBankImage(point);
    }

    private void OnBankSpriteDeleted(SpriteModel spriteToDelete)
    {
        if (_bankModel == null)
        {
            return;
        }

        bool ret = _bankModel.RemoveSprite(spriteToDelete);

        if (ret)
        {
            _cacheSelectedSpriteFromBank = null;

            ReloadImage();
        }
    }

    private void OnAdjustCanvasBankSize(bool use256colors)
    {
        AdjustCanvasHeight(use256colors);
    }

    private void AdjustCanvasHeight(bool use256colors)
    {
        if (use256colors)
        {
            CanvasHeight = 128;
        }
        else
        {
            CanvasHeight = 256;
        }
    }

    private static void ReloadImage()
    {
        SignalManager.Get<CleanupBankSpritesSignal>().Dispatch();
        SignalManager.Get<CleanupTileSetLinksSignal>().Dispatch();
        SignalManager.Get<BankImageUpdatedSignal>().Dispatch();
    }

    private void OnReloadBankImage()
    {
        ReloadImage();
    }

    private void LoadImage(BankModel model)
    {
        _metaData = BankUtils.CreateImage(model, Force2DView);

        if (_metaData == null)
            return;

        BankImage = _metaData.image;

        SignalManager.Get<UpdateBankViewerParentWithImageMetadataSignal>().Dispatch(_metaData);
    }

    private void OnBankImageUpdated()
    {
        if (_bankModel == null)
        {
            return;
        }

        LoadImage(_bankModel);

        SelectedSpriteFromBank = null;

        if (_cacheSelectedSpriteFromBank != null)
        {
            SelectedSpriteFromBank = _cacheSelectedSpriteFromBank;
        }
    }

    protected virtual void OnPropertyChanged(string propname)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }

    private bool Is1DImage()
    {
        if (Force2DView)
        {
            return false;
        }

        if (_bankModel == null)
        {
            return false;
        }

        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        bool is1DImage = _bankModel.IsBackground || projectModel.SpritePatternFormat == SpritePattern.Format1D;

        return is1DImage;
    }

    private void ShowSelectionOverSprite(SpriteModel? spriteModel)
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
            SpriteRectVisibility = Visibility.Visible;

            int width = 0;
            int height = 0;
            SpriteUtils.ConvertToWidthHeight(spriteModel.Shape, spriteModel.Size, ref width, ref height);

            // Going through the list again to find the first occurrence of this item
            (int firstIndex, string _, string _) = _metaData.SpriteIndices.Find(item => item.Item2 == spriteModel.ID);

            int canvasWidthInCells = CanvasWidth / BankUtils.SizeOfCellInPixels;
            int left = (firstIndex % canvasWidthInCells) * BankUtils.SizeOfCellInPixels;
            int top = (firstIndex / BankUtils.MaxTextureCellsWidth) * BankUtils.SizeOfCellInPixels;

            SpriteRectLeft = left;
            SpriteRectWidth = width;
            SpriteRectHeight = height;
            SpriteRectTop = top;
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

    private void OnObtainTransparentColor(SpriteModel spriteModel)
    {
        TileSetModel? tileSetModel = ProjectFiles.GetModel<TileSetModel>(spriteModel.TileSetID);

        if (tileSetModel == null)
        {
            return;
        }

        SpriteModel? sm = tileSetModel.Sprites.Find(x => x.ID == spriteModel.ID);

        if (sm == null)
        {
            return;
        }

        int width = 0;
        int height = 0;

        SpriteUtils.ConvertToWidthHeight(spriteModel.Shape, spriteModel.Size, ref width, ref height);

        (_, WriteableBitmap? sourceBitmapCached) = TileSetUtils.GetSourceBitmapFromCache(tileSetModel);

        if (sourceBitmapCached == null)
        {
            return;
        }

        WriteableBitmap sourceBitmap = sourceBitmapCached.CloneCurrentValue();

        WriteableBitmap cropped = sourceBitmap.Crop(sm.PosX, sm.PosY, width, height);

        SignalManager.Get<ReturnTransparentColorFromBankSignal>().Dispatch(cropped.GetPixel(0, 0));
    }

    private void OnGeneratePaletteFromBank(string name, IEnumerable<SpriteModel> bankSprites, Color transparentColor, BitsPerPixel bitPerPixel)
    {
        List<Color> colorArray = new([transparentColor]);

        foreach (SpriteModel spriteModel in bankSprites)
        {
            (_, WriteableBitmap? sourceBitmapCached) = TileSetUtils.GetSourceBitmapFromCache(spriteModel.TileSetID);

            if (sourceBitmapCached == null)
            {
                continue;
            }

            WriteableBitmap sourceBitmap = sourceBitmapCached.CloneCurrentValue();

            int width = 0;
            int height = 0;

            SpriteUtils.ConvertToWidthHeight(spriteModel.Shape, spriteModel.Size, ref width, ref height);

            WriteableBitmap cropped = sourceBitmap.Crop(spriteModel.PosX, spriteModel.PosY, width, height);

            int yOffset = 0;
            int xOffset = 0;

            loop:
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    Color color = cropped.GetPixel(x + xOffset, y + yOffset);

                    if (!colorArray.Contains(color))
                    {
                        if (colorArray.Count < bitPerPixel.GetNumberOfColors())
                        {
                            colorArray.Add(color);
                        }
                    }
                }
            }

            xOffset += 8;
            if (xOffset > cropped.PixelWidth)
            {
                yOffset += 8;
                if (yOffset <= cropped.PixelHeight)
                {
                    xOffset = 0;
                    goto loop;
                }
            }
            else
            {
                goto loop;
            }
        }

        if (colorArray.Count > 1)
        {
            if (!string.IsNullOrEmpty(name))
            {
                SignalManager.Get<TryCreatePaletteElementSignal>().Dispatch(name, colorArray, bitPerPixel);
            }
        }
    }
}
