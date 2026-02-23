// 

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace AutoHitCounter.Behaviors;

public class InsertionAdorner : Adorner
{
    private static readonly Pen LinePen;
    private static readonly Brush CircleBrush;

    private const double CircleRadius = 3.5;

    private readonly bool _drawAbove;

    static InsertionAdorner()
    {
        var color = Color.FromRgb(0xC8, 0x50, 0xC0);
        CircleBrush = new SolidColorBrush(color);
        CircleBrush.Freeze();

        LinePen = new Pen(CircleBrush, 2.0) { DashCap = PenLineCap.Round };
        LinePen.Freeze();
    }

    public InsertionAdorner(UIElement adornedElement, bool drawAbove)
        : base(adornedElement)
    {
        _drawAbove = drawAbove;
        IsHitTestVisible = false;
    }

    protected override void OnRender(DrawingContext dc)
    {
        var adornedRect = new Rect(AdornedElement.RenderSize);

        var y = _drawAbove ? adornedRect.Top : adornedRect.Bottom;

        var left = new Point(adornedRect.Left + 2, y);
        var right = new Point(adornedRect.Right - 2, y);
        
        dc.DrawEllipse(CircleBrush, null, left, CircleRadius, CircleRadius);
        dc.DrawEllipse(CircleBrush, null, right, CircleRadius, CircleRadius);
        
        dc.DrawLine(LinePen, left, right);
    }
}