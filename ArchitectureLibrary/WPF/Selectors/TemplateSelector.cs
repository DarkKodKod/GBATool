using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ArchitectureLibrary.WPF.Selectors;

public class TemplateSelector : DataTemplateSelector
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
