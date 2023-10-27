using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace GBATool.Utils
{
    public static class TextInputBindingManager
    {
        public static readonly DependencyProperty UpdatePropertySourceWhenEnterPressedProperty = DependencyProperty.RegisterAttached("UpdatePropertySourceWhenEnterPressed", typeof(DependencyProperty), typeof(TextInputBindingManager), new PropertyMetadata(null, OnUpdatePropertySourceWhenEnterPressedPropertyChanged));

        public static void SetUpdatePropertySourceWhenEnterPressed(DependencyObject dp, DependencyProperty value)
        {
            dp.SetValue(UpdatePropertySourceWhenEnterPressedProperty, value);
        }

        public static DependencyProperty GetUpdatePropertySourceWhenEnterPressed(DependencyObject dp)
        {
            return (DependencyProperty)dp.GetValue(UpdatePropertySourceWhenEnterPressedProperty);
        }

        private static void OnUpdatePropertySourceWhenEnterPressedPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is not UIElement element)
            {
                return;
            }

            if (e.OldValue != null)
            {
                element.PreviewKeyDown -= HandlePreviewKeyDown;
            }

            if (e.NewValue != null)
            {
                element.PreviewKeyDown += new KeyEventHandler(HandlePreviewKeyDown);
            }
        }

        static void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoUpdateSource(e.Source);
            }
        }

        static void DoUpdateSource(object source)
        {
            if (source is not DependencyObject obj)
            {
                return;
            }

            DependencyProperty property = GetUpdatePropertySourceWhenEnterPressed(obj);

            if (property == null)
            {
                return;
            }

            if (source is not UIElement elt)
            {
                return;
            }

            BindingExpression? binding = BindingOperations.GetBindingExpression(elt, property);

            binding?.UpdateSource();
        }
    }
}
