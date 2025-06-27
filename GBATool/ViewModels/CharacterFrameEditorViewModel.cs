using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands.Banks;
using GBATool.Commands.Character;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GBATool.ViewModels;

public class CharacterFrameEditorViewModel : ViewModel
{
    private FileModelVO[]? _banks;
    private int _selectedBank = -1;
    private string _tabId = string.Empty;
    private int _frameIndex;
    private FileHandler? _fileHandler;
    private BankModel? _bankModel = null;
    private string _selectedFrameSprite = string.Empty;
    private BankImageMetaData? _bankImageMetaData = null;
    private bool _enableOnionSkin;

    #region Commands
    public SwitchCharacterFrameViewCommand SwitchCharacterFrameViewCommand { get; } = new();
    public FileModelVOSelectionChangedCommand FileModelVOSelectionChangedCommand { get; } = new();
    #endregion

    #region get/set
    public string SelectedFrameSprite
    {
        get => _selectedFrameSprite;
        set
        {
            _selectedFrameSprite = value;

            OnPropertyChanged(nameof(SelectedFrameSprite));
        }
    }

    public bool EnableOnionSkin
    {
        get => _enableOnionSkin;
        set
        {
            if (value != _enableOnionSkin)
            {
                _enableOnionSkin = value;

                ModelManager.Get<GBAToolConfigurationModel>().EnableOnionSkin = value;
                ModelManager.Get<GBAToolConfigurationModel>().Save();

                SignalManager.Get<OptionOnionSkinSignal>().Dispatch(value);
            }

            OnPropertyChanged(nameof(EnableOnionSkin));
        }
    }

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
    public string PreviousFrameID { get; set; } = string.Empty;
    public int PreviousFrameIndex { get; set; }

    public BankImageMetaData? BankImageMetaData
    {
        get => _bankImageMetaData;
        set
        {
            _bankImageMetaData = value;

            OnPropertyChanged(nameof(BankImageMetaData));
        }
    }

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
        SignalManager.Get<MouseImageSelectedSignal>().Listener += OnMouseImageSelected;
        SignalManager.Get<SelectFrameSpriteSignal>().Listener += OnSelectFrameSprite;
        SignalManager.Get<AddOrUpdateSpriteIntoCharacterFrameSignal>().Listener += OnAddOrUpdateSpriteIntoCharacterFrame;
        SignalManager.Get<DeleteSpriteFromCharacterFrameSignal>().Listener += OnDeleteSpriteFromCharacterFrame;
        SignalManager.Get<CharacterFrameEditorViewLoadedSignal>().Listener += OnCharacterFRameEditorViewLoaded;
        #endregion

        EnableOnionSkin = ModelManager.Get<GBAToolConfigurationModel>().EnableOnionSkin;
    }

    private void LoadFrameSprites()
    {
        var model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (!model.Animations.TryGetValue(AnimationID, out CharacterAnimation? animation))
        {
            return;
        }

        (_, List<SpriteControlVO>? previousSprites, _) = LoadSpritesFromFrame(animation, PreviousFrameID);

        if (previousSprites?.Count > 0)
        {
            SignalManager.Get<FillWithPreviousFrameSpriteControlsSignal>().Dispatch(previousSprites);
        }

        (BankImageMetaData? metaData, List<SpriteControlVO>? sprites, string bankID) = LoadSpritesFromFrame(animation, FrameID);

        if (metaData != null)
        {
            BankImageMetaData = metaData;

            if (sprites?.Count > 0)
            {
                SignalManager.Get<FillWithSpriteControlsSignal>().Dispatch(sprites);

                SelectBank(bankID);
            }
        }
    }

    private static (BankImageMetaData? meta, List<SpriteControlVO>? sprites, string bankID) LoadSpritesFromFrame(CharacterAnimation animation, string frameID)
    {
        if (!animation.Frames.TryGetValue(frameID, out FrameModel? frame))
        {
            return (null, null, string.Empty);
        }

        BankModel? bankModel = ProjectFiles.GetModel<BankModel>(frame.BankID);
        if (bankModel == null)
        {
            return (null, null, string.Empty);
        }

        BankImageMetaData meteaData = BankUtils.CreateImage(bankModel, true);

        List<SpriteControlVO> sprites = [];

        foreach (KeyValuePair<string, CharacterSprite> item in frame.Tiles)
        {
            if (!meteaData.Sprites.TryGetValue(item.Value.SpriteID, out SpriteInfo? spriteInfo))
            {
                continue;
            }

            if (spriteInfo == null || spriteInfo.BitmapSource == null)
            {
                continue;
            }

            SpriteControlVO sprite = new()
            {
                ID = item.Value.ID,
                Image = Util.GetImageFromWriteableBitmap(spriteInfo.BitmapSource),
                SpriteID = item.Value.SpriteID,
                TileSetID = item.Value.TileSetID,
                BankID = bankModel.GUID,
                Width = item.Value.Width,
                Height = item.Value.Height,
                OffsetX = spriteInfo.OffsetX,
                OffsetY = spriteInfo.OffsetY,
                PositionX = (int)item.Value.Position.X,
                PositionY = (int)item.Value.Position.Y
            };

            sprites.Add(sprite);
        }

        return (meteaData, sprites, frame.BankID);
    }

    private void SelectBank(string bankID)
    {
        for (int i = 0; i < Banks?.Length; i++)
        {
            if (Banks[i].Model is not BankModel bank)
            {
                continue;
            }

            if (bank.GUID == bankID)
            {
                SelectedBank = i;
                return;
            }
        }
    }

    private void OnSelectFrameSprite(SpriteControlVO sprite)
    {
        SelectedFrameSprite = sprite.ID;
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        #region Signals
        SignalManager.Get<UpdateCharacterImageSignal>().Listener -= OnUpdateCharacterImage;
        SignalManager.Get<FileModelVOSelectionChangedSignal>().Listener -= OnFileModelVOSelectionChanged;
        SignalManager.Get<MouseImageSelectedSignal>().Listener -= OnMouseImageSelected;
        SignalManager.Get<SelectFrameSpriteSignal>().Listener -= OnSelectFrameSprite;
        SignalManager.Get<AddOrUpdateSpriteIntoCharacterFrameSignal>().Listener -= OnAddOrUpdateSpriteIntoCharacterFrame;
        SignalManager.Get<DeleteSpriteFromCharacterFrameSignal>().Listener -= OnDeleteSpriteFromCharacterFrame;
        SignalManager.Get<CharacterFrameEditorViewLoadedSignal>().Listener -= OnCharacterFrameEditorViewLoaded;
        #endregion
    }

    private void OnCharacterFrameEditorViewLoaded()
    {
        LoadFrameSprites();
    }

    private void OnAddOrUpdateSpriteIntoCharacterFrame(CharacterSprite sprite, string bankID)
    {
        var model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (!model.Animations.TryGetValue(AnimationID, out CharacterAnimation? animation))
        {
            return;
        }

        if (!animation.Frames.TryGetValue(FrameID, out FrameModel? frame))
        {
            return;
        }

        frame.BankID = bankID;

        if (frame.Tiles.TryGetValue(sprite.ID, out _))
        {
            frame.Tiles[sprite.ID] = sprite;
        }
        else
        {
            frame.Tiles.Add(sprite.ID, sprite);
        }

        CharacterUtils.InvalidateFrameImageFromCache(frame.ID);

        FileHandler?.Save();
    }

    private void OnDeleteSpriteFromCharacterFrame(string spriteID)
    {
        var model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (!model.Animations.TryGetValue(AnimationID, out CharacterAnimation? animation))
        {
            return;
        }

        if (!animation.Frames.TryGetValue(FrameID, out FrameModel? frame))
        {
            return;
        }

        CharacterUtils.InvalidateFrameImageFromCache(frame.ID);

        if (frame.Tiles.Remove(spriteID))
        {
            FileHandler?.Save();
        }
    }

    private void OnUpdateCharacterImage()
    {
        // todo
    }

    private void OnFileModelVOSelectionChanged(FileModelVO fileModel)
    {
        SignalManager.Get<CleanUpSpriteListSignal>().Dispatch();

        if (Banks == null || Banks.Length == 0)
        {
            return;
        }

        if (Banks[SelectedBank].Model is not BankModel model)
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

    private void OnMouseImageSelected(Image imageParent, Point point)
    {
        if (imageParent.Name != "imgFrame")
        {
            return;
        }

        SignalManager.Get<SelectImageControlInFrameViewSignal>().Dispatch(point);
    }
}
