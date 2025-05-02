using Microsoft.Xaml.Behaviors;
using System.Reflection;
using System.Windows;

namespace GBATool.Utils.Behaviors;

public class RedirectRoutedEventBehavior : Behavior<UIElement>
{
    public static readonly DependencyProperty RedirectTargetProperty =
        DependencyProperty.Register("RedirectTarget", typeof(UIElement),
            typeof(RedirectRoutedEventBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty RoutedEventProperty =
        DependencyProperty.Register("RoutedEvent", typeof(RoutedEvent), typeof(RedirectRoutedEventBehavior),
            new PropertyMetadata(null, OnRoutedEventChanged));

    public UIElement RedirectTarget
    {
        get => (UIElement)GetValue(RedirectTargetProperty);
        set => SetValue(RedirectTargetProperty, value);
    }

    public RoutedEvent RoutedEvent
    {
        get => (RoutedEvent)GetValue(RoutedEventProperty);
        set => SetValue(RoutedEventProperty, value);
    }

    private static MethodInfo? MemberwiseCloneMethod { get; }
        = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

    private static void OnRoutedEventChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((RedirectRoutedEventBehavior)d).OnRoutedEventChanged((RoutedEvent)e.OldValue, (RoutedEvent)e.NewValue);
    }

    private void OnRoutedEventChanged(RoutedEvent oldValue, RoutedEvent newValue)
    {
        if (AssociatedObject == null)
        {
            return;
        }

        if (oldValue != null)
        {
            AssociatedObject.RemoveHandler(oldValue, new RoutedEventHandler(EventHandler));
        }

        if (newValue != null)
        {
            AssociatedObject.AddHandler(newValue, new RoutedEventHandler(EventHandler));
        }
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (RoutedEvent != null)
        {
            AssociatedObject.AddHandler(RoutedEvent, new RoutedEventHandler(EventHandler));
        }
    }

    protected override void OnDetaching()
    {
        if (RoutedEvent != null)
        {
            AssociatedObject.RemoveHandler(RoutedEvent, new RoutedEventHandler(EventHandler));
        }

        base.OnDetaching();
    }

    private static RoutedEventArgs? CloneEvent(RoutedEventArgs e)
    {
        RoutedEventArgs? eventArg = (RoutedEventArgs?)MemberwiseCloneMethod?.Invoke(e, null);

        return eventArg;
    }

    private void EventHandler(object sender, RoutedEventArgs e)
    {
        RoutedEventArgs? newEvent = CloneEvent(e);
        e.Handled = true;

        if (newEvent != null && RedirectTarget != null)
        {
            newEvent.Source = RedirectTarget;
            RedirectTarget.RaiseEvent(newEvent);
        }
    }
}
