using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GBATool.Utils;

public class CanvasHitDetection<TElement, TGeometry>(TGeometry hitArea, Canvas canvas)
    where TElement : DependencyObject, new()
    where TGeometry : Geometry, new()
{
    private readonly TGeometry _hitArea = hitArea;
    private readonly Canvas _canvas = canvas;

    public List<TElement> HitTest()
    {
        List<TElement> hitList = [];

        VisualTreeHelper.HitTest(_canvas,
                new HitTestFilterCallback(o =>
                {
                    if (o.GetType() == typeof(TElement))
                        return HitTestFilterBehavior.ContinueSkipChildren;
                    else
                        return HitTestFilterBehavior.Continue;
                }),
                new HitTestResultCallback(result =>
                {
                    if (result?.VisualHit is TElement)
                    {
                        IntersectionDetail intersectionDetail = ((GeometryHitTestResult)result).IntersectionDetail;
                        if (intersectionDetail == IntersectionDetail.FullyContains ||
                            intersectionDetail == IntersectionDetail.FullyInside ||
                            intersectionDetail == IntersectionDetail.Intersects)
                        {
                            hitList.Add((TElement)result.VisualHit);
                            return HitTestResultBehavior.Continue;
                        }
                        else
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
