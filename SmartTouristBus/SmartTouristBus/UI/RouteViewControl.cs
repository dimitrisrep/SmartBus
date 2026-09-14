using System.Drawing.Drawing2D;
using SmartTouristBus.Models;
using SmartTouristBus.Services;

namespace SmartTouristBus.UI;

public sealed class RouteViewControl : Control
{
    private BusSnapshot _snapshot = SimulationEngine.Instance.Snapshot();

    public BusSnapshot Snapshot
    {
        get => _snapshot;
        set
        {
            _snapshot = value;
            Invalidate();
        }
    }

    public RouteViewControl()
    {
        DoubleBuffered = true;
        Dock = DockStyle.Fill;
        MinimumSize = new Size(480, 300);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var horizon = Height * 0.42f;

        var skyTop = _snapshot.Weather == WeatherKind.Rainy ? Color.FromArgb(105, 125, 140) : Color.FromArgb(96, 190, 223);
        var skyBottom = _snapshot.Weather == WeatherKind.Rainy ? Color.FromArgb(180, 190, 195) : Color.FromArgb(229, 224, 178);
        using var sky = new LinearGradientBrush(new Rectangle(0, 0, Width, (int)horizon + 20), skyTop, skyBottom, 90f);
        g.FillRectangle(sky, 0, 0, Width, horizon + 20);

        if (_snapshot.Weather == WeatherKind.Sunny)
        {
            using var sunBrush = new SolidBrush(Color.FromArgb(245, 210, 75));
            g.FillEllipse(sunBrush, Width - 110, 35, 55, 55);
        }
        else if (_snapshot.Weather == WeatherKind.Rainy)
        {
            using var rainPen = new Pen(Color.FromArgb(155, 220, 240), 2);
            for (var x = 20; x < Width; x += 38)
                g.DrawLine(rainPen, x, 35 + x % 55, x - 9, 65 + x % 55);
        }

        DrawBuildings(g, horizon);

        var road = new[]
        {
            new PointF(Width * 0.40f, horizon),
            new PointF(Width * 0.60f, horizon),
            new PointF(Width * 0.94f, Height),
            new PointF(Width * 0.06f, Height)
        };
        using var roadBrush = new SolidBrush(Color.FromArgb(66, 72, 77));
        g.FillPolygon(roadBrush, road);

        using var lanePen = new Pen(Color.White, 5) { DashStyle = DashStyle.Dash };
        var offset = (float)(_snapshot.LaneOffset * Width * 0.025);
        g.DrawLine(lanePen, Width / 2f + offset, horizon + 25, Width / 2f + offset, Height);

        using var curbPen = new Pen(Color.FromArgb(230, 218, 185), 9);
        g.DrawLine(curbPen, road[0], road[3]);
        g.DrawLine(curbPen, road[1], road[2]);

        var overlay = new Rectangle(18, 18, 250, 92);
        using var overlayBrush = new SolidBrush(Color.FromArgb(210, 18, 45, 67));
        g.FillRoundedRectangle(overlayBrush, overlay, 12);
        using var speedFont = new Font("Segoe UI Semibold", 24);
        using var stopFont = new Font("Segoe UI", 10);
        g.DrawString($"{_snapshot.Speed:0} km/h", speedFont, Brushes.White, 34, 27);
        g.DrawString($"Επόμενη στάση: {SimulationEngine.Instance.NextStop.Name}", stopFont, Brushes.White, 35, 73);

        var progressWidth = Math.Max(80, Width - 50);
        using var progressBackground = new SolidBrush(Color.FromArgb(130, Color.White));
        using var progressForeground = new SolidBrush(Theme.Cyan);
        g.FillRectangle(progressBackground, 25, Height - 24, progressWidth, 7);
        g.FillRectangle(progressForeground, 25, Height - 24, (int)(progressWidth * _snapshot.RouteProgress), 7);
    }

    private void DrawBuildings(Graphics g, float horizon)
    {
        var colors = new[]
        {
            Color.FromArgb(202, 182, 145), Color.FromArgb(224, 210, 181),
            Color.FromArgb(183, 163, 137), Color.FromArgb(210, 191, 158)
        };
        for (var i = 0; i < 9; i++)
        {
            var width = Width / 10 + 14;
            var x = i * (Width / 8) - 35;
            var height = 75 + (i * 31 % 105);
            var rect = new Rectangle(x, (int)horizon - height, width, height);
            using var buildingBrush = new SolidBrush(colors[i % colors.Length]);
            g.FillRectangle(buildingBrush, rect);
            using var windowBrush = new SolidBrush(Color.FromArgb(70, 91, 105));
            for (var wx = rect.X + 10; wx < rect.Right - 8; wx += 24)
                for (var wy = rect.Y + 14; wy < rect.Bottom - 12; wy += 24)
                    g.FillRectangle(windowBrush, wx, wy, 10, 13);
        }
    }
}
