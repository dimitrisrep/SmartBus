using SmartTouristBus.Models;
using SmartTouristBus.Services;
using SmartTouristBus.UI;

namespace SmartTouristBus.Forms;

public sealed class CafeteriaForm : AppFormBase
{//ftiakse ena pedio _orders typou orderservice kai kane to na deixnei sto instance.

    private readonly OrderService _orders = OrderService.Instance;
    private readonly SimulationEngine _engine = SimulationEngine.Instance;
    private readonly ListBox _orderList = new();
    private readonly RichTextBox _details = new();
    private readonly ComboBox _deliveryStop = new();
    private readonly Label _summary = new();

    protected override string HelpTopic => "Καφετέρια";

    public CafeteriaForm() : base("Συνεργαζόμενη Καφετέρια — Smart Tourist Bus")
    {
        var header = new Panel { 
            Dock = DockStyle.Top, 
            Height = 82, 
            BackColor = Theme.Gold, 
            Padding = new Padding(24, 12, 18, 10) 
        };
        header.Controls.Add(
            new Label { 
                Text = "Κονσόλα παραγγελιών καφετέριας", 
                Dock = DockStyle.Fill, 
                ForeColor = Theme.Navy, 
                Font = Theme.TitleFont, 
                TextAlign = ContentAlignment.MiddleLeft 
            }
         );

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(14) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        root.Controls.Add(BuildOrderList(), 0, 0);
        root.Controls.Add(BuildOrderDetails(), 1, 0);

        Controls.Add(root);
        Controls.Add(header);

        _orders.OrderPlaced += OrdersOnChanged;
        _orders.OrderChanged += OrdersOnChanged;
        FormClosed += (_, _) =>
        {
            _orders.OrderPlaced -= OrdersOnChanged;
            _orders.OrderChanged -= OrdersOnChanged;
        }; //ta kanw unsubscribe otan kleisei, gia na min sinexisei na akouei ena kleisto parathrio
        
        RefreshOrders();
    }

    private Control BuildOrderList()
    {
        var card = Theme.Card();
        card.Dock = DockStyle.Fill;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.Controls.Add(Theme.SectionTitle("Εισερχόμενες παραγγελίες"), 0, 0);
        _orderList.Dock = DockStyle.Fill;
        _orderList.BorderStyle = BorderStyle.None;
        _orderList.Font = new Font("Segoe UI", 10.5f);
        _orderList.SelectedIndexChanged += (_, _) => ShowSelectedOrder();
        root.Controls.Add(_orderList, 0, 1);
        _summary.Dock = DockStyle.Fill;
        _summary.TextAlign = ContentAlignment.MiddleLeft;
        _summary.Font = new Font("Segoe UI Semibold", 10f);
        _summary.ForeColor = Theme.Muted;
        root.Controls.Add(_summary, 0, 2);
        card.Controls.Add(root);
        return card;
    }

    private Control BuildOrderDetails()
    {
        var card = Theme.Card();
        card.Dock = DockStyle.Fill;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
        root.Controls.Add(Theme.SectionTitle("Λεπτομέρειες & ροή παράδοσης"), 0, 0);
        _details.Dock = DockStyle.Fill;
        _details.ReadOnly = true;
        _details.BorderStyle = BorderStyle.None;
        _details.BackColor = Color.FromArgb(252, 250, 244);
        _details.Font = new Font("Segoe UI", 11f);
        root.Controls.Add(_details, 0, 1);

        var delivery = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        delivery.Controls.Add(Theme.Label("Στάση παράδοσης:", 10, true));
        _deliveryStop.Width = 310;
        _deliveryStop.DropDownStyle = ComboBoxStyle.DropDownList;
        _deliveryStop.DataSource = _engine.Stops.Select(stop => stop.Name).ToList();
        delivery.Controls.Add(_deliveryStop);
        root.Controls.Add(delivery, 0, 2);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        actions.Controls.Add(StatusButton("Αποδοχή", OrderStatus.Accepted, Theme.Blue));
        actions.Controls.Add(StatusButton("Προετοιμασία", OrderStatus.Preparing, Theme.Gold));
        actions.Controls.Add(StatusButton("Έτοιμη", OrderStatus.ReadyForDelivery, Theme.Green));
        actions.Controls.Add(StatusButton("Παραδόθηκε", OrderStatus.Delivered, Theme.Navy));
        actions.Controls.Add(StatusButton("Ακύρωση", OrderStatus.Cancelled, Theme.Coral));
        root.Controls.Add(actions, 0, 3);
        card.Controls.Add(root);
        return card;
    }

    private Button StatusButton(string text, OrderStatus status, Color color)
    {
        var button = Theme.Button(text, color, 130);
        button.Click += (_, _) => UpdateSelected(status);
        return button;
    }

    private void OrdersOnChanged(object? sender, CafeOrder order) => RefreshOrders(order.Id);

    private void RefreshOrders(string? selectId = null)
    {
        var selected = selectId ?? (_orderList.SelectedItem as CafeOrder)?.Id;
        _orderList.DataSource = null;
        _orderList.DataSource = _orders.Orders.ToList();
        _summary.Text = $"Σύνολο: {_orders.Orders.Count} • Σε εξέλιξη: {_orders.Orders.Count(order => order.Status != OrderStatus.Delivered && order.Status != OrderStatus.Cancelled)}";
        if (selected is not null)
        {
            var index = _orders.Orders.ToList().FindIndex(order => order.Id == selected);
            if (index >= 0) _orderList.SelectedIndex = index;
        }
        if (_orderList.SelectedIndex < 0 && _orderList.Items.Count > 0) _orderList.SelectedIndex = 0;
        ShowSelectedOrder();
    }

    private void ShowSelectedOrder()
    {
        if (_orderList.SelectedItem is not CafeOrder order)
        {
            _details.Text = "Δεν υπάρχουν ακόμη παραγγελίες. Ανοίξτε την οθόνη Επιβάτη και πραγματοποιήστε μια δοκιμαστική αγορά.";
            return;
        }

        var lines = string.Join(Environment.NewLine, order.Lines.Select(line => $"• {line.Quantity} × {line.Product.Name} — {line.Total:C2}"));
        _details.Text = $"Παραγγελία: {order.Id}\nΏρα: {order.CreatedAt:HH:mm}\nΘέση επιβάτη: {order.PassengerSeat}\nΚάρτα: {order.MaskedCard}\n\n{lines}\n\nΣύνολο: {order.Total:C2}\nΚατάσταση: {CafeOrder.StatusLabel(order.Status)}\nΣτάση παράδοσης: {order.DeliveryStop}";
        var stopIndex = _engine.Stops.ToList().FindIndex(stop => stop.Name == order.DeliveryStop);
        if (stopIndex >= 0) _deliveryStop.SelectedIndex = stopIndex;
    }

    private void UpdateSelected(OrderStatus status)
    {
        if (_orderList.SelectedItem is not CafeOrder order)
        {
            MessageBox.Show("Επιλέξτε πρώτα μια παραγγελία.", "Καφετέρια", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var delivery = _deliveryStop.SelectedItem?.ToString();
        if (status is not OrderStatus.Cancelled && string.IsNullOrWhiteSpace(delivery))
        {
            MessageBox.Show("Ορίστε τη στάση παράδοσης.", "Στάση παράδοσης", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _orders.Update(order, status, status == OrderStatus.Cancelled ? null : delivery);
    }
}
