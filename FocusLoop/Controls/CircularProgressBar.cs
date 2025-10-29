using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace FocusLoop.Controls;

public class CircularProgressBar : Control
{
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<CircularProgressBar, double>(nameof(Progress), 0.0);

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<CircularProgressBar, double>(nameof(StrokeThickness), 8.0);

    public static readonly StyledProperty<IBrush?> BackgroundStrokeProperty =
        AvaloniaProperty.Register<CircularProgressBar, IBrush?>(nameof(BackgroundStroke), Brushes.LightGray);

    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<CircularProgressBar, IBrush?>(nameof(Stroke), Brushes.DeepSkyBlue);

    public static readonly StyledProperty<int> DashCountProperty =
        AvaloniaProperty.Register<CircularProgressBar, int>(nameof(DashCount), 24);

    public static readonly StyledProperty<double> DashFillProperty =
        AvaloniaProperty.Register<CircularProgressBar, double>(nameof(DashFill), 0.35); // Portion of each slot drawn [0..1]

    // Relative lengths (fraction of control size) for line-style dashes
    public static readonly StyledProperty<double> DashLengthRatioProperty =
        AvaloniaProperty.Register<CircularProgressBar, double>(nameof(DashLengthRatio), 0.08);

    public static readonly StyledProperty<double> ActiveDashLengthRatioProperty =
        AvaloniaProperty.Register<CircularProgressBar, double>(nameof(ActiveDashLengthRatio), 0.12);

    public static readonly StyledProperty<IBrush?> RimStrokeProperty =
        AvaloniaProperty.Register<CircularProgressBar, IBrush?>(nameof(RimStroke), new SolidColorBrush(Color.FromUInt32(0xFFEFEFEF)));

    public static readonly StyledProperty<double> RimThicknessProperty =
        AvaloniaProperty.Register<CircularProgressBar, double>(nameof(RimThickness), 1.0);

    public static readonly StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<CircularProgressBar, double>(nameof(StartAngle), -90.0); // 12 o'clock

    // Highlight a number of leading ticks (from StartAngle, clockwise)
    public static readonly StyledProperty<int> LeadingHighlightCountProperty =
        AvaloniaProperty.Register<CircularProgressBar, int>(nameof(LeadingHighlightCount), 0);

    public static readonly StyledProperty<IBrush?> LeadingHighlightBrushProperty =
        AvaloniaProperty.Register<CircularProgressBar, IBrush?>(nameof(LeadingHighlightBrush), null);

    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public IBrush? BackgroundStroke
    {
        get => GetValue(BackgroundStrokeProperty);
        set => SetValue(BackgroundStrokeProperty, value);
    }

    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public int DashCount
    {
        get => GetValue(DashCountProperty);
        set => SetValue(DashCountProperty, value);
    }

    public double DashFill
    {
        get => GetValue(DashFillProperty);
        set => SetValue(DashFillProperty, value);
    }

    public double DashLengthRatio
    {
        get => GetValue(DashLengthRatioProperty);
        set => SetValue(DashLengthRatioProperty, value);
    }

    public double ActiveDashLengthRatio
    {
        get => GetValue(ActiveDashLengthRatioProperty);
        set => SetValue(ActiveDashLengthRatioProperty, value);
    }

    public IBrush? RimStroke
    {
        get => GetValue(RimStrokeProperty);
        set => SetValue(RimStrokeProperty, value);
    }

    public double RimThickness
    {
        get => GetValue(RimThicknessProperty);
        set => SetValue(RimThicknessProperty, value);
    }

    public double StartAngle
    {
        get => GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    // Extra spacing between the outer rim and the tick ring (in device pixels)
    public static readonly StyledProperty<double> InnerPaddingProperty =
        AvaloniaProperty.Register<CircularProgressBar, double>(nameof(InnerPadding), 12.0);

    public double InnerPadding
    {
        get => GetValue(InnerPaddingProperty);
        set => SetValue(InnerPaddingProperty, value);
    }

    public int LeadingHighlightCount
    {
        get => GetValue(LeadingHighlightCountProperty);
        set => SetValue(LeadingHighlightCountProperty, value);
    }

    public IBrush? LeadingHighlightBrush
    {
        get => GetValue(LeadingHighlightBrushProperty);
        set => SetValue(LeadingHighlightBrushProperty, value);
    }

    static CircularProgressBar()
    {
        AffectsRender<CircularProgressBar>(ProgressProperty,
            StrokeThicknessProperty,
            BackgroundStrokeProperty,
            StrokeProperty,
            StartAngleProperty,
            DashCountProperty,
            DashFillProperty,
            DashLengthRatioProperty,
            ActiveDashLengthRatioProperty,
            RimStrokeProperty,
            RimThicknessProperty,
            InnerPaddingProperty,
            LeadingHighlightCountProperty,
            LeadingHighlightBrushProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        var size = Math.Min(bounds.Width, bounds.Height);
        if (size <= 0)
            return;

        var thickness = StrokeThickness;
        var maxStroke = Math.Max(thickness, RimThickness);
        var radius = (size - maxStroke) / 2.0;
        if (radius <= 0)
            return;

        var center = bounds.Center;

        // Thin outer rim
        if (RimStroke is { } rim)
        {
            var rimGeom = new StreamGeometry();
            using (var g = rimGeom.Open())
            {
                var start = PointOnCircle(center, radius, 0);
                g.BeginFigure(start, false);
                var mid = PointOnCircle(center, radius, 180);
                g.ArcTo(mid, new Size(radius, radius), 0, true, SweepDirection.Clockwise);
                g.ArcTo(start, new Size(radius, radius), 0, true, SweepDirection.Clockwise);
                g.EndFigure(false);
            }
            var pen = new Pen(rim, RimThickness);
            context.DrawGeometry(null, pen, rimGeom);
        }

        // Draw segmented background dashes
        var count = Math.Max(1, DashCount);
        var slot = 360.0 / count;
        // line segments: length set via ratios but clamped per slot to avoid overlap (considering round caps)
        var reqInactiveLen = size * Math.Clamp(DashLengthRatio, 0.0, 1.0);
        var reqActiveLen = size * Math.Clamp(ActiveDashLengthRatio, 0.0, 1.0);
        // center these lines slightly inside the rim
        var padding = Math.Max(0, InnerPadding);
        var lineRadius = Math.Max(0, radius - RimThickness - padding - (thickness / 2.0));
        var circumferenceAtLine = 2 * Math.PI * lineRadius;
        var slotLen = circumferenceAtLine / count;
        var minGap = 2.0; // pixels of gap between dashes
        var inactiveLen = Math.Max(0, Math.Min(reqInactiveLen, slotLen - thickness - minGap));
        var activeLen = Math.Max(0, Math.Min(reqActiveLen, slotLen - thickness - minGap));

        var cap = PenLineCap.Round;
        var bgPenDefault = new Pen(BackgroundStroke ?? Brushes.LightGray, thickness, lineCap: cap);
        var leadCount = Math.Clamp(LeadingHighlightCount, 0, count);
        var leadBrush = LeadingHighlightBrush ?? Stroke; // if provided, use it; else fall back to active stroke color
        var bgPenLead = new Pen(leadBrush ?? (BackgroundStroke ?? Brushes.LightGray), thickness, lineCap: cap);

        for (var i = 0; i < count; i++)
        {
            var pen = i < leadCount ? bgPenLead : bgPenDefault;
            DrawTickSegment(context, center, lineRadius, StartAngle + i * slot, inactiveLen, pen);
        }

        // Draw progressed dashes on top
        var p = Math.Clamp(Progress, 0.0, 1.0);
        if (p > 0)
        {
            // Highlight rule: any partial progress into a dash highlights that dash fully
            var active = (int)Math.Ceiling(p * count);
            active = Math.Clamp(active, 0, count);

            var fgPenToUse = new Pen(Stroke ?? Brushes.DeepSkyBlue, thickness, lineCap: cap);

            for (var i = 0; i < active && i < count; i++)
            {
                DrawTickSegment(context, center, lineRadius, StartAngle + i * slot, activeLen, fgPenToUse);
            }
        }
    }

    private static Point PointOnCircle(Point center, double radius, double angleDegrees)
    {
        var radians = angleDegrees * Math.PI / 180.0;
        var x = center.X + radius * Math.Cos(radians);
        var y = center.Y + radius * Math.Sin(radians);
        return new Point(x, y);
    }

    private static void DrawTickSegment(DrawingContext ctx, Point center, double radius, double angle, double length, Pen pen)
    {
        if (length <= 0) return;
        var radians = angle * Math.PI / 180.0;
        var pos = new Point(center.X + radius * Math.Cos(radians), center.Y + radius * Math.Sin(radians));
        // Radial direction (pointing toward/away from center)
        var dx = Math.Cos(radians) * (length / 2.0);
        var dy = Math.Sin(radians) * (length / 2.0);
        var p1 = new Point(pos.X - dx, pos.Y - dy);
        var p2 = new Point(pos.X + dx, pos.Y + dy);

        var geom = new StreamGeometry();
        using (var g = geom.Open())
        {
            g.BeginFigure(p1, false);
            g.LineTo(p2);
            g.EndFigure(false);
        }
        ctx.DrawGeometry(null, pen, geom);
    }
}
