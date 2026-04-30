using System.Windows.Controls;

namespace GBATool.VOs;

public record SelectionChangedVO(SelectionChangedEventArgs EventArgs) : EventVO
{
}

