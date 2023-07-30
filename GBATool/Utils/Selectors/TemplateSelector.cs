using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;

namespace GBATool.Utils
{
    internal class TemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; } = new();
        public DataTemplate NewButtonTemplate { get; set; } = new();

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == CollectionView.NewItemPlaceholder)
            {
                return NewButtonTemplate;
            }
            else
            {
                return ItemTemplate;
            }
        }
    }
}
