using System.Drawing.Drawing2D;

namespace SmartTouristBus.UI;

public static class Theme
{
    public static readonly Color Navy = Color.FromArgb(18, 45, 67);
    public static readonly Color Blue = Color.FromArgb(22, 119, 170);
    public static readonly Color Cyan = Color.FromArgb(52, 183, 199);
    public static readonly Color Sand = Color.FromArgb(244, 236, 216);
    public static readonly Color Coral = Color.FromArgb(230, 103, 76);
    public static readonly Color Green = Color.FromArgb(45, 148, 108);
    public static readonly Color Gold = Color.FromArgb(224, 170, 61);
    public static readonly Color Pale = Color.FromArgb(245, 248, 250);
    public static readonly Color Ink = Color.FromArgb(32, 45, 55);
    public static readonly Color Muted = Color.FromArgb(95, 111, 122);

    public static readonly Font TitleFont = new("Segoe UI Semibold", 22f);
    public static readonly Font HeadingFont = new("Segoe UI Semibold", 14f);
    public static readonly Font BodyFont = new("Segoe UI", 10.5f);
    public static readonly Font SmallFont = new("Segoe UI", 9f);

    public static void ApplyForm(Form form, string title, Size? minimumSize = null)
    {
        form.Text = title;
        form.Font = BodyFont;
        form.BackColor = Pale;
        form.ForeColor = Ink;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimumSize = minimumSize ?? new Size(980, 680);
        form.Size = new Size(1180, 790);
        form.KeyPreview = true;
    }

    public static Label Label(string text, float size = 10.5f, bool bold = false, Color? color = null)
        => new()
        {
            Text = text,
            AutoSize = true,
            Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = color ?? Ink,
            Margin = new Padding(4)
        };

    public static Label SectionTitle(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = HeadingFont,
        ForeColor = Navy,
        Margin = new Padding(4, 6, 4, 8)
    };

    public static Button Button(string text, Color? color = null, int width = 160)
    {
        var button = new Button
        {
            Text = text,
            Width = width,
            Height = 40,
            FlatStyle = FlatStyle.Flat,
            BackColor = color ?? Blue,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 10f),
            Cursor = Cursors.Hand,
            Margin = new Padding(5),
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color ?? Blue, 0.12f);
        return button;
    }

    public static Panel Card(int padding = 18) => new()
    {
        BackColor = Color.White,
        Padding = new Padding(padding),
        Margin = new Padding(8),
        BorderStyle = BorderStyle.FixedSingle
    };

    public static FlowLayoutPanel VerticalFlow() => new()
    {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoScroll = true,
        Padding = new Padding(8)
    };

    public static void StyleTabControl(TabControl tabs)
    {
        tabs.Font = new Font("Segoe UI Semibold", 10.5f);
        tabs.Padding = new Point(18, 7);
    }
}

public sealed class CityBanner : Control
{
    public CityBanner()
    {
        DoubleBuffered = true;
        Height = 170;
        Dock = DockStyle.Top;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var sky = new LinearGradientBrush(ClientRectangle, Color.FromArgb(127, 205, 224), Color.FromArgb(239, 218, 170), 90f);
        g.FillRectangle(sky, ClientRectangle);

        using var skyline = new SolidBrush(Color.FromArgb(80, 65, 89, 103));
        var x = 0;
        var heights = new[] { 55, 88, 62, 105, 74, 96, 58, 82, 110, 70, 93, 63 };
        foreach (var h in heights)
        {
            g.FillRectangle(skyline, x, Height - h - 25, 70, h);
            x += 82;
        }

        using var road = new SolidBrush(Color.FromArgb(71, 77, 82));
        g.FillRectangle(road, 0, Height - 42, Width, 42);
        using var linePen = new Pen(Color.FromArgb(238, 202, 95), 3) { DashStyle = DashStyle.Dash };
        g.DrawLine(linePen, 0, Height - 20, Width, Height - 20);

        var busRect = new Rectangle(Math.Max(40, Width - 330), Height - 104, 245, 72);
        using var busBrush = new SolidBrush(Theme.Coral);
        using var busDark = new SolidBrush(Theme.Navy);
        g.FillRoundedRectangle(busBrush, busRect, 14);
        g.FillRectangle(busDark, busRect.X + 18, busRect.Y + 12, busRect.Width - 52, 25);
        g.FillEllipse(Brushes.Black, busRect.X + 35, busRect.Bottom - 12, 28, 28);
        g.FillEllipse(Brushes.Black, busRect.Right - 70, busRect.Bottom - 12, 28, 28);
        using var busFont = new Font("Segoe UI Semibold", 11);
        g.DrawString("ATHENS SMART TOUR", busFont, Brushes.White, busRect.X + 50, busRect.Y + 45);
    }
}

internal static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle rect, int radius)
    {
        using var path = new GraphicsPath();
        var diameter = radius * 2;
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        graphics.FillPath(brush, path);
    }
}
