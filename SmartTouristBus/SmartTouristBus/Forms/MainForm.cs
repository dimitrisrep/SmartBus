using SmartTouristBus.Models;
using SmartTouristBus.Services;
using SmartTouristBus.UI;

namespace SmartTouristBus.Forms;

public sealed class MainForm : AppFormBase
{
    private readonly SimulationEngine _engine = SimulationEngine.Instance;
    private readonly Label _status = new();
    private readonly Button _runButton;

    protected override string HelpTopic => "Αρχική";

    public MainForm() : base("Το Έξυπνο Τουριστικό Λεωφορείο")
    {
        Size = new Size(1160, 790);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(20),
            BackColor = Theme.Pale
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));

        root.Controls.Add(new CityBanner(), 0, 0);

        var roleGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Padding = new Padding(0, 16, 0, 8) };
        for (var i = 0; i < 4; i++) roleGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        roleGrid.Controls.Add(RoleCard("Επιβάτης", "Θέα, αξιοθέατα, παραγγελίες και τουριστική πλοήγηση.", Theme.Blue, () => new PassengerForm().Show(this)), 0, 0);
        roleGrid.Controls.Add(RoleCard("Οδηγός", "Ταχύτητα, λωρίδα, κόπωση, πόρτες και θερμοκρασία.", Theme.Coral, () => new DriverForm().Show(this)), 1, 0);
        roleGrid.Controls.Add(RoleCard("Υπάλληλος", "Οροφή, φωτοβολταϊκά, ενέργεια και σκούπα-ρομπότ.", Theme.Green, () => new EmployeeForm().Show(this)), 2, 0);
        roleGrid.Controls.Add(RoleCard("Καφετέρια", "Παραλαβή παραγγελιών και ορισμός στάσης παράδοσης.", Theme.Gold, () => new CafeteriaForm().Show(this)), 3, 0);
        root.Controls.Add(roleGrid, 0, 1);

        var footer = Theme.Card(12);
        footer.Dock = DockStyle.Fill;
        var footerLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        _status.AutoSize = true;
        _status.Font = new Font("Segoe UI Semibold", 10.5f);
        _status.ForeColor = Theme.Navy;
        _status.Anchor = AnchorStyles.Left;

        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        _runButton = Theme.Button("Έναρξη διαδρομής", Theme.Green, 175);
        _runButton.Click += (_, _) => _engine.ToggleRunning();
        var reset = Theme.Button("Επαναφορά επίδειξης", Theme.Muted, 180);
        reset.Click += (_, _) => _engine.ResetDemo();
        var help = Theme.Button("Βοήθεια (F1)", Theme.Blue, 135);
        help.Click += (_, _) => HelpForm.ShowTopic("Παρουσίαση", this);
        buttons.Controls.AddRange(new Control[] { _runButton, reset, help });

        footerLayout.Controls.Add(_status, 0, 0);
        footerLayout.Controls.Add(buttons, 1, 0);
        footer.Controls.Add(footerLayout);
        root.Controls.Add(footer, 0, 2);

        Controls.Add(root);

        _engine.StateChanged += EngineOnStateChanged;
        FormClosed += (_, _) => _engine.StateChanged -= EngineOnStateChanged;
        UpdateStatus(_engine.Snapshot());
    }

    private static Panel RoleCard(string title, string description, Color accent, Action open)
    {
        var card = Theme.Card(18);
        card.Dock = DockStyle.Fill;

        var accentBar = new Panel { Dock = DockStyle.Top, Height = 7, BackColor = accent };
        var button = Theme.Button($"Άνοιγμα: {title}", accent, 210);
        button.Dock = DockStyle.Bottom;
        button.Click += (_, _) => open();

        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 50,
            Font = new Font("Segoe UI Semibold", 18f),
            ForeColor = Theme.Navy,
            TextAlign = ContentAlignment.MiddleLeft
        };
        var descriptionLabel = new Label
        {
            Text = description,
            Dock = DockStyle.Fill,
            Font = Theme.BodyFont,
            ForeColor = Theme.Muted,
            Padding = new Padding(0, 10, 0, 10)
        };

        card.Controls.Add(descriptionLabel);
        card.Controls.Add(button);
        card.Controls.Add(titleLabel);
        card.Controls.Add(accentBar);
        return card;
    }

    private void EngineOnStateChanged(object? sender, BusSnapshot snapshot) => UpdateStatus(snapshot);

    private void UpdateStatus(BusSnapshot snapshot)
    {
        _status.Text = snapshot.IsRunning
            ? $"● Σε κίνηση — επόμενη στάση: {_engine.NextStop.Name} — {snapshot.Speed:0} km/h"
            : $"● Στάση: {_engine.CurrentStop.Name} — πόρτες {(snapshot.DoorsOpen ? "ανοιχτές" : "κλειστές")}";
        _status.ForeColor = snapshot.IsRunning ? Theme.Green : Theme.Navy;
        _runButton.Text = snapshot.IsRunning ? "Παύση διαδρομής" : "Έναρξη διαδρομής";
    }
}
