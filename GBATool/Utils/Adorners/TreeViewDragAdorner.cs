using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;

namespace GBATool.Utils.Adorners
{
    public class TreeViewDragAdorner : Adorner
    {
        private readonly ContentPresenter _contentPresenter;
        private readonly AdornerLayer _adornerLayer;
        private double _leftOffset;
        private double _topOffset;

        public TreeViewDragAdorner(object data, DataTemplate dataTemplate, UIElement adornedElement, AdornerLayer adornerLayer) : base(adornedElement)
        {
            IsHitTestVisible = false;

            _adornerLayer = adornerLayer;

            _contentPresenter = new() { Content = data, ContentTemplate = dataTemplate, Opacity = 0.75 };

            _adornerLayer.Add(this);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _contentPresenter.Measure(constraint);

            return _contentPresenter.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _contentPresenter.Arrange(new Rect(finalSize));

            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _contentPresenter;
        }

        protected override int VisualChildrenCount => 1;

        public void UpdatePosition(double left, double top)
        {
            _leftOffset = left;
            _topOffset = top;

            _adornerLayer?.Update(AdornedElement);
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(_leftOffset, _topOffset));

            return result;
        }

        public void Destroy()
        {
            _adornerLayer.Remove(this);
        }
    }
}
