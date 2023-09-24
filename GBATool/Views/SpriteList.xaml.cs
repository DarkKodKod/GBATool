using ArchitectureLibrary.Signals;
using GBATool.Commands;
using GBATool.Signals;
using GBATool.VOs;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for SpriteList.xaml
    /// </summary>
    public partial class SpriteList : UserControl, INotifyPropertyChanged
    {
        private SpriteVO _selectedSprite = new();
        private List<SpriteVO> _spriteModels = new();
        private Visibility _deleteButton = Visibility.Visible;

        public event PropertyChangedEventHandler? PropertyChanged;

        #region Commands
        public DeleteSpriteCommand DeleteSpriteCommand { get; } = new();
        #endregion

        #region get/set
        public SpriteVO SelectedSprite
        {
            get => _selectedSprite;
            set
            {
                _selectedSprite = value;
                OnPropertyChanged("SelectedSprite");
            }
        }

        public List<SpriteVO> SpriteModels
        {
            get => _spriteModels;
            set
            {
                _spriteModels = value;

                OnPropertyChanged("SpriteModels");
            }
        }

        public Visibility DeleteButton
        {
            get => _deleteButton;
            set
            {
                _deleteButton = value;

                OnPropertyChanged("DeleteButton");
            }
        }
        #endregion

        public SpriteList()
        {
            InitializeComponent();
        }

        public void OnActivate()
        {
            SignalManager.Get<AddSpriteSignal>().Listener += OnAddSprite;
            SignalManager.Get<UpdateSpriteListSignal>().Listener += OnUpdateSpriteList;
            SignalManager.Get<DeletingSpriteSignal>().Listener += OnDeletingSprite;
        }

        public void OnDeactivate()
        {
            SignalManager.Get<AddSpriteSignal>().Listener -= OnAddSprite;
            SignalManager.Get<DeletingSpriteSignal>().Listener -= OnDeletingSprite;
            SignalManager.Get<UpdateSpriteListSignal>().Listener -= OnUpdateSpriteList;
        }

        private void OnDeletingSprite(SpriteVO sprite)
        {
            foreach (SpriteVO item in SpriteModels)
            {
                if (item.SpriteID == sprite.SpriteID)
                {
                    SpriteModels.Remove(item);

                    OnUpdateSpriteList();

                    SignalManager.Get<ConfirmSpriteDeletionSignal>().Dispatch(sprite);

                    return;
                }
            }
        }

        public void OnAddSprite(SpriteVO sprite)
        {
            SpriteModels.Add(sprite);
        }

        private void OnUpdateSpriteList()
        {
            lvSprites.Items.Refresh();
        }

        protected virtual void OnPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }
}
