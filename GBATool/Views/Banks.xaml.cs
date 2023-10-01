using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.ViewModels;
using GBATool.VOs;
using System.Windows.Controls;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for Banks.xaml
    /// </summary>
    public partial class Banks : UserControl, ICleanable
    {
        public Banks()
        {
            InitializeComponent();

            slSprites.OnActivate();

            SignalManager.Get<AddNewTileSetLinkSignal>().Listener += OnAddNewTileSetLink;
            SignalManager.Get<CleanupTileSetLinksSignal>().Listener += OnCleanupTileSetLinks;
        }

        private void OnCleanupTileSetLinks()
        {
            if (DataContext is BanksViewModel viewModel)
            {
                if (!viewModel.IsActive)
                {
                    return;
                }
            }

            wpLinks.Children.Clear();
        }

        private void OnAddNewTileSetLink(BankLinkVO vo)
        {
            if (DataContext is BanksViewModel viewModel)
            {
                if (!viewModel.IsActive)
                {
                    return;
                }

                BankLinkView link = new();

                ((BankLinkViewModel)link.DataContext).Caption = vo.Caption;
                ((BankLinkViewModel)link.DataContext).TileSetId = vo.Id;

                _ = wpLinks.Children.Add(link);
            }
        }

        public void CleanUp()
        {
            SignalManager.Get<AddNewTileSetLinkSignal>().Listener -= OnAddNewTileSetLink;
            SignalManager.Get<CleanupTileSetLinksSignal>().Listener -= OnCleanupTileSetLinks;

            slSprites.OnDeactivate();
        }
    }
}
