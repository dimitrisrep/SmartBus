using SmartTouristBus.UI;

namespace SmartTouristBus.Forms;

public sealed class HelpForm : Form
{
    private readonly TreeView _topics = new();
    private readonly RichTextBox _content = new();

    private static readonly Dictionary<string, string> Articles = new()
    {
        ["Αρχική"] = "Καλώς ήρθατε στο on-line help του Έξυπνου Τουριστικού Λεωφορείου.\n\nΑπό την αρχική οθόνη μπορείτε να ανοίξετε ταυτόχρονα τις οθόνες Επιβάτη, Οδηγού, Υπαλλήλου και Καφετέριας. Όλες μοιράζονται την ίδια προσομοίωση.\n\nΠατήστε F1 σε οποιαδήποτε οθόνη για βοήθεια σχετική με τον τρέχοντα ρόλο.",
        ["Επιβάτης"] = "Η οθόνη του επιβάτη περιλαμβάνει τέσσερις καρτέλες:\n\n• Θέα διαδρομής: ζωντανή προσομοίωση του οπτικού πεδίου του οδηγού.\n• Αξιοθέατα: πληροφορίες, οδηγίες και ακρόαση περιγραφών.\n• Καφετέρια: κατάλογος, καλάθι, πληρωμή με κάρτα και παρακολούθηση παραγγελίας.\n• Πλοήγηση: οδηγίες προς αξιοθέατα, εστιατόρια, ασφαλές καταφύγιο και κοντινότερη στάση.",
        ["Οδηγός"] = "Η οθόνη οδηγού εμφανίζει ταχύτητα, όριο, θέση στη λωρίδα, κόπωση, πόρτες και θερμοκρασία.\n\nΤο σύστημα προειδοποιεί με μήνυμα και ήχο για υπέρβαση ταχύτητας, εγκατάλειψη λωρίδας και κόπωση. Δεν επιτρέπει το κλείσιμο των θυρών όταν επιβάτες βρίσκονται στην έξοδο.\n\nΤα κουμπιά σεναρίων χρησιμοποιούνται για γρήγορη επίδειξη κατά την εξέταση.",
        ["Υπάλληλος"] = "Η κονσόλα υπαλλήλου ελέγχει αποκλειστικά την οροφή, τον καιρό της προσομοίωσης, το κλιματιστικό, την ενέργεια και τη σκούπα-ρομπότ.\n\nΗ οροφή δεν ανοίγει όταν βρέχει. Οι μετρήσεις παραγωγής, κατανάλωσης και μπαταρίας ενημερώνονται σε πραγματικό χρόνο.\n\nΗ σκούπα μπορεί να αναγνωρίσει πολύτιμο αντικείμενο και να ειδοποιήσει όλους τους ενδιαφερόμενους.",
        ["Καφετέρια"] = "Η συνεργαζόμενη καφετέρια βλέπει τις παραγγελίες μόλις υποβληθούν από τον επιβάτη.\n\nΕπιλέξτε μια παραγγελία, ορίστε την επόμενη στάση παράδοσης και ενημερώστε διαδοχικά την κατάστασή της: αποδοχή, προετοιμασία, έτοιμη και παραδόθηκε.",
        ["Παρουσίαση"] = "Προτεινόμενο σενάριο παρουσίασης:\n\n1. Ανοίξτε και τις τέσσερις οθόνες.\n2. Ξεκινήστε τη διαδρομή.\n3. Υποβάλετε παραγγελία από τον Επιβάτη.\n4. Ορίστε στάση παράδοσης από την Καφετέρια.\n5. Δείξτε τις προειδοποιήσεις οδηγού.\n6. Αλλάξτε τον καιρό σε βροχή και δείξτε την οροφή/ενέργεια.\n7. Ξεκινήστε τη σκούπα και προσομοιώστε εύρεση διαβατηρίου.",
        ["Αντιμετώπιση προβλημάτων"] = "Αν η εφαρμογή δεν ξεκινά:\n\n• Βεβαιωθείτε ότι χρησιμοποιείτε Windows και Visual Studio 2022.\n• Εγκαταστήστε το workload .NET desktop development.\n• Ανοίξτε το SmartTouristBus.sln και περιμένετε να ολοκληρωθεί η φόρτωση.\n• Επιλέξτε Build > Rebuild Solution και έπειτα πατήστε F5."
    };

    private HelpForm(string topic)
    {
        Theme.ApplyForm(this, "On-line Help — Smart Tourist Bus", new Size(820, 560));
        Size = new Size(920, 650);

        var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = Theme.Navy, Padding = new Padding(22, 12, 10, 10) };
        header.Controls.Add(new Label
        {
            Text = "Βοήθεια εφαρμογής",
            Dock = DockStyle.Fill,
            Font = Theme.TitleFont,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft
        });

        _topics.Dock = DockStyle.Fill;
        _topics.BorderStyle = BorderStyle.None;
        _topics.Font = Theme.BodyFont;
        foreach (var title in Articles.Keys)
            _topics.Nodes.Add(title, title);

        _content.Dock = DockStyle.Fill;
        _content.ReadOnly = true;
        _content.BorderStyle = BorderStyle.None;
        _content.BackColor = Color.White;
        _content.Font = new Font("Segoe UI", 11f);
        _content.Padding = new Padding(12);

        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 230, BackColor = Theme.Pale };
        var left = Theme.Card(10);
        left.Dock = DockStyle.Fill;
        left.Controls.Add(_topics);
        var right = Theme.Card(24);
        right.Dock = DockStyle.Fill;
        right.Controls.Add(_content);
        split.Panel1.Padding = new Padding(10);
        split.Panel2.Padding = new Padding(0, 10, 10, 10);
        split.Panel1.Controls.Add(left);
        split.Panel2.Controls.Add(right);

        Controls.Add(split);
        Controls.Add(header);

        _topics.AfterSelect += (_, args) => Display(args.Node.Text);
        var node = _topics.Nodes.Find(Articles.ContainsKey(topic) ? topic : "Αρχική", false).FirstOrDefault();
        _topics.SelectedNode = node ?? _topics.Nodes[0];
    }

    public static void ShowTopic(string topic, IWin32Window owner)
    {
        using var help = new HelpForm(topic);
        help.ShowDialog(owner);
    }

    private void Display(string title)
    {
        _content.Clear();
        _content.SelectionFont = new Font("Segoe UI Semibold", 17f);
        _content.SelectionColor = Theme.Navy;
        _content.AppendText(title + Environment.NewLine + Environment.NewLine);
        _content.SelectionFont = new Font("Segoe UI", 11f);
        _content.SelectionColor = Theme.Ink;
        _content.AppendText(Articles[title]);
    }
}
