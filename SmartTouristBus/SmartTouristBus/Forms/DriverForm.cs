using System.Media;
using SmartTouristBus.Models;
using SmartTouristBus.Services;
using SmartTouristBus.UI;

namespace SmartTouristBus.Forms;

public sealed class DriverForm : AppFormBase
{
    private readonly SimulationEngine _engine = SimulationEngine.Instance;
    private readonly Label _speed = new();
    private readonly Label _limit = new();
    private readonly Label _doors = new();
    private readonly Label _climate = new();
    private readonly ProgressBar _fatigue = new() { Minimum = 0, Maximum = 100, Style = ProgressBarStyle.Continuous };
    private readonly LaneIndicator _lane = new();
    private readonly ListBox _alerts = new();
    private readonly Button _run;
    private readonly ComboBox _climateMode = new();
    private readonly TrackBar _targetTemperature = new() { Minimum = 17, Maximum = 28, TickFrequency = 1, Value = 23 };
    private bool _updating;

    protected override string HelpTopic => "Οδηγός";

    public DriverForm() : base("Ταμπλό Οδηγού — Smart Tourist Bus")
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 82, BackColor = Theme.Navy, Padding = new Padding(24, 12, 18, 10) };
        header.Controls.Add(new Label { Text = "Κέντρο υποβοήθησης οδηγού", Dock = DockStyle.Fill, ForeColor = Color.White, Font = Theme.TitleFont, TextAlign = ContentAlignment.MiddleLeft });

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(14) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 64));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 36));

        root.Controls.Add(BuildDrivingCard(), 0, 0);
        root.Controls.Add(BuildSafetyCard(), 1, 0);
        var alertCard = BuildAlertCard();
        root.SetColumnSpan(alertCard, 2);
        root.Controls.Add(alertCard, 0, 1);

        Controls.Add(root);
        Controls.Add(header);

        _run = _runButtonReference!;

        _engine.StateChanged += EngineOnStateChanged;
        _engine.AlertRaised += EngineOnAlertRaised;
        FormClosed += (_, _) =>
        {
            _engine.StateChanged -= EngineOnStateChanged;
            _engine.AlertRaised -= EngineOnAlertRaised;
        };
        UpdateState(_engine.Snapshot());
    }

    private Button? _runButtonReference;

    private Control BuildDrivingCard()
    {
        var card = Theme.Card();
        card.Dock = DockStyle.Fill;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        root.Controls.Add(Theme.SectionTitle("Οδήγηση & λωρίδα"), 0, 0);

        var speedPanel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Navy };
        _speed.Dock = DockStyle.Fill;
        _speed.TextAlign = ContentAlignment.MiddleCenter;
        _speed.Font = new Font("Segoe UI Semibold", 36f);
        _speed.ForeColor = Color.White;
        _limit.AutoSize = true;
        _limit.Font = new Font("Segoe UI Semibold", 10f);
        _limit.ForeColor = Color.White;
        _limit.BackColor = Theme.Coral;
        _limit.Padding = new Padding(10, 5, 10, 5);
        _limit.Location = new Point(14, 14);
        speedPanel.Controls.Add(_speed);
        speedPanel.Controls.Add(_limit);
        root.Controls.Add(speedPanel, 0, 1);
        root.Controls.Add(_lane, 0, 2);

        var controls = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        _runButtonReference = Theme.Button("Έναρξη διαδρομής", Theme.Green, 170);
        _runButtonReference.Click += (_, _) => _engine.ToggleRunning();
        var slower = Theme.Button("− Ταχύτητα", Theme.Muted, 125);
        slower.Click += (_, _) => _engine.AdjustSpeed(-8);
        var faster = Theme.Button("+ Ταχύτητα", Theme.Coral, 125);
        faster.Click += (_, _) => _engine.AdjustSpeed(8);
        controls.Controls.AddRange(new Control[] { _runButtonReference, slower, faster });
        root.Controls.Add(controls, 0, 3);
        card.Controls.Add(root);
        return card;
    }

    private Control BuildSafetyCard()
    {
        var card = Theme.Card();
        card.Dock = DockStyle.Fill;
        var flow = Theme.VerticalFlow();
        flow.Padding = new Padding(6);
        flow.Controls.Add(Theme.SectionTitle("Ασφάλεια, κόπωση & άνεση"));
        flow.Controls.Add(Theme.Label("Επίπεδο κόπωσης", 9, true));
        _fatigue.Width = 430;
        _fatigue.Height = 24;
        flow.Controls.Add(_fatigue);
        var fatigueDemo = Theme.Button("Σενάριο: κόπωση οδηγού", Theme.Gold, 225);
        fatigueDemo.Click += (_, _) => _engine.TriggerFatigue();
        flow.Controls.Add(fatigueDemo);
        flow.Controls.Add(Theme.Label("Κατάσταση θυρών", 9, true));
        _doors.AutoSize = false;
        _doors.Width = 430;
        _doors.Height = 38;
        _doors.TextAlign = ContentAlignment.MiddleLeft;
        _doors.Font = new Font("Segoe UI Semibold", 11f);
        flow.Controls.Add(_doors);
        var doorButtons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
        var open = Theme.Button("Άνοιγμα θυρών", Theme.Blue, 150);
        open.Click += (_, _) => _engine.OpenDoors();
        var close = Theme.Button("Κλείσιμο θυρών", Theme.Coral, 150);
        close.Click += (_, _) =>
        {
            if (!_engine.TryCloseDoors(out var message))
                MessageBox.Show(message, "Ασφάλεια θυρών", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        };
        var alighting = Theme.Button("Επιβάτες κατεβαίνουν", Theme.Gold, 195);
        alighting.Click += (_, _) => _engine.TriggerAlighting();
        doorButtons.Controls.AddRange(new Control[] { open, close, alighting });
        flow.Controls.Add(doorButtons);

        flow.Controls.Add(Theme.Label("Κλιματισμός", 9, true));
        _climateMode.Width = 170;
        _climateMode.DropDownStyle = ComboBoxStyle.DropDownList;
        _climateMode.Items.AddRange(new object[] { "Απενεργοποιημένο", "Ψύξη", "Θέρμανση" });
        _targetTemperature.Width = 240;
        var climateRow = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        climateRow.Controls.Add(_climateMode);
        climateRow.Controls.Add(_targetTemperature);
        var apply = Theme.Button("Εφαρμογή", Theme.Green, 110);
        apply.Click += (_, _) => ApplyClimate();
        climateRow.Controls.Add(apply);
        flow.Controls.Add(climateRow);
        _climate.AutoSize = true;
        _climate.ForeColor = Theme.Muted;
        flow.Controls.Add(_climate);

        var laneDemo = Theme.Button("Σενάριο: έξοδος από λωρίδα", Theme.Coral, 245);
        laneDemo.Click += (_, _) => _engine.TriggerLaneDeparture();
        flow.Controls.Add(laneDemo);
        card.Controls.Add(flow);
        return card;
    }

    private Control BuildAlertCard()
    {
        var card = Theme.Card();
        card.Dock = DockStyle.Fill;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(Theme.SectionTitle("Ειδοποιήσεις υποβοήθησης"), 0, 0);
        _alerts.Dock = DockStyle.Fill;
        _alerts.BorderStyle = BorderStyle.None;
        _alerts.Font = new Font("Consolas", 10f);
        root.Controls.Add(_alerts, 0, 1);
        card.Controls.Add(root);
        return card;
    }

    private void EngineOnStateChanged(object? sender, BusSnapshot snapshot) => UpdateState(snapshot);

    private void UpdateState(BusSnapshot snapshot)
    {
        _updating = true;
        _speed.Text = $"{snapshot.Speed:0}\nkm/h";
        _speed.ForeColor = snapshot.Speed > snapshot.SpeedLimit ? Color.FromArgb(255, 214, 205) : Color.White;
        _limit.Text = $"ΟΡΙΟ {snapshot.SpeedLimit}";
        _lane.Offset = snapshot.LaneOffset;
        _fatigue.Value = Math.Clamp((int)(snapshot.Fatigue * 100), 0, 100);
        _doors.Text = snapshot.PassengersAlighting
            ? "⚠ Επιβάτες βρίσκονται στην έξοδο"
            : snapshot.DoorsOpen ? "● Πόρτες ανοιχτές" : "● Πόρτες κλειστές";
        _doors.ForeColor = snapshot.PassengersAlighting ? Theme.Coral : snapshot.DoorsOpen ? Theme.Gold : Theme.Green;
        _run.Text = snapshot.IsRunning ? "Παύση διαδρομής" : "Έναρξη διαδρομής";
        _climateMode.SelectedIndex = snapshot.ClimateMode switch { ClimateMode.Off => 0, ClimateMode.Cooling => 1, ClimateMode.Heating => 2, _ => 0 };
        _targetTemperature.Value = Math.Clamp((int)Math.Round(snapshot.TargetTemperature), 17, 28);
        _climate.Text = $"Καμπίνα {snapshot.CabinTemperature:0.0}°C • στόχος {snapshot.TargetTemperature:0}°C • {SimulationEngine.ClimateLabel(snapshot.ClimateMode)}";
        _updating = false;
    }

    private void ApplyClimate()
    {
        if (_updating) return;
        var mode = _climateMode.SelectedIndex switch { 1 => ClimateMode.Cooling, 2 => ClimateMode.Heating, _ => ClimateMode.Off };
        _engine.SetClimate(mode, _targetTemperature.Value);
    }

    private void EngineOnAlertRaised(object? sender, SystemAlert alert)
    {
        _alerts.Items.Insert(0, alert.ToString());
        while (_alerts.Items.Count > 30) _alerts.Items.RemoveAt(_alerts.Items.Count - 1);
        if (alert.Critical) SystemSounds.Exclamation.Play();
    }
}
