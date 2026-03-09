using ArchitectureLibrary.Signals;
using GBATool.Commands.Input;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace GBATool.ViewModels;

public class MapViewModel : ItemViewModel
{
    private Visibility _gridVisibility = Visibility.Visible;
    private ImageSource? _mapImage;
    private int _canvasHeght = 256;
    private int _canvasWidth = 256;
    private float _scale = 3.0f;

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
    #endregion

    #region get/set
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
    #endregion

    public override void OnActivate()
    {
        base.OnActivate();

        #region Signals
        SignalManager.Get<DragLeaveEventSignal>().Listener += OnDragLeaveEvent;
        SignalManager.Get<DropEventSignal>().Listener += OnDropEventS;
        SignalManager.Get<DragOverEventSignal>().Listener += OnDragOverEvent;
        SignalManager.Get<MouseDownEventSignal>().Listener += OnMouseDownEvent;
        SignalManager.Get<MouseUpEventSignal>().Listener += OnMouseUpEvent;
        SignalManager.Get<MouseMoveEventSignal>().Listener += OnMouseMoveEvent;
        #endregion

        MapModel? model = GetModel();

        if (model == null)
        {
            return;
        }

        LoadMapData();
    }

    private void LoadMapData()
    {
        //
    }

    public override void OnDeactivate()
    {
        #region Signals
        SignalManager.Get<DragLeaveEventSignal>().Listener -= OnDragLeaveEvent;
        SignalManager.Get<DropEventSignal>().Listener -= OnDropEventS;
        SignalManager.Get<DragOverEventSignal>().Listener -= OnDragOverEvent;
        SignalManager.Get<MouseDownEventSignal>().Listener -= OnMouseDownEvent;
        SignalManager.Get<MouseUpEventSignal>().Listener -= OnMouseUpEvent;
        SignalManager.Get<MouseMoveEventSignal>().Listener -= OnMouseMoveEvent;
        #endregion

        base.OnDeactivate();
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

    private void SaveMapData()
    {
        MapModel? map = GetModel();

        if (map == null)
        {
            return;
        }

        if (map.Tiles.Count > 0)
        {
            return;
        }

        List<FileModelVO> bankModels = ProjectFiles.GetModels<BankModel>();

        FileModelVO? fileModel = bankModels.Find(t =>
        {
            if (t.Model != null)
            {
                if (t.Model is BankModel bm)
                {
                    return bm.Sprites.Count > 1;
                }
            }

            return false;
        });

        if (fileModel?.Model is not BankModel bank)
        {
            return;
        }

        map.BankID = bank.GUID;

        for (int i = 0; i < 60; i++)
        {
            map.Tiles.Add(new()
            {
                ID = Guid.NewGuid().ToString(),
                FlipHorizontal = false,
                FlipVertical = false,
                PaletteIndex = 0,
                SpriteTileID = bank.Sprites[4].SpriteID ?? string.Empty,
                TileSetID = bank.Sprites[4].TileSetID ?? string.Empty
            });
            map.Tiles.Add(new()
            {
                ID = Guid.NewGuid().ToString(),
                FlipHorizontal = false,
                FlipVertical = false,
                PaletteIndex = 0,
                SpriteTileID = bank.Sprites[6].SpriteID ?? string.Empty,
                TileSetID = bank.Sprites[6].TileSetID ?? string.Empty
            });
        }

        ProjectItem?.FileHandler?.Save();
    }
}
