using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands.Banks;
using GBATool.Commands.Character;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
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
    private BankModel? _bankModel = null;

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

            OnPropertyChanged(nameof(FileHandler));
        }
    }

    public FileModelVO[]? Banks
    {
        get => _banks;
        set
        {
            _banks = value;

            OnPropertyChanged(nameof(Banks));
        }
    }

    public int SelectedBank
    {
        get => _selectedBank;
        set
        {
            _selectedBank = value;

            OnPropertyChanged(nameof(SelectedBank));
        }
    }

    public string TabID
    {
        get => _tabId;
        set
        {
            _tabId = value;

            OnPropertyChanged(nameof(TabID));

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
    public string FrameID { get; set; } = string.Empty;

    public int FrameIndex
    {
        get => _frameIndex;
        set
        {
            _frameIndex = value;

            OnPropertyChanged(nameof(FrameIndex));
        }
    }

    public BankModel? BankModel
    {
        get => _bankModel;
        set
        {
            _bankModel = value;

            OnPropertyChanged(nameof(BankModel));
        }
    }
    #endregion

    public CharacterFrameEditorViewModel()
    {
        FileModelVO[] filemodelVo = [.. ProjectFiles.GetModels<BankModel>()];

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

    public override void OnActivate()
    {
        base.OnActivate();

        #region Signals
        SignalManager.Get<UpdateCharacterImageSignal>().Listener += OnUpdateCharacterImage;
        SignalManager.Get<FileModelVOSelectionChangedSignal>().Listener += OnFileModelVOSelectionChanged;
        #endregion

        if (Banks?.Length > 0)
        {
            BankModel = Banks[0].Model as BankModel;
        }
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        #region Signals
        SignalManager.Get<UpdateCharacterImageSignal>().Listener -= OnUpdateCharacterImage;
        SignalManager.Get<FileModelVOSelectionChangedSignal>().Listener -= OnFileModelVOSelectionChanged;
        #endregion
    }

    private void OnUpdateCharacterImage()
    {
        // todo
    }

    private void OnFileModelVOSelectionChanged(FileModelVO fileModel)
    {
        SignalManager.Get<CleanUpSpriteListSignal>().Dispatch();

        if (Banks?.Length == 0)
        {
            return;
        }

        if (Banks?[SelectedBank].Model is not BankModel model)
        {
            return;
        }

        if (model == null)
        {
            return;
        }

        BankModel = model;

        SignalManager.Get<SetBankModelToBankViewerSignal>().Dispatch(model);
        SignalManager.Get<RemoveSpriteSelectionFromBank>().Dispatch();
    }
}
