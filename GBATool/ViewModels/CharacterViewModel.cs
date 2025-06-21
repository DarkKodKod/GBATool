using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands.Banks;
using GBATool.Commands.Character;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Views;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace GBATool.ViewModels;

public class CharacterViewModel : ItemViewModel
{
    private ObservableCollection<ActionTabItem>? _tabs;
    private bool _doNotSavePalettes = false;
    private FileModelVO[]? _palettes;
    private int _selectedPalette = -1;
    private int _selectedIndex = 0;
    private int[] _indices = new int[16];

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

    public int[] Indices
    {
        get { return _indices; }
        set
        {
            _indices = value;

            OnPropertyChanged(nameof(Indices));
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

        foreach (ActionTabItem tab in Tabs)
        {
            if (tab.Content?.DataContext is AActivate vm)
            {
                vm.OnActivate();
            }
        }

        _doNotSavePalettes = true;

        CharacterModel? model = GetModel();

        if (model == null)
            return;

        SelectedIndex = model.PaletteIndex;

        LoadPalette(model);

        _doNotSavePalettes = false;
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        #region Signals
        SignalManager.Get<AnimationTabDeletedSignal>().Listener -= OnAnimationTabDeleted;
        SignalManager.Get<AnimationTabNewSignal>().Listener -= OnAnimationTabNew;
        SignalManager.Get<RenamedAnimationTabSignal>().Listener -= OnRenamedAnimationTab;
        SignalManager.Get<SwitchCharacterFrameViewSignal>().Listener += OnSwitchCharacterFrameView;
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

        if (_doNotSavePalettes)
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

    public void PopulateTabs()
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

            AddNewAnimation(animation.ID, animation.Name);
        }
    }

    private void AddNewAnimation(string id, string animationName)
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

        if (!model.Animations.ContainsKey(id))
        {
            model.Animations.Add(id, new CharacterAnimation());
        }
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

    private void UpdateAndSaveIndex(int newValue)
    {
        CharacterModel? model = GetModel();

        if (model == null)
            return;

        model.PaletteIndex = newValue;

        if (!_doNotSavePalettes)
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

        if (!_doNotSavePalettes)
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
