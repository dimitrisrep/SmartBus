using System.Drawing.Drawing2D;

namespace SmartTouristBus.UI;

public sealed class LaneIndicator : Control
{
    private double _offset;

    public double Offset
    {
        get => _offset;
        set
        {
            _offset = Math.Clamp(value, -1, 1);
            Invalidate();
        }
    }

    public LaneIndicator()
    {
        DoubleBuffered = true;
        Height = 135;
        Dock = DockStyle.Fill;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.FromArgb(46, 54, 61));

        using var lanePen = new Pen(Color.White, 4) { DashStyle = DashStyle.Dash };
        g.DrawLine(lanePen, Width * 0.28f, 0, Width * 0.12f, Height);
        g.DrawLine(lanePen, Width * 0.72f, 0, Width * 0.88f, Height);

        var carX = Width / 2f - 27 + (float)(_offset * Width * 0.26);
        var carRect = new RectangleF(carX, Height - 76, 54, 66);
        using var carBrush = new SolidBrush(Math.Abs(_offset) > 0.7 ? Theme.Coral : Theme.Cyan);
        g.FillRoundedRectangle(carBrush, Rectangle.Round(carRect), 8);
        g.FillRectangle(Brushes.LightBlue, carRect.X + 10, carRect.Y + 10, 34, 18);
    }
}
