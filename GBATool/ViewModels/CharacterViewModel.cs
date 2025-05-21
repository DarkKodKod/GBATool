using ArchitectureLibrary.Signals;
using GBATool.Commands.Banks;
using GBATool.Commands.Character;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
using GBATool.VOs;
using System.Collections.Generic;

namespace GBATool.ViewModels;

public class CharacterViewModel : ItemViewModel
{
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
    public int[] Indices
    {
        get { return _indices; }
        set
        {
            _indices = value;

            OnPropertyChanged("Indices");
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

            OnPropertyChanged("SelectedIndex");
        }
    }

    public FileModelVO[]? Palettes
    {
        get => _palettes;
        set
        {
            _palettes = value;

            OnPropertyChanged("Palettes");
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

            OnPropertyChanged("SelectedPalette");
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
    }

    public CharacterModel? GetModel()
    {
        return ProjectItem?.FileHandler?.FileModel is CharacterModel model ? model : null;
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
