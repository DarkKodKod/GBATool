using ArchitectureLibrary.ViewModel;
using GBATool.Commands.Banks;
using GBATool.Commands.Character;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GBATool.ViewModels;

public class CharacterFrameEditorViewModel : ViewModel
{
    private FileModelVO[]? _banks;
    private int _selectedBank;
    private string _tabId = string.Empty;
    private int _frameIndex;
    private FileHandler? _fileHandler;

    #region Commands
    public SwitchCharacterFrameViewCommand SwitchCharacterFrameViewCommand { get; } = new();
    public FileModelVOSelectionChangedCommand FileModelVOSelectionChangedCommand { get; } = new();
    #endregion

    #region get/set
    public FileHandler? FileHandler
    {
        get => _fileHandler;
        set
        {
            _fileHandler = value;

            OnPropertyChanged("FileHandler");
        }
    }

    public FileModelVO[]? Banks
    {
        get => _banks;
        set
        {
            _banks = value;

            OnPropertyChanged("Banks");
        }
    }

    public int SelectedBank
    {
        get => _selectedBank;
        set
        {
            _selectedBank = value;

            OnPropertyChanged("SelectedBank");
        }
    }

    public string TabID
    {
        get => _tabId;
        set
        {
            _tabId = value;

            OnPropertyChanged("TabID");

            var model = CharacterModel;

            if (model == null)
                return;

            foreach (var item in model.Animations)
            {
                CharacterAnimation animation = item.Value;

                if (animation.ID == TabID)
                {
                    AnimationID = animation.ID;
                    break;
                }
            }
        }
    }

    public CharacterModel? CharacterModel { get; set; }
    public string AnimationID { get; set; } = string.Empty;

    public int FrameIndex
    {
        get => _frameIndex;
        set
        {
            _frameIndex = value;

            OnPropertyChanged("FrameIndex");
        }
    }
    #endregion

    public CharacterFrameEditorViewModel()
    {
        UpdateDialogInfo();
    }

    private void UpdateDialogInfo()
    {
        FileModelVO[] filemodelVo = ProjectFiles.GetModels<BankModel>().ToArray();

        IEnumerable<FileModelVO> banks = filemodelVo;

        Banks = new FileModelVO[banks.Count()];

        int index = 0;

        foreach (FileModelVO item in banks)
        {
            item.Index = index;

            Banks[index] = item;

            index++;
        }
    }
}
