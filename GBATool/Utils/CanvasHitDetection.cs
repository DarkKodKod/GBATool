using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GBATool.Utils;

public class CanvasHitDetection<Element>(EllipseGeometry hitArea, Canvas canvas) where Element : DependencyObject, new()
{
    private readonly EllipseGeometry _hitArea = hitArea;
    private readonly Canvas _canvas = canvas;

    public List<Element> HitTest()
    {
        List<Element> hitList = [];

        VisualTreeHelper.HitTest(_canvas,
                new HitTestFilterCallback(o =>
                {
                    if (o.GetType() == typeof(Element))
                        return HitTestFilterBehavior.ContinueSkipChildren;
                    else
                        return HitTestFilterBehavior.Continue;
                }),
                new HitTestResultCallback(result =>
                {
                    if (result?.VisualHit is Element)
                    {
                        IntersectionDetail intersectionDetail = ((GeometryHitTestResult)result).IntersectionDetail;
                        if (intersectionDetail == IntersectionDetail.FullyContains)
                        {
                            hitList.Add((Element)result.VisualHit);
                            return HitTestResultBehavior.Continue;
                        }
                        else if (intersectionDetail != IntersectionDetail.Intersects &&
                            intersectionDetail != IntersectionDetail.FullyInside)
                        {
                            return HitTestResultBehavior.Stop;
                        }
                    }

                    return HitTestResultBehavior.Continue;
                }),
                new GeometryHitTestParameters(_hitArea));

        return hitList;
    }
}
