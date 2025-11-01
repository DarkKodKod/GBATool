using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands.Banks;
using GBATool.Commands.Character;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Views;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace GBATool.ViewModels;

public class CharacterViewModel : ItemViewModel
{
    private ObservableCollection<ActionTabItem>? _tabs;
    private bool _doNotSave = false;
    private FileModelVO[]? _palettes;
    private int _selectedPalette = -1;
    private int _selectedIndex = 0;
    private Priority _selectedPriority = Priority.Highest;
    private int[] _indices = new int[16];
    private int _relativeOriginX;
    private int _relativeOriginY;
    private int _spriteBase;
    private int _verticalAxis;

    #region Commands
    public FileModelVOSelectionChangedCommand FileModelVOSelectionChangedCommand { get; } = new();
    public CharacterCloseTabCommand CharacterCloseTabCommand { get; } = new();
    public CharacterNewTabCommand CharacterNewTabCommand { get; } = new();
    #endregion

    #region get/set
    public ObservableCollection<ActionTabItem> Tabs
    {
        get
        {
            if (_tabs == null)
            {
                _tabs = [];
                IEditableCollectionView itemsView = (IEditableCollectionView)CollectionViewSource.GetDefaultView(_tabs);
                itemsView.NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtEnd;
            }

            return _tabs;
        }
    }

    public int RelativeOriginX
    {
        get => _relativeOriginX;
        set
        {
            _relativeOriginX = value;

            OnPropertyChanged(nameof(RelativeOriginX));

            UpdateOriginPosition(value, RelativeOriginY);
        }
    }

    public int RelativeOriginY
    {
        get => _relativeOriginY;
        set
        {
            _relativeOriginY = value;

            OnPropertyChanged(nameof(RelativeOriginY));

            UpdateOriginPosition(RelativeOriginX, value);
        }
    }

    public int SpriteBase
    {
        get => _spriteBase;
        set
        {
            _spriteBase = value;

            OnPropertyChanged(nameof(SpriteBase));

            UpdateSpriteBase(value);
        }
    }

    public int VerticalAxis
    {
        get => _verticalAxis;
        set
        {
            _verticalAxis = value;

            OnPropertyChanged(nameof(VerticalAxis));

            UpdateVerticalAxis(value);
        }
    }

    public int[] Indices
    {
        get { return _indices; }
        set
        {
            _indices = value;

            OnPropertyChanged(nameof(Indices));
        }
    }

    public Priority SelectedPriority
    {
        get { return _selectedPriority; }
        set
        {
            if (_selectedPriority != value)
            {
                _selectedPriority = value;

                UpdateAndSavePriority(value);
            }

            OnPropertyChanged(nameof(SelectedPriority));
        }
    }

    public int SelectedIndex
    {
        get { return _selectedIndex; }
        set
        {
            if (_selectedIndex != value)
            {
                _selectedIndex = value;

                UpdateAndSaveIndex(value);
            }

            OnPropertyChanged(nameof(SelectedIndex));
        }
    }

    public FileModelVO[]? Palettes
    {
        get => _palettes;
        set
        {
            _palettes = value;

            OnPropertyChanged(nameof(Palettes));
        }
    }

    public int SelectedPalette
    {
        get => _selectedPalette;
        set
        {
            if (_selectedPalette != value)
            {
                _selectedPalette = value;

                UpdateAndSavePalette(value);
            }

            OnPropertyChanged(nameof(SelectedPalette));
        }
    }
    #endregion

    public override void OnActivate()
    {
        base.OnActivate();

        List<FileModelVO> list =
        [
            new()
            {
                Index = -1,
                Name = "None",
                Model = null
            },
            .. ProjectFiles.GetModels<PaletteModel>(),
        ];

        Palettes = [.. list];

        Indices = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];

        #region Signals
        SignalManager.Get<AnimationTabDeletedSignal>().Listener += OnAnimationTabDeleted;
        SignalManager.Get<AnimationTabNewSignal>().Listener += OnAnimationTabNew;
        SignalManager.Get<RenamedAnimationTabSignal>().Listener += OnRenamedAnimationTab;
        SignalManager.Get<SwitchCharacterFrameViewSignal>().Listener += OnSwitchCharacterFrameView;
        SignalManager.Get<ColorPaletteControlSelectedSignal>().Listener += OnColorPaletteControlSelected;
        SignalManager.Get<UpdateCharacterImageSignal>().Listener += OnUpdateCharacterImage;
        #endregion

        PopulateTabs();
        ActivateTabs();

        _doNotSave = true;

        CharacterModel? model = GetModel();

        if (model == null)
            return;

        VerticalAxis = model.VerticalAxis;
        Point origin = model.RelativeOrigin;

        SpriteBase = model.SpriteBase;

        RelativeOriginX = (int)origin.X;
        RelativeOriginY = (int)origin.Y;

        SelectedIndex = model.PaletteIndex;
        SelectedPriority = model.Priority;

        LoadPalette(model);

        _doNotSave = false;
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        #region Signals
        SignalManager.Get<AnimationTabDeletedSignal>().Listener -= OnAnimationTabDeleted;
        SignalManager.Get<AnimationTabNewSignal>().Listener -= OnAnimationTabNew;
        SignalManager.Get<RenamedAnimationTabSignal>().Listener -= OnRenamedAnimationTab;
        SignalManager.Get<SwitchCharacterFrameViewSignal>().Listener -= OnSwitchCharacterFrameView;
        SignalManager.Get<ColorPaletteControlSelectedSignal>().Listener -= OnColorPaletteControlSelected;
        SignalManager.Get<UpdateCharacterImageSignal>().Listener -= OnUpdateCharacterImage;
        #endregion

        foreach (ActionTabItem tab in Tabs)
        {
            if (tab.Content is CharacterAnimationView animationView)
            {
                animationView.OnDeactivate();
            }

            if (tab.Content?.DataContext is AActivate vm)
            {
                vm.OnDeactivate();
            }
        }
    }

    private void OnUpdateCharacterImage()
    {
        foreach (ActionTabItem tab in Tabs)
        {
            if (tab.FramesView is CharacterAnimationView frameView)
            {
                if (frameView.DataContext is CharacterAnimationViewModel viewModel)
                {
                    viewModel.LoadFrameImage();
                }

                foreach (CharacterFrameView frame in frameView.FrameViewList)
                {
                    frame.OnActivate();
                }
            }
        }
    }

    private void OnColorPaletteControlSelected(int[] colors)
    {
        if (!IsActive)
        {
            return;
        }

        if (_doNotSave)
        {
            return;
        }

        SignalManager.Get<UpdateCharacterImageSignal>().Dispatch();
    }

    private void OnSwitchCharacterFrameView(string tabId, string frameId, int frameIndex)
    {
        if (!IsActive)
        {
            return;
        }

        foreach (ActionTabItem tab in Tabs)
        {
            if (tab.ID == tabId)
            {
                tab.SwapContent(tabId, frameId, frameIndex);

                return;
            }
        }
    }

    private void OnRenamedAnimationTab(string obj)
    {
        if (!IsActive)
        {
            return;
        }

        Save();
    }

    private void OnAnimationTabNew()
    {
        if (!IsActive)
        {
            return;
        }

        string newTabName = "Animation_" + (Tabs.Count + 1);

        AddNewAnimation(Guid.NewGuid().ToString(), newTabName);

        Save();
    }

    private void OnAnimationTabDeleted(ActionTabItem tabItem)
    {
        if (!IsActive)
        {
            return;
        }

        foreach (ActionTabItem tab in Tabs)
        {
            if (tab == tabItem)
            {
                _ = Tabs.Remove(tab);

                CharacterModel? model = GetModel();
                model?.Animations.Remove(tabItem.ID);

                Save();

                return;
            }
        }
    }

    private void UpdateOriginPosition(int posX, int posY)
    {
        CharacterModel? model = GetModel();

        if (model == null)
        {
            return;
        }

        SignalManager.Get<UpdateOriginPositionSignal>().Dispatch(posX, posY);

        Point newPos = new(posX, posY);

        model.RelativeOrigin = newPos;

        if (_doNotSave)
            return;

        Save();
    }

    private void UpdateSpriteBase(int value)
    {
        CharacterModel? model = GetModel();

        if (model == null)
        {
            return;
        }

        SignalManager.Get<UpdateSpriteBaseSignal>().Dispatch(value);

        model.SpriteBase = value;

        if (_doNotSave)
            return;

        Save();
    }

    private void UpdateVerticalAxis(int value)
    {
        CharacterModel? model = GetModel();

        if (model == null)
        {
            return;
        }

        SignalManager.Get<UpdateVerticalAxisSignal>().Dispatch(value);

        model.VerticalAxis = value;

        if (_doNotSave)
            return;

        Save();
    }

    private void Save()
    {
        CharacterModel? model = GetModel();

        if (model == null)
        {
            return;
        }

        foreach (ActionTabItem tab in Tabs)
        {
            model.Animations[tab.ID].ID = tab.ID;
            model.Animations[tab.ID].Name = tab.Header;

            CharacterAnimationView? view = tab.FramesView as CharacterAnimationView;

            if (view?.DataContext is CharacterAnimationViewModel viewModel)
            {
                model.Animations[tab.ID].Speed = viewModel.Speed;
            }
        }

        ProjectItem?.FileHandler?.Save();
    }

    public CharacterModel? GetModel()
    {
        return ProjectItem?.FileHandler?.FileModel is CharacterModel model ? model : null;
    }

    private void PopulateTabs()
    {
        CharacterModel? model = GetModel();

        if (model == null)
        {
            return;
        }

        foreach (var item in model.Animations)
        {
            CharacterAnimation animation = item.Value;

            if (string.IsNullOrEmpty(animation.ID))
            {
                continue;
            }

            AddAnimation(animation.ID, animation.Name);
        }
    }

    private void ActivateTabs()
    {
        foreach (ActionTabItem tab in Tabs)
        {
            if (tab.Content is CharacterAnimationView animationView)
            {
                animationView.OnActivate();
            }

            if (tab.Content?.DataContext is AActivate vm)
            {
                vm.OnActivate();
            }
        }
    }

    private void ActivateTab(string tabID)
    {
        foreach (ActionTabItem tab in Tabs)
        {
            if (tabID != tab.ID)
            {
                continue;
            }

            if (tab.Content is CharacterAnimationView animationView)
            {
                animationView.OnActivate();
                return;
            }
        }
    }

    private void AddAnimation(string id, string animationName)
    {
        CharacterModel? model = GetModel();

        if (model == null)
            return;

        CharacterAnimationView animationView = new();
        ((CharacterAnimationViewModel)animationView.DataContext).CharacterModel = model;
        ((CharacterAnimationViewModel)animationView.DataContext).FileHandler = ProjectItem?.FileHandler;
        ((CharacterAnimationViewModel)animationView.DataContext).TabID = id;

        CharacterFrameEditorView frameView = new();
        ((CharacterFrameEditorViewModel)frameView.DataContext).CharacterModel = model;
        ((CharacterFrameEditorViewModel)frameView.DataContext).FileHandler = ProjectItem?.FileHandler;
        ((CharacterFrameEditorViewModel)frameView.DataContext).TabID = id;

        Tabs.Add(new ActionTabItem
        {
            ID = id,
            Header = animationName,
            Content = animationView,
            FramesView = animationView,
            PixelsView = frameView
        });
    }

    private void AddNewAnimation(string id, string animationName)
    {
        CharacterModel? model = GetModel();

        if (model == null)
            return;

        AddAnimation(id, animationName);

        if (!model.Animations.ContainsKey(id))
        {
            model.Animations.Add(id, new CharacterAnimation());
        }

        ActivateTab(id);
    }

    private void LoadPalette(CharacterModel model)
    {
        if (string.IsNullOrEmpty(model.PaletteID))
            return;

        PaletteModel? paletteModel = ProjectFiles.GetModel<PaletteModel>(model.PaletteID);
        if (paletteModel == null)
        {
            SelectedPalette = -1;
        }
        else
        {
            for (int i = 0; i < Palettes?.Length; ++i)
            {
                FileModelVO item = Palettes[i];

                if (item.Model == null)
                {
                    continue;
                }

                if (item.Model.GUID == model.PaletteID)
                {
                    SelectedPalette = i - 1;
                    break;
                }
            }
        }
    }

    private void UpdateAndSavePriority(Priority newValue)
    {
        CharacterModel? model = GetModel();

        if (model == null)
            return;

        model.Priority = newValue;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSaveIndex(int newValue)
    {
        CharacterModel? model = GetModel();

        if (model == null)
            return;

        model.PaletteIndex = newValue;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSavePalette(int newValue)
    {
        CharacterModel? model = GetModel();

        if (model == null)
            return;

        if (Palettes == null)
            return;

        if (newValue == -1)
        {
            model.PaletteID = string.Empty;
        }
        else
        {
            AFileModel? fileModel = Palettes[newValue + 1].Model;

            if (fileModel != null)
            {
                model.PaletteID = fileModel.GUID;
            }
        }

        PaletteModel? paletteModel = ProjectFiles.GetModel<PaletteModel>(model.PaletteID);

        if (paletteModel != null)
        {
            SetPaletteWithColors(paletteModel);
        }
        else
        {
            SetPaletteEmpty();
        }

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private static void SetPaletteEmpty()
    {
        SignalManager.Get<CleanColorPaletteControlSelectedSignal>().Dispatch();
    }

    private static void SetPaletteWithColors(PaletteModel paletteModel)
    {
        SignalManager.Get<ColorPaletteControlSelectedSignal>().Dispatch(paletteModel.Colors);
    }
}
