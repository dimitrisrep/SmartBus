using SmartTouristBus.Models;
using SmartTouristBus.Services;
using SmartTouristBus.UI;

namespace SmartTouristBus.Forms;

public sealed class MainForm : AppFormBase
{
    private readonly SimulationEngine _engine = SimulationEngine.Instance;
    private readonly Label _status = new(); //dimiourgw label pou tha deixnei to status tou leoforeiou
    private readonly Button _runButton;

    protected override string HelpTopic => "Αρχική"; //poio topic antistixei sto help

    public MainForm() : base("Το Έξυπνο Τουριστικό Λεωφορείο") // to main form einai o constructor ths main form. to : base leei
        //leei prin sinexiseis, kalese to construcot ths appformbase kai dwse tou afton ton titlo
    {
        Size = new Size(1160, 790); //megethos formas

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, //piase olon ton available xwro tis formas
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(20), //20 pixels eswteriko perithwrio gyrw apo to periexomeno
            BackColor = Theme.Pale
        };
        //kathorizw ta ipsi twn 3 grammwn
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 170)); //170 pixels
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));//100% ypolipou xwrou
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));

        root.Controls.Add(new CityBanner(), 0, 0);
        
        //var = ase thn C# na katalavei monh ton typo ths metavlitis apo afto pou tis dinw
        var roleGrid = new TableLayoutPanel { 
            Dock = DockStyle.Fill, 
            ColumnCount = 4, 
            RowCount = 1, 
            Padding = new Padding(0, 16, 0, 8) }; 
        //afto dimiourgei pinaka me 4 stilles kai mia grammi.

        for (var i = 0; i < 4; i++) 
            roleGrid.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 25)
                ); //gia kathe fore prosthese mia stili pou pairnei 25% tou xwrou.

        roleGrid.Controls.Add(
            RoleCard( //h methodos RoleCard kataskevazei ena kartelaki epilogis. dinw title, description, color, ti tha
                      //ginei otan patithei to kartelaki   
                "Επιβάτης", 
                "Θέα, αξιοθέατα, παραγγελίες και τουριστική πλοήγηση.", 
                Theme.Blue, 
                () => new PassengerForm().Show(this)), //otaon patithei anoikse passengerForm. () => einai lambda expression pou leei oti otan patithei to kartelaki, kalesa afti thn methodo
            0, //stili
            0 //grammi
         );
        
        roleGrid.Controls.Add(RoleCard(
            "Οδηγός", 
            "Ταχύτητα, λωρίδα, κόπωση, πόρτες και θερμοκρασία.",
            Theme.Coral, 
            () => new DriverForm().Show(this)), 
            1, 
            0
         );
        
        roleGrid.Controls.Add(RoleCard(
            "Υπάλληλος", 
            "Οροφή, φωτοβολταϊκά, ενέργεια και σκούπα-ρομπότ.", 
            Theme.Green, 
            () => new EmployeeForm().Show(this)), 
            2, 
            0
         );
       
        roleGrid.Controls.Add(RoleCard(
            "Καφετέρια", 
            "Παραλαβή παραγγελιών και ορισμός στάσης παράδοσης.", 
            Theme.Gold, 
            () => new CafeteriaForm().Show(this)), 
            3, 
            0
         );
        // SOS= TO Lambda expression apothikevei thn ektelesi tis energias. den kanei to activation apo mono
        root.Controls.Add(roleGrid, 0, 1); //bazei olo afto to roleGrid sti stili 0,grammi 1 tou megalou root.

        var footer = Theme.Card(12); //to 12 einai parametros emfaniseis
        footer.Dock = DockStyle.Fill;
        var footerLayout = new TableLayoutPanel {
            Dock = DockStyle.Fill, //dimiourgw ena table layouy panel me 2 columns
             ColumnCount = 2 
        };
        footerLayout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100)
            ); //prwth stili pairnei 100% tou xwrou pou perisevei
        footerLayout.ColumnStyles.Add(
            new ColumnStyle(SizeType.AutoSize)
            ); //deuteri stili pairnei auto to xwro pou xreiazetai gia controls

        _status.AutoSize = true;
        _status.Font = new Font("Segoe UI Semibold", 10.5f);
        _status.ForeColor = Theme.Navy;
        _status.Anchor = AnchorStyles.Left;

        var buttons = new FlowLayoutPanel { //vazei ta buttons se seira
            AutoSize = true, 
            FlowDirection = FlowDirection.LeftToRight, // apo aristera sta deksia 
            WrapContents = false 
        };
        _runButton = Theme.Button(
            "Έναρξη διαδρομής", 
            Theme.Green, 
            175
         );
        _runButton.Click += (_, _) => _engine.ToggleRunning();
        //otan ginei click sto runbutton, kalese to enginge. allazei se kinisi/stamataei
       
        var reset = Theme.Button(
            "Επαναφορά επίδειξης",
            Theme.Muted, 
            180
         );
        reset.Click += (_, _) => _engine.ResetDemo();
        //otan ginei click, epanefere tin prosomiwsi se arxiki katastasi

        var help = Theme.Button(
            "Βοήθεια (F1)", 
            Theme.Blue,
            135
            );

        help.Click += (_, _) => HelpForm.ShowTopic("Παρουσίαση", this);
        buttons.Controls.AddRange(
            new Control[] { _runButton, reset, help }
            ); //prosthetei ta 3 buttons sto flow layout panel

        footerLayout.Controls.Add(_status, 0, 0); //stili 0 status
        footerLayout.Controls.Add(buttons, 1, 0); //stili 1 ta buttons
        footer.Controls.Add(footerLayout); //vazei to footerLayout mesa sto footer
        root.Controls.Add(footer, 0, 2); //vazei to footer sti grammi 2 tou vasikou root

        Controls.Add(root); //to controls anikei sto mainform.
        //ara vale olo to root mesa stin forma

        //afta einai event subscription/unsubscription
        _engine.StateChanged += EngineOnStateChanged; //otan allaksei h katastasi to simulation engine, kalese tin engineOnStateChanged
        FormClosed += (_, _) =>  // += einai sindese afti tin energia me afto to event. ta kena einai parametroi
        _engine.StateChanged -= EngineOnStateChanged;// otan kleisei h forma, stamata na akous to event
        UpdateStatus(_engine.Snapshot());
    }

    private static Panel RoleCard( //method pou epistrefei panel
        string title, 
        string description, 
        Color accent, 
        Action open) //h energia pou tha eketelesei otan patisei to koumpi
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

    //pairnei 2 pragmata: poios kalese to event kai to kainourgio snapshot. to snapshot einai h katastasi tou leoforeiou                                       
    private void EngineOnStateChanged(object? sender, BusSnapshot snapshot)
        => UpdateStatus(snapshot);

    private void UpdateStatus(BusSnapshot snapshot)
    {
        if (snapshot.IsRunning)
        {
            _status.Text =
                $"● Σε κίνηση — επόμενη στάση: {_engine.NextStop.Name} — {snapshot.Speed:0} km/h";

            _status.ForeColor = Theme.Green;

            _runButton.Text = "Παύση διαδρομής";
        }
        else
        {
            string doorText;

            if (snapshot.DoorsOpen)
            {
                doorText = "ανοιχτές";
            }
            else
            {
                doorText = "κλειστές";
            }

            _status.Text =
                $"● Στάση: {_engine.CurrentStop.Name} — πόρτες {doorText}";

            _status.ForeColor = Theme.Navy;

            _runButton.Text = "Έναρξη διαδρομής";
        }
    }
}
