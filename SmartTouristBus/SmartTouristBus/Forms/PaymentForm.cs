using SmartTouristBus.UI;

namespace SmartTouristBus.Forms;

public sealed class PaymentForm : Form
{
    private readonly TextBox _card = new() { MaxLength = 19, PlaceholderText = "4242 4242 4242 4242" };
    private readonly TextBox _name = new() { PlaceholderText = "ΟΝΟΜΑ ΚΑΤΟΧΟΥ" };
    private readonly TextBox _expiry = new() { MaxLength = 5, PlaceholderText = "MM/YY" };
    private readonly TextBox _cvv = new() { MaxLength = 3, UseSystemPasswordChar = true, PlaceholderText = "CVV" };

    public string MaskedCard { get; private set; } = string.Empty;

    public PaymentForm(decimal total)
    {
        Theme.ApplyForm(this, "Ασφαλής πληρωμή", new Size(510, 500));
        Size = new Size(530, 540);
        MaximizeBox = false;
        MinimizeBox = false;

        var root = Theme.VerticalFlow();
        root.Padding = new Padding(28);
        root.Controls.Add(Theme.SectionTitle("Πληρωμή με κάρτα"));
        root.Controls.Add(Theme.Label($"Συνολικό ποσό: {total:C2}", 16, true, Theme.Green));
        root.Controls.Add(Theme.Label("Πρόκειται για εκπαιδευτική προσομοίωση. Δεν πραγματοποιείται πραγματική χρέωση.", 9, false, Theme.Muted));
        AddField(root, "Αριθμός κάρτας", _card);
        AddField(root, "Όνομα κατόχου", _name);

        var row = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var expiryBox = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown };
        expiryBox.Controls.Add(Theme.Label("Λήξη", 9, true));
        _expiry.Width = 180;
        expiryBox.Controls.Add(_expiry);
        var cvvBox = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown };
        cvvBox.Controls.Add(Theme.Label("CVV", 9, true));
        _cvv.Width = 180;
        cvvBox.Controls.Add(_cvv);
        row.Controls.Add(expiryBox);
        row.Controls.Add(cvvBox);
        root.Controls.Add(row);

        var actions = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
        var pay = Theme.Button("Επιβεβαίωση πληρωμής", Theme.Green, 220);
        pay.Click += (_, _) => ConfirmPayment();
        var cancel = Theme.Button("Ακύρωση", Theme.Muted, 120);
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        actions.Controls.Add(pay);
        actions.Controls.Add(cancel);
        root.Controls.Add(actions);
        Controls.Add(root);

        _card.TextChanged += (_, _) => FormatCard();
        _expiry.TextChanged += (_, _) => FormatExpiry();
        AcceptButton = pay;
        CancelButton = cancel;
    }

    private static void AddField(Control parent, string label, TextBox textBox)
    {
        parent.Controls.Add(Theme.Label(label, 9, true));
        textBox.Width = 390;
        textBox.Font = new Font("Segoe UI", 11f);
        textBox.Margin = new Padding(4, 0, 4, 8);
        parent.Controls.Add(textBox);
    }

    private void ConfirmPayment()
    {
        var digits = new string(_card.Text.Where(char.IsDigit).ToArray());
        if (digits.Length != 16 || !PassesLuhn(digits))
        {
            MessageBox.Show("Εισαγάγετε έγκυρο δοκιμαστικό αριθμό κάρτας 16 ψηφίων.", "Έλεγχος κάρτας", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(_name.Text) || !_expiry.Text.Contains('/') || _cvv.Text.Length != 3 || !_cvv.Text.All(char.IsDigit))
        {
            MessageBox.Show("Συμπληρώστε όνομα, ημερομηνία λήξης και τριψήφιο CVV.", "Ελλιπή στοιχεία", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MaskedCard = $"•••• •••• •••• {digits[^4..]}";
        DialogResult = DialogResult.OK;
    }

    private void FormatCard()
    {
        var digits = new string(_card.Text.Where(char.IsDigit).Take(16).ToArray());
        var formatted = string.Join(" ", Enumerable.Range(0, (digits.Length + 3) / 4)
            .Select(index => digits.Substring(index * 4, Math.Min(4, digits.Length - index * 4))));
        if (_card.Text == formatted) return;
        var selection = formatted.Length;
        _card.Text = formatted;
        _card.SelectionStart = selection;
    }

    private void FormatExpiry()
    {
        var digits = new string(_expiry.Text.Where(char.IsDigit).Take(4).ToArray());
        var formatted = digits.Length > 2 ? digits.Insert(2, "/") : digits;
        if (_expiry.Text == formatted) return;
        _expiry.Text = formatted;
        _expiry.SelectionStart = formatted.Length;
    }

    private static bool PassesLuhn(string digits)
    {
        var sum = 0;
        var alternate = false;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var value = digits[i] - '0';
            if (alternate)
            {
                value *= 2;
                if (value > 9) value -= 9;
            }
            sum += value;
            alternate = !alternate;
        }
        return sum % 10 == 0;
    }
}
