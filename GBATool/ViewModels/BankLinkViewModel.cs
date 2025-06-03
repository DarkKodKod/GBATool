using ArchitectureLibrary.ViewModel;
using GBATool.Commands.Banks;

namespace GBATool.ViewModels;

public class BankLinkViewModel : ViewModel
{
    private string? _caption;
    private string? _tileSetId;

    public string? Caption
    {
        get => _caption;
        set
        {
            _caption = value;

            OnPropertyChanged(nameof(Caption));
        }
    }

    public string? TileSetId
    {
        get => _tileSetId;
        set
        {
            _tileSetId = value;

            OnPropertyChanged(nameof(TileSetId));
        }
    }

    #region Commands
    public SelectTileSetCommand SelectTileSetCommand { get; } = new();
    #endregion
}
