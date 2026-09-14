using SmartTouristBus.Models;
using SmartTouristBus.Services;
using SmartTouristBus.UI;

namespace SmartTouristBus.Forms;

public sealed class EmployeeForm : AppFormBase
{
    private readonly SimulationEngine _engine = SimulationEngine.Instance;
    private readonly ComboBox _weather = new();
    private readonly Label _roof = new();
    private readonly Label _production = new();
    private readonly Label _consumption = new();
    private readonly Label _battery = new();
    private readonly ProgressBar _productionBar = new() { Minimum = 0, Maximum = 200 };
    private readonly ProgressBar _consumptionBar = new() { Minimum = 0, Maximum = 200 };
    private readonly ProgressBar _batteryBar = new() { Minimum = 0, Maximum = 100 };
    private readonly Label _climateFeedback = new();
    private readonly ComboBox _climateMode = new();
    private readonly NumericUpDown _target = new() { Minimum = 17, Maximum = 28, Value = 23 };
    private readonly ProgressBar _robotProgress = new() { Minimum = 0, Maximum = 100 };
    private readonly Label _robotState = new();
    private readonly ListBox _lostItems = new();
    private readonly ListBox _notifications = new();
    private readonly Button _robotStart;
    private bool _updating;

    protected override string HelpTopic => "Υπάλληλος";

    public EmployeeForm() : base("Κονσόλα Υπαλλήλου — Smart Tourist Bus")
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 82, BackColor = Theme.Green, Padding = new Padding(24, 12, 18, 10) };
        header.Controls.Add(new Label { Text = "Διαχείριση λεωφορείου & ενέργειας", Dock = DockStyle.Fill, ForeColor = Color.White, Font = Theme.TitleFont, TextAlign = ContentAlignment.MiddleLeft });

        var tabs = new TabControl { Dock = DockStyle.Fill };
        Theme.StyleTabControl(tabs);
        tabs.TabPages.Add(BuildEnergyTab());
        tabs.TabPages.Add(BuildRobotTab());
        tabs.TabPages.Add(BuildNotificationsTab());

        Controls.Add(tabs);
        Controls.Add(header);

        _robotStart = _robotStartReference!;
        _engine.StateChanged += EngineOnStateChanged;
        _engine.AlertRaised += EngineOnAlertRaised;
        FormClosed += (_, _) =>
        {
            _engine.StateChanged -= EngineOnStateChanged;
            _engine.AlertRaised -= EngineOnAlertRaised;
        };
        UpdateState(_engine.Snapshot());
    }

    private Button? _robotStartReference;

    private TabPage BuildEnergyTab()
    {
        var page = new TabPage("Οροφή & ενέργεια") { BackColor = Theme.Pale, Padding = new Padding(14) };
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));

        var controlsCard = Theme.Card();
        controlsCard.Dock = DockStyle.Fill;
        var controls = Theme.VerticalFlow();
        controls.Padding = new Padding(6);
        controls.Controls.Add(Theme.SectionTitle("Αποκλειστικός έλεγχος υπαλλήλου"));
        controls.Controls.Add(Theme.Label("Καιρικές συνθήκες προσομοίωσης", 9, true));
        _weather.Width = 320;
        _weather.DropDownStyle = ComboBoxStyle.DropDownList;
        _weather.Items.AddRange(new object[] { "Ηλιοφάνεια", "Συννεφιά", "Βροχή" });
        _weather.SelectedIndexChanged += (_, _) =>
        {
            if (_updating) return;
            _engine.SetWeather(_weather.SelectedIndex switch { 1 => WeatherKind.Cloudy, 2 => WeatherKind.Rainy, _ => WeatherKind.Sunny });
        };
        controls.Controls.Add(_weather);
        controls.Controls.Add(Theme.Label("Κατάσταση οροφής", 9, true));
        _roof.AutoSize = false;
        _roof.Width = 360;
        _roof.Height = 42;
        _roof.Font = new Font("Segoe UI Semibold", 12f);
        controls.Controls.Add(_roof);
        var roofButton = Theme.Button("Άνοιγμα / κλείσιμο οροφής", Theme.Blue, 235);
        roofButton.Click += (_, _) =>
        {
            if (!_engine.ToggleRoof(out var message))
                MessageBox.Show(message, "Έλεγχος οροφής", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        };
        controls.Controls.Add(roofButton);

        controls.Controls.Add(Theme.Label("Κλιματισμός και στόχος θερμοκρασίας", 9, true));
        _climateMode.Width = 170;
        _climateMode.DropDownStyle = ComboBoxStyle.DropDownList;
        _climateMode.Items.AddRange(new object[] { "Απενεργοποιημένο", "Ψύξη", "Θέρμανση" });
        _target.Width = 90;
        var climateRow = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
        climateRow.Controls.Add(_climateMode);
        climateRow.Controls.Add(_target);
        var climateApply = Theme.Button("Εφαρμογή", Theme.Green, 110);
        climateApply.Click += (_, _) =>
        {
            var mode = _climateMode.SelectedIndex switch { 1 => ClimateMode.Cooling, 2 => ClimateMode.Heating, _ => ClimateMode.Off };
            _engine.SetClimate(mode, (double)_target.Value);
        };
        climateRow.Controls.Add(climateApply);
        controls.Controls.Add(climateRow);
        _climateFeedback.AutoSize = true;
        _climateFeedback.MaximumSize = new Size(360, 0);
        _climateFeedback.ForeColor = Theme.Muted;
        controls.Controls.Add(_climateFeedback);
        controlsCard.Controls.Add(controls);

        var energyCard = Theme.Card(24);
        energyCard.Dock = DockStyle.Fill;
        var energy = Theme.VerticalFlow();
        energy.Padding = new Padding(8);
        energy.Controls.Add(Theme.SectionTitle("Ενέργεια σε πραγματικό χρόνο"));
        AddMeter(energy, "Παραγωγή φωτοβολταϊκών", _production, _productionBar, Theme.Gold);
        AddMeter(energy, "Συνολική κατανάλωση", _consumption, _consumptionBar, Theme.Coral);
        AddMeter(energy, "Αποθήκευση μπαταρίας", _battery, _batteryBar, Theme.Green);
        energy.Controls.Add(Theme.Label("Οι τιμές μεταβάλλονται ανάλογα με τον καιρό, την κίνηση, τον κλιματισμό και τη σκούπα-ρομπότ.", 9.5f, false, Theme.Muted));
        energyCard.Controls.Add(energy);

        root.Controls.Add(controlsCard, 0, 0);
        root.Controls.Add(energyCard, 1, 0);
        page.Controls.Add(root);
        return page;
    }

    private TabPage BuildRobotTab()
    {
        var page = new TabPage("Σκούπα-ρομπότ") { BackColor = Theme.Pale, Padding = new Padding(14) };
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));

        var controlCard = Theme.Card();
        controlCard.Dock = DockStyle.Fill;
        var flow = Theme.VerticalFlow();
        flow.Padding = new Padding(8);
        flow.Controls.Add(Theme.SectionTitle("Αυτόνομος καθαρισμός"));
        _robotState.AutoSize = true;
        _robotState.Font = new Font("Segoe UI Semibold", 14f);
        flow.Controls.Add(_robotState);
        _robotProgress.Width = 400;
        _robotProgress.Height = 30;
        flow.Controls.Add(_robotProgress);
        _robotStartReference = Theme.Button("Έναρξη καθαρισμού", Theme.Green, 200);
        _robotStartReference.Click += (_, _) => _engine.StartRobot();
        var pause = Theme.Button("Παύση / συνέχιση", Theme.Blue, 180);
        pause.Click += (_, _) => _engine.PauseRobot();
        var demo = Theme.Button("Σενάριο: εύρεση διαβατηρίου", Theme.Gold, 255);
        demo.Click += (_, _) => _engine.TriggerLostItem();
        flow.Controls.Add(_robotStartReference);
        flow.Controls.Add(pause);
        flow.Controls.Add(demo);
        flow.Controls.Add(Theme.Label("Κατά την έναρξη τα πόδια της σκούπας εκτείνονται. Μετά την ολοκλήρωση μαζεύονται αυτόματα.", 9.5f, false, Theme.Muted));
        controlCard.Controls.Add(flow);

        var itemsCard = Theme.Card();
        itemsCard.Dock = DockStyle.Fill;
        var itemLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        itemLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        itemLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        itemLayout.Controls.Add(Theme.SectionTitle("Αναγνωρισμένα πολύτιμα αντικείμενα"), 0, 0);
        _lostItems.Dock = DockStyle.Fill;
        _lostItems.BorderStyle = BorderStyle.None;
        _lostItems.Font = new Font("Segoe UI", 11f);
        itemLayout.Controls.Add(_lostItems, 0, 1);
        itemsCard.Controls.Add(itemLayout);

        root.Controls.Add(controlCard, 0, 0);
        root.Controls.Add(itemsCard, 1, 0);
        page.Controls.Add(root);
        return page;
    }

    private TabPage BuildNotificationsTab()
    {
        var page = new TabPage("Ειδοποιήσεις") { BackColor = Theme.Pale, Padding = new Padding(14) };
        var card = Theme.Card();
        card.Dock = DockStyle.Fill;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(Theme.SectionTitle("Κέντρο ειδοποιήσεων εταιρείας"), 0, 0);
        _notifications.Dock = DockStyle.Fill;
        _notifications.BorderStyle = BorderStyle.None;
        _notifications.Font = new Font("Consolas", 10f);
        root.Controls.Add(_notifications, 0, 1);
        card.Controls.Add(root);
        page.Controls.Add(card);
        return page;
    }

    private static void AddMeter(FlowLayoutPanel parent, string title, Label value, ProgressBar bar, Color color)
    {
        parent.Controls.Add(Theme.Label(title, 10, true, color));
        value.AutoSize = true;
        value.Font = new Font("Segoe UI Semibold", 15f);
        value.ForeColor = Theme.Navy;
        parent.Controls.Add(value);
        bar.Width = 520;
        bar.Height = 28;
        parent.Controls.Add(bar);
    }

    private void EngineOnStateChanged(object? sender, BusSnapshot snapshot) => UpdateState(snapshot);

    private void UpdateState(BusSnapshot snapshot)
    {
        _updating = true;
        _weather.SelectedIndex = snapshot.Weather switch { WeatherKind.Cloudy => 1, WeatherKind.Rainy => 2, _ => 0 };
        _roof.Text = snapshot.RoofOpen ? "☀ Οροφή ανοιχτή — πάνελ εκτεταμένα" : "● Οροφή κλειστή";
        _roof.ForeColor = snapshot.RoofOpen ? Theme.Green : snapshot.Weather == WeatherKind.Rainy ? Theme.Coral : Theme.Navy;
        _production.Text = $"{snapshot.SolarProduction:0.0} kW";
        _consumption.Text = $"{snapshot.EnergyConsumption:0.0} kW";
        _battery.Text = $"{snapshot.BatteryPercent:0}%";
        _productionBar.Value = Math.Clamp((int)(snapshot.SolarProduction * 10), 0, 200);
        _consumptionBar.Value = Math.Clamp((int)(snapshot.EnergyConsumption * 10), 0, 200);
        _batteryBar.Value = Math.Clamp((int)snapshot.BatteryPercent, 0, 100);
        _climateMode.SelectedIndex = snapshot.ClimateMode switch { ClimateMode.Cooling => 1, ClimateMode.Heating => 2, _ => 0 };
        _target.Value = Math.Clamp((decimal)snapshot.TargetTemperature, 17, 28);
        var balance = snapshot.SolarProduction - snapshot.EnergyConsumption;
        _climateFeedback.Text = $"Καμπίνα {snapshot.CabinTemperature:0.0}°C. Ενεργειακό ισοζύγιο: {balance:+0.0;-0.0;0.0} kW.";
        _robotState.Text = snapshot.RobotState switch
        {
            RobotState.Cleaning => "Καθαρισμός σε εξέλιξη — πόδια εκτεταμένα",
            RobotState.Paused => "Παύση καθαρισμού",
            RobotState.Completed => "Ολοκληρώθηκε — πόδια μαζεμένα",
            _ => "Σε αναμονή"
        };
        _robotProgress.Value = Math.Clamp((int)(snapshot.RobotProgress * 100), 0, 100);
        _robotStart.Text = snapshot.RobotState == RobotState.Completed ? "Νέος καθαρισμός" : "Έναρξη καθαρισμού";
        _lostItems.DataSource = null;
        _lostItems.DataSource = snapshot.LostItems.ToList();
        _updating = false;
    }

    private void EngineOnAlertRaised(object? sender, SystemAlert alert)
    {
        _notifications.Items.Insert(0, $"[{alert.Source}] {alert}");
        while (_notifications.Items.Count > 50) _notifications.Items.RemoveAt(_notifications.Items.Count - 1);
    }
}
