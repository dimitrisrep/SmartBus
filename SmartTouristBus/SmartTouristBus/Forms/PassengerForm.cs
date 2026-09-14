using System.Reflection;
using SmartTouristBus.Models;
using SmartTouristBus.Services;
using SmartTouristBus.UI;

namespace SmartTouristBus.Forms;

public sealed class PassengerForm : AppFormBase
{
    private readonly SimulationEngine _engine = SimulationEngine.Instance;
    private readonly OrderService _orders = OrderService.Instance;
    private readonly RouteViewControl _routeView = new();
    private readonly Label _journeyStatus = new();
    private readonly Label _weatherWarning = new();
    private readonly ListBox _attractionList = new();
    private readonly RichTextBox _attractionDescription = new();
    private readonly ComboBox _product = new();
    private readonly NumericUpDown _quantity = new() { Minimum = 1, Maximum = 10, Value = 1 };
    private readonly ListView _cart = new();
    private readonly Label _cartTotal = new();
    private readonly Label _orderStatus = new();
    private readonly List<OrderLine> _cartLines = new();
    private readonly ComboBox _destination = new();
    private readonly RichTextBox _directions = new();
    private CafeOrder? _activeOrder;
    private int _lastStopIndex = -1;

    protected override string HelpTopic => "Επιβάτης";

    public PassengerForm() : base("Οθόνη Επιβάτη — Smart Tourist Bus")
    {
        var header = BuildHeader();
        var tabs = new TabControl { Dock = DockStyle.Fill };
        Theme.StyleTabControl(tabs);
        tabs.TabPages.Add(BuildViewTab());
        tabs.TabPages.Add(BuildAttractionsTab());
        tabs.TabPages.Add(BuildCafeTab());
        tabs.TabPages.Add(BuildNavigationTab());

        Controls.Add(tabs);
        Controls.Add(header);

        _engine.StateChanged += EngineOnStateChanged;
        _orders.OrderChanged += OrdersOnOrderChanged;
        _orders.OrderPlaced += OrdersOnOrderChanged;
        FormClosed += (_, _) =>
        {
            _engine.StateChanged -= EngineOnStateChanged;
            _orders.OrderChanged -= OrdersOnOrderChanged;
            _orders.OrderPlaced -= OrdersOnOrderChanged;
        };

        UpdateState(_engine.Snapshot());
    }

    private Control BuildHeader()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 88, BackColor = Theme.Navy, Padding = new Padding(24, 12, 20, 10) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var title = new Label { Text = "Η διαδρομή σας στην Αθήνα", Dock = DockStyle.Fill, ForeColor = Color.White, Font = Theme.TitleFont, TextAlign = ContentAlignment.MiddleLeft };
        _journeyStatus.AutoSize = true;
        _journeyStatus.ForeColor = Color.White;
        _journeyStatus.Font = new Font("Segoe UI Semibold", 10.5f);
        _journeyStatus.Anchor = AnchorStyles.Right;
        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(_journeyStatus, 1, 0);
        header.Controls.Add(layout);
        return header;
    }

    private TabPage BuildViewTab()
    {
        var page = new TabPage("Θέα διαδρομής") { BackColor = Theme.Pale, Padding = new Padding(14) };
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        var viewCard = Theme.Card(8);
        viewCard.Dock = DockStyle.Fill;
        viewCard.Controls.Add(_routeView);
        _weatherWarning.Dock = DockStyle.Fill;
        _weatherWarning.TextAlign = ContentAlignment.MiddleLeft;
        _weatherWarning.Padding = new Padding(18, 8, 18, 8);
        _weatherWarning.Font = new Font("Segoe UI Semibold", 10.5f);
        _weatherWarning.BackColor = Color.White;
        root.Controls.Add(viewCard, 0, 0);
        root.Controls.Add(_weatherWarning, 0, 1);
        page.Controls.Add(root);
        return page;
    }

    private TabPage BuildAttractionsTab()
    {
        var page = new TabPage("Αξιοθέατα") { BackColor = Theme.Pale, Padding = new Padding(14) };
        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 330 };
        split.Panel1.Padding = new Padding(0, 0, 8, 0);
        split.Panel2.Padding = new Padding(8, 0, 0, 0);

        var left = Theme.Card();
        left.Dock = DockStyle.Fill;
        var leftLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        leftLayout.Controls.Add(Theme.SectionTitle("Κοντινά αξιοθέατα"), 0, 0);
        _attractionList.Dock = DockStyle.Fill;
        _attractionList.BorderStyle = BorderStyle.None;
        _attractionList.DisplayMember = nameof(Attraction.Name);
        _attractionList.Font = new Font("Segoe UI", 11f);
        _attractionList.SelectedIndexChanged += (_, _) => ShowAttraction();
        leftLayout.Controls.Add(_attractionList, 0, 1);
        left.Controls.Add(leftLayout);

        var right = Theme.Card();
        right.Dock = DockStyle.Fill;
        var rightLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        _attractionDescription.Dock = DockStyle.Fill;
        _attractionDescription.ReadOnly = true;
        _attractionDescription.BorderStyle = BorderStyle.None;
        _attractionDescription.BackColor = Color.White;
        _attractionDescription.Font = new Font("Segoe UI", 11f);
        var listen = Theme.Button("🔊 Ακρόαση περιγραφής", Theme.Blue, 220);
        listen.Click += (_, _) => SpeakAttraction();
        rightLayout.Controls.Add(_attractionDescription, 0, 0);
        rightLayout.Controls.Add(listen, 0, 1);
        right.Controls.Add(rightLayout);

        split.Panel1.Controls.Add(left);
        split.Panel2.Controls.Add(right);
        page.Controls.Add(split);
        return page;
    }

    private TabPage BuildCafeTab()
    {
        var page = new TabPage("Καφετέρια") { BackColor = Theme.Pale, Padding = new Padding(14) };
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));

        var menuCard = Theme.Card();
        menuCard.Dock = DockStyle.Fill;
        var menuFlow = Theme.VerticalFlow();
        menuFlow.AutoScroll = false;
        menuFlow.Controls.Add(Theme.SectionTitle("Κατάλογος συνεργαζόμενης καφετέριας"));
        menuFlow.Controls.Add(Theme.Label("Προϊόν", 9, true));
        _product.Width = 360;
        _product.DropDownStyle = ComboBoxStyle.DropDownList;
        _product.DataSource = _orders.Products.ToList();
        _product.Format += (_, args) =>
        {
            if (args.ListItem is CafeProduct p) args.Value = $"{p.Name} — {p.Price:C2}";
        };
        menuFlow.Controls.Add(_product);
        menuFlow.Controls.Add(Theme.Label("Ποσότητα", 9, true));
        _quantity.Width = 100;
        menuFlow.Controls.Add(_quantity);
        var add = Theme.Button("Προσθήκη στο καλάθι", Theme.Blue, 210);
        add.Click += (_, _) => AddToCart();
        menuFlow.Controls.Add(add);
        menuFlow.Controls.Add(Theme.Label("Η παραγγελία θα σταλεί στην καφετέρια μετά την προσομοιωμένη πληρωμή.", 9, false, Theme.Muted));
        menuCard.Controls.Add(menuFlow);

        var cartCard = Theme.Card();
        cartCard.Dock = DockStyle.Fill;
        var cartLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4 };
        cartLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        cartLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        cartLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        cartLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        cartLayout.Controls.Add(Theme.SectionTitle("Καλάθι & κατάσταση παραγγελίας"), 0, 0);
        _cart.Dock = DockStyle.Fill;
        _cart.View = View.Details;
        _cart.FullRowSelect = true;
        _cart.GridLines = true;
        _cart.Columns.Add("Προϊόν", 250);
        _cart.Columns.Add("Ποσ.", 60);
        _cart.Columns.Add("Σύνολο", 100);
        cartLayout.Controls.Add(_cart, 0, 1);
        _cartTotal.Dock = DockStyle.Fill;
        _cartTotal.Font = new Font("Segoe UI Semibold", 14f);
        _cartTotal.ForeColor = Theme.Green;
        _cartTotal.TextAlign = ContentAlignment.MiddleRight;
        cartLayout.Controls.Add(_cartTotal, 0, 2);
        var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _orderStatus.Dock = DockStyle.Fill;
        _orderStatus.Font = new Font("Segoe UI Semibold", 9.5f);
        _orderStatus.ForeColor = Theme.Navy;
        _orderStatus.TextAlign = ContentAlignment.MiddleLeft;
        var checkout = Theme.Button("Πληρωμή με κάρτα", Theme.Green, 190);
        checkout.Click += (_, _) => Checkout();
        bottom.Controls.Add(_orderStatus, 0, 0);
        bottom.Controls.Add(checkout, 1, 0);
        cartLayout.Controls.Add(bottom, 0, 3);
        cartCard.Controls.Add(cartLayout);

        root.Controls.Add(menuCard, 0, 0);
        root.Controls.Add(cartCard, 1, 0);
        page.Controls.Add(root);
        RefreshCart();
        return page;
    }

    private TabPage BuildNavigationTab()
    {
        var page = new TabPage("Τουριστική πλοήγηση") { BackColor = Theme.Pale, Padding = new Padding(14) };
        var card = Theme.Card(24);
        card.Dock = DockStyle.Fill;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        root.Controls.Add(Theme.SectionTitle("Ξενάγηση μετά την αποβίβαση"), 0, 0);
        var choice = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        choice.Controls.Add(Theme.Label("Προορισμός:", 10, true));
        _destination.Width = 500;
        _destination.DropDownStyle = ComboBoxStyle.DropDownList;
        choice.Controls.Add(_destination);
        var navigate = Theme.Button("Έναρξη καθοδήγησης", Theme.Blue, 200);
        navigate.Click += (_, _) => ShowDirections();
        choice.Controls.Add(navigate);
        root.Controls.Add(choice, 0, 1);
        _directions.Dock = DockStyle.Fill;
        _directions.ReadOnly = true;
        _directions.BorderStyle = BorderStyle.None;
        _directions.BackColor = Color.FromArgb(235, 246, 248);
        _directions.Font = new Font("Segoe UI", 11f);
        root.Controls.Add(_directions, 0, 2);
        root.Controls.Add(Theme.Label("Η πλοήγηση είναι προσομοίωση GPS. Σε βροχή προτείνεται αυτόματα το κοντινότερο ασφαλές καταφύγιο.", 9.5f, false, Theme.Muted), 0, 3);
        card.Controls.Add(root);
        page.Controls.Add(card);
        return page;
    }

    private void EngineOnStateChanged(object? sender, BusSnapshot snapshot) => UpdateState(snapshot);

    private void UpdateState(BusSnapshot snapshot)
    {
        _routeView.Snapshot = snapshot;
        _journeyStatus.Text = snapshot.IsRunning
            ? $"Προς {_engine.NextStop.Name} • {snapshot.Speed:0} km/h"
            : $"Στάση {_engine.CurrentStop.Name}";

        if (snapshot.Weather == WeatherKind.Rainy)
        {
            _weatherWarning.Text = $"🌧 Βροχή — ασφαλές καταφύγιο: {_engine.CurrentStop.SafeShelter}";
            _weatherWarning.ForeColor = Color.FromArgb(130, 65, 20);
            _weatherWarning.BackColor = Color.FromArgb(255, 239, 203);
        }
        else
        {
            _weatherWarning.Text = $"☀ {SimulationEngine.WeatherLabel(snapshot.Weather)} • Εξωτερική θερμοκρασία {snapshot.OutsideTemperature:0}°C • Επόμενη στάση {_engine.NextStop.Name}";
            _weatherWarning.ForeColor = Theme.Navy;
            _weatherWarning.BackColor = Color.White;
        }

        if (_lastStopIndex != snapshot.CurrentStopIndex)
        {
            _lastStopIndex = snapshot.CurrentStopIndex;
            RefreshAttractions();
            RefreshNavigation();
        }
    }

    private void RefreshAttractions()
    {
        _attractionList.DataSource = null;
        _attractionList.DataSource = _engine.CurrentStop.Attractions.ToList();
        _attractionList.DisplayMember = nameof(Attraction.Name);
        if (_attractionList.Items.Count > 0) _attractionList.SelectedIndex = 0;
    }

    private void ShowAttraction()
    {
        if (_attractionList.SelectedItem is not Attraction attraction) return;
        _attractionDescription.Text = $"{attraction.Name}\n{attraction.Category}\n\n{attraction.Description}\n\nΟδηγίες πεζή:\n{attraction.WalkingDirections}";
    }

    private void SpeakAttraction()
    {
        if (_attractionList.SelectedItem is not Attraction attraction) return;
        try
        {
            var voiceType = Type.GetTypeFromProgID("SAPI.SpVoice");
            var voice = voiceType is null ? null : Activator.CreateInstance(voiceType);
            if (voice is null) throw new InvalidOperationException();
            voiceType!.InvokeMember("Speak", BindingFlags.InvokeMethod, null, voice, new object[] { $"{attraction.Name}. {attraction.Description}", 1 });
        }
        catch
        {
            MessageBox.Show("Η φωνητική ανάγνωση απαιτεί ενεργοποιημένη φωνή Windows. Μπορείτε να διαβάσετε την περιγραφή στην οθόνη.", "Ακρόαση", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void AddToCart()
    {
        if (_product.SelectedItem is not CafeProduct product) return;
        var quantity = (int)_quantity.Value;
        var existing = _cartLines.FindIndex(line => line.Product.Id == product.Id);
        if (existing >= 0)
            _cartLines[existing] = new OrderLine(product, _cartLines[existing].Quantity + quantity);
        else
            _cartLines.Add(new OrderLine(product, quantity));
        RefreshCart();
    }

    private void RefreshCart()
    {
        _cart.Items.Clear();
        foreach (var line in _cartLines)
        {
            var item = new ListViewItem(line.Product.Name);
            item.SubItems.Add(line.Quantity.ToString());
            item.SubItems.Add(line.Total.ToString("C2"));
            _cart.Items.Add(item);
        }
        _cartTotal.Text = $"Σύνολο: {_cartLines.Sum(line => line.Total):C2}";
        if (_activeOrder is null)
            _orderStatus.Text = "Δεν υπάρχει ενεργή παραγγελία.";
    }

    private void Checkout()
    {
        if (_cartLines.Count == 0)
        {
            MessageBox.Show("Προσθέστε πρώτα ένα προϊόν στο καλάθι.", "Κενό καλάθι", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var total = _cartLines.Sum(line => line.Total);
        using var payment = new PaymentForm(total);
        if (payment.ShowDialog(this) != DialogResult.OK) return;

        _activeOrder = _orders.PlaceOrder(_cartLines.ToArray(), "12A", payment.MaskedCard);
        _cartLines.Clear();
        RefreshCart();
        UpdateOrderStatus(_activeOrder);
        MessageBox.Show($"Η παραγγελία {_activeOrder.Id} στάλθηκε στην καφετέρια. Θα ενημερωθείτε για τη στάση παράδοσης.", "Επιτυχής παραγγελία", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OrdersOnOrderChanged(object? sender, CafeOrder order)
    {
        if (_activeOrder?.Id == order.Id) UpdateOrderStatus(order);
    }

    private void UpdateOrderStatus(CafeOrder order)
    {
        _orderStatus.Text = $"{order.Id}: {CafeOrder.StatusLabel(order.Status)} • Παράδοση: {order.DeliveryStop}";
        _orderStatus.ForeColor = order.Status == OrderStatus.Cancelled ? Theme.Coral : Theme.Navy;
    }

    private void RefreshNavigation()
    {
        var options = new List<NavigationOption>();
        options.AddRange(_engine.CurrentStop.Attractions.Select(attraction => new NavigationOption($"Αξιοθέατο: {attraction.Name}", attraction.WalkingDirections)));
        options.AddRange(_engine.CurrentStop.Restaurants.Select(restaurant => new NavigationOption($"Εστιατόριο: {restaurant}", $"Ακολουθήστε τη φωτεινή διαδρομή στον χάρτη. Εκτιμώμενος χρόνος πεζή: {5 + restaurant.Length % 8} λεπτά.")));
        options.Add(new NavigationOption($"Ασφαλές καταφύγιο: {_engine.CurrentStop.SafeShelter}", $"Κατευθυνθείτε προς {_engine.CurrentStop.SafeShelter}. Η διαδρομή αποφεύγει ακάλυπτα σημεία και διαρκεί περίπου 4 λεπτά."));
        options.Add(new NavigationOption($"Κοντινότερη στάση: {_engine.NextStop.Name}", $"Η κοντινότερη επόμενη στάση της κυκλικής διαδρομής είναι {_engine.NextStop.Name}. Ακολουθήστε τις μπλε ενδείξεις για περίπου 9 λεπτά."));
        _destination.DataSource = options;
        _destination.DisplayMember = nameof(NavigationOption.Label);
        if (_engine.Weather == WeatherKind.Rainy)
            _destination.SelectedIndex = options.FindIndex(option => option.Label.StartsWith("Ασφαλές"));
        ShowDirections();
    }

    private void ShowDirections()
    {
        if (_destination.SelectedItem is not NavigationOption option) return;
        _directions.Text = $"Αφετηρία: Στάση {_engine.CurrentStop.Name}\n\nΠροορισμός: {option.Label}\n\n{option.Directions}\n\n• Κρατήστε το κινητό σε ασφαλή θέση.\n• Χρησιμοποιείτε τις διαβάσεις πεζών.\n• Πατήστε «Κοντινότερη στάση» για επανεπιβίβαση.";
    }

    private sealed record NavigationOption(string Label, string Directions);
}
