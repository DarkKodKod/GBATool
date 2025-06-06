﻿using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands.FileSystem;
using GBATool.Commands.Utils;
using GBATool.Signals;

namespace GBATool.ViewModels;

public class ImportImageDialogViewModel : ViewModel
{
    public BrowseFileCommand BrowseFileCommand { get; } = new();
    public DispatchSignalCommand<CloseDialogSignal> CloseDialogCommand { get; } = new();
    public ImportImageCommand ImportImageCommand { get; } = new();

    #region get/set
    public string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            OnPropertyChanged(nameof(FilePath));
        }
    }

    public string[] Filters { get; } = new string[14];

    public bool NewFile { get; } = true;
    #endregion

    private string _filePath = "";

    public ImportImageDialogViewModel()
    {
        SignalManager.Get<BrowseFileSuccessSignal>().Listener += OnBrowseFileSuccess;
        SignalManager.Get<CloseDialogSignal>().Listener += OnCloseDialog;

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

        OnPropertyChanged(nameof(Filters));
    }

    private void OnCloseDialog()
    {
        SignalManager.Get<BrowseFileSuccessSignal>().Listener -= OnBrowseFileSuccess;
        SignalManager.Get<CloseDialogSignal>().Listener -= OnCloseDialog;
    }

    private void OnBrowseFileSuccess(string filePath, bool newFile) => FilePath = filePath;
}
