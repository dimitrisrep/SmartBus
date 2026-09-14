using SmartTouristBus.Models;

namespace SmartTouristBus.Services;

public sealed class SimulationEngine
{
    public static SimulationEngine Instance { get; } = new();

    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 800 };
    private readonly Random _random = new(2026);
    private readonly List<LostItem> _lostItems = new();
    private int _alightingTicks;
    private bool _speedAlertActive;
    private bool _laneAlertActive;
    private bool _fatigueAlertActive;
    private bool _robotFoundDemoItem;

    public IReadOnlyList<BusStop> Stops { get; } = BuildStops();

    public bool IsRunning { get; private set; }
    public int CurrentStopIndex { get; private set; }
    public double RouteProgress { get; private set; }
    public double Speed { get; private set; }
    public int SpeedLimit { get; private set; } = 40;
    public double LaneOffset { get; private set; }
    public double Fatigue { get; private set; } = 0.18;
    public bool DoorsOpen { get; private set; } = true;
    public bool PassengersAlighting => _alightingTicks > 0;
    public WeatherKind Weather { get; private set; } = WeatherKind.Sunny;
    public double OutsideTemperature { get; private set; } = 28;
    public double CabinTemperature { get; private set; } = 25;
    public double TargetTemperature { get; private set; } = 23;
    public ClimateMode ClimateMode { get; private set; } = ClimateMode.Cooling;
    public bool RoofOpen { get; private set; } = true;
    public double SolarProduction { get; private set; } = 12.5;
    public double EnergyConsumption { get; private set; } = 8.2;
    public double BatteryPercent { get; private set; } = 72;
    public RobotState RobotState { get; private set; } = RobotState.Idle;
    public double RobotProgress { get; private set; }

    public BusStop CurrentStop => Stops[CurrentStopIndex];
    public BusStop NextStop => Stops[(CurrentStopIndex + 1) % Stops.Count];

    public event EventHandler<BusSnapshot>? StateChanged;
    public event EventHandler<SystemAlert>? AlertRaised;

    private SimulationEngine()
    {
        _timer.Tick += (_, _) => Tick();
        _timer.Start();
    }

    public BusSnapshot Snapshot() => new()
    {
        IsRunning = IsRunning,
        CurrentStopIndex = CurrentStopIndex,
        RouteProgress = RouteProgress,
        Speed = Speed,
        SpeedLimit = SpeedLimit,
        LaneOffset = LaneOffset,
        Fatigue = Fatigue,
        DoorsOpen = DoorsOpen,
        PassengersAlighting = PassengersAlighting,
        Weather = Weather,
        OutsideTemperature = OutsideTemperature,
        CabinTemperature = CabinTemperature,
        TargetTemperature = TargetTemperature,
        ClimateMode = ClimateMode,
        RoofOpen = RoofOpen,
        SolarProduction = SolarProduction,
        EnergyConsumption = EnergyConsumption,
        BatteryPercent = BatteryPercent,
        RobotState = RobotState,
        RobotProgress = RobotProgress,
        LostItems = _lostItems.ToArray()
    };

    public void ToggleRunning()
    {
        IsRunning = !IsRunning;
        if (IsRunning && DoorsOpen)
            DoorsOpen = false;
        RaiseStateChanged();
    }

    public void SetWeather(WeatherKind weather)
    {
        Weather = weather;
        OutsideTemperature = weather switch
        {
            WeatherKind.Sunny => 29,
            WeatherKind.Cloudy => 23,
            WeatherKind.Rainy => 18,
            _ => OutsideTemperature
        };

        if (weather == WeatherKind.Rainy && RoofOpen)
        {
            RoofOpen = false;
            RaiseAlert("Οροφή", "Η οροφή έκλεισε αυτόματα για λόγους ασφαλείας λόγω βροχής.", true);
        }
        RaiseStateChanged();
    }

    public bool ToggleRoof(out string message)
    {
        if (!RoofOpen && Weather == WeatherKind.Rainy)
        {
            message = "Η οροφή δεν μπορεί να ανοίξει όσο βρέχει.";
            RaiseAlert("Οροφή", message, true);
            return false;
        }

        RoofOpen = !RoofOpen;
        message = RoofOpen ? "Η οροφή άνοιξε." : "Η οροφή έκλεισε.";
        RaiseAlert("Οροφή", message);
        RaiseStateChanged();
        return true;
    }

    public void SetClimate(ClimateMode mode, double target)
    {
        ClimateMode = mode;
        TargetTemperature = Math.Clamp(target, 17, 28);
        var before = EnergyConsumption;
        RecalculateEnergy();
        var saving = Math.Max(0, before - EnergyConsumption);
        RaiseAlert("Κλιματισμός", $"Νέα ρύθμιση: {ClimateLabel(mode)}, {TargetTemperature:0}°C. Εκτιμώμενη εξοικονόμηση {saving:0.0} kW.");
        RaiseStateChanged();
    }

    public void AdjustSpeed(double delta)
    {
        Speed = Math.Clamp(Speed + delta, 0, 80);
        if (Speed > 0 && DoorsOpen)
            DoorsOpen = false;
        CheckDrivingAlerts();
        RaiseStateChanged();
    }

    public void TriggerLaneDeparture()
    {
        LaneOffset = _random.Next(0, 2) == 0 ? -0.92 : 0.92;
        CheckDrivingAlerts();
        RaiseStateChanged();
    }

    public void TriggerFatigue()
    {
        Fatigue = 0.9;
        CheckDrivingAlerts();
        RaiseStateChanged();
    }

    public void TriggerAlighting()
    {
        IsRunning = false;
        Speed = 0;
        DoorsOpen = true;
        _alightingTicks = 10;
        RaiseAlert("Πόρτες", "Οι επιβάτες κατεβαίνουν. Μην κλείσετε τις πόρτες.", true);
        RaiseStateChanged();
    }

    public bool TryCloseDoors(out string message)
    {
        if (PassengersAlighting)
        {
            message = "Αδυναμία κλεισίματος: επιβάτες βρίσκονται ακόμη στην έξοδο.";
            RaiseAlert("Πόρτες", message, true);
            return false;
        }

        DoorsOpen = false;
        message = "Οι πόρτες έκλεισαν με ασφάλεια.";
        RaiseStateChanged();
        return true;
    }

    public void OpenDoors()
    {
        IsRunning = false;
        Speed = 0;
        DoorsOpen = true;
        RaiseStateChanged();
    }

    public void StartRobot()
    {
        if (RobotState == RobotState.Completed)
            RobotProgress = 0;
        RobotState = RobotState.Cleaning;
        _robotFoundDemoItem = false;
        RaiseAlert("Σκούπα-ρομπότ", "Ο αυτόνομος καθαρισμός ξεκίνησε. Τα πόδια της σκούπας εκτάθηκαν.");
        RaiseStateChanged();
    }

    public void PauseRobot()
    {
        RobotState = RobotState == RobotState.Cleaning ? RobotState.Paused : RobotState.Cleaning;
        RaiseStateChanged();
    }

    public void TriggerLostItem(string item = "Διαβατήριο")
    {
        var found = new LostItem(item, $"Άνω όροφος, κάθισμα {_random.Next(1, 30)}", DateTime.Now);
        _lostItems.Insert(0, found);
        RaiseAlert("Σκούπα-ρομπότ", $"Βρέθηκε πολύτιμο αντικείμενο: {found.Name}. Ειδοποιήθηκαν οδηγός, εταιρεία και επιβάτες ημέρας.", true);
        RaiseStateChanged();
    }

    public void ResetDemo()
    {
        IsRunning = false;
        CurrentStopIndex = 0;
        RouteProgress = 0;
        Speed = 0;
        SpeedLimit = 40;
        LaneOffset = 0;
        Fatigue = 0.18;
        DoorsOpen = true;
        _alightingTicks = 0;
        Weather = WeatherKind.Sunny;
        OutsideTemperature = 28;
        CabinTemperature = 25;
        TargetTemperature = 23;
        ClimateMode = ClimateMode.Cooling;
        RoofOpen = true;
        BatteryPercent = 72;
        RobotState = RobotState.Idle;
        RobotProgress = 0;
        _lostItems.Clear();
        _speedAlertActive = _laneAlertActive = _fatigueAlertActive = _robotFoundDemoItem = false;
        RecalculateEnergy();
        RaiseStateChanged();
    }

    private void Tick()
    {
        if (IsRunning)
        {
            Speed = Math.Clamp(Speed + (_random.NextDouble() - 0.45) * 2.6, 25, 51);
            RouteProgress += 0.012;
            Fatigue = Math.Clamp(Fatigue + 0.0025, 0, 1);
            LaneOffset *= 0.78;

            if (RouteProgress >= 1)
            {
                RouteProgress = 0;
                CurrentStopIndex = (CurrentStopIndex + 1) % Stops.Count;
                TriggerAlighting();
            }
        }

        if (_alightingTicks > 0)
            _alightingTicks--;

        var desiredCabin = ClimateMode switch
        {
            ClimateMode.Cooling => Math.Min(CabinTemperature, TargetTemperature),
            ClimateMode.Heating => Math.Max(CabinTemperature, TargetTemperature),
            _ => OutsideTemperature
        };
        CabinTemperature += (desiredCabin - CabinTemperature) * 0.08;

        if (RobotState == RobotState.Cleaning)
        {
            RobotProgress = Math.Clamp(RobotProgress + 0.018, 0, 1);
            if (RobotProgress > 0.58 && !_robotFoundDemoItem)
            {
                _robotFoundDemoItem = true;
                TriggerLostItem();
            }
            if (RobotProgress >= 1)
            {
                RobotState = RobotState.Completed;
                RaiseAlert("Σκούπα-ρομπότ", "Ο καθαρισμός ολοκληρώθηκε. Τα πόδια της σκούπας μαζεύτηκαν.");
            }
        }

        RecalculateEnergy();
        BatteryPercent = Math.Clamp(BatteryPercent + (SolarProduction - EnergyConsumption) * 0.006, 5, 100);
        CheckDrivingAlerts();
        RaiseStateChanged();
    }

    private void RecalculateEnergy()
    {
        SolarProduction = Weather switch
        {
            WeatherKind.Sunny => RoofOpen ? 14.8 : 11.2,
            WeatherKind.Cloudy => RoofOpen ? 6.3 : 4.9,
            WeatherKind.Rainy => 1.4,
            _ => 0
        };

        var climateLoad = ClimateMode == ClimateMode.Off
            ? 0
            : 1.8 + Math.Abs(TargetTemperature - OutsideTemperature) * 0.22;
        EnergyConsumption = 5.1 + climateLoad + (IsRunning ? 2.2 : 0.7) + (RobotState == RobotState.Cleaning ? 0.9 : 0);
    }

    private void CheckDrivingAlerts()
    {
        var speeding = Speed > SpeedLimit;
        if (speeding && !_speedAlertActive)
            RaiseAlert("Οδήγηση", $"Υπέρβαση ορίου: {Speed:0} km/h με όριο {SpeedLimit} km/h.", true);
        _speedAlertActive = speeding;

        var laneDeparture = Math.Abs(LaneOffset) > 0.7;
        if (laneDeparture && !_laneAlertActive)
            RaiseAlert("Οδήγηση", "Προειδοποίηση εγκατάλειψης λωρίδας. Επιστρέψτε στο κέντρο.", true);
        _laneAlertActive = laneDeparture;

        var tired = Fatigue > 0.8;
        if (tired && !_fatigueAlertActive)
            RaiseAlert("Οδήγηση", "Ανιχνεύθηκε κόπωση. Σκεφτείτε μήπως θέλετε καφέ.", true);
        _fatigueAlertActive = tired;
    }

    private void RaiseAlert(string source, string message, bool critical = false)
        => AlertRaised?.Invoke(this, new SystemAlert(source, message, DateTime.Now, critical));

    private void RaiseStateChanged() => StateChanged?.Invoke(this, Snapshot());

    public static string WeatherLabel(WeatherKind weather) => weather switch
    {
        WeatherKind.Sunny => "Ηλιοφάνεια",
        WeatherKind.Cloudy => "Συννεφιά",
        WeatherKind.Rainy => "Βροχή",
        _ => weather.ToString()
    };

    public static string ClimateLabel(ClimateMode mode) => mode switch
    {
        ClimateMode.Off => "Απενεργοποιημένο",
        ClimateMode.Cooling => "Ψύξη",
        ClimateMode.Heating => "Θέρμανση",
        _ => mode.ToString()
    };

    private static IReadOnlyList<BusStop> BuildStops() => new List<BusStop>
    {
        new("Σύνταγμα", "Ιστορικό κέντρο", "Σταθμός Μετρό Συντάγματος",
            new List<Attraction>
            {
                new("Βουλή των Ελλήνων", "Ιστορία", "Το κτήριο της Βουλής δεσπόζει στην Πλατεία Συντάγματος. Μπροστά του πραγματοποιείται η αλλαγή φρουράς των Ευζώνων.", "Περπατήστε 2 λεπτά προς την ανατολική πλευρά της πλατείας."),
                new("Εθνικός Κήπος", "Περίπατος", "Ένας ήρεμος ιστορικός κήπος στο κέντρο της Αθήνας, κατάλληλος για περίπατο και ξεκούραση.", "Διασχίστε τη λεωφόρο Αμαλίας από τη διάβαση πεζών.")
            },
            new List<string> { "Athens Garden Bistro", "Plaka Taste" }),
        new("Ακρόπολη", "Μακρυγιάννη", "Σταθμός Μετρό Ακρόπολη",
            new List<Attraction>
            {
                new("Ιερός Βράχος Ακρόπολης", "Αρχαιολογία", "Το σημαντικότερο μνημειακό σύνολο της κλασικής Αθήνας, με κορυφαίο μνημείο τον Παρθενώνα.", "Ακολουθήστε την οδό Διονυσίου Αρεοπαγίτου για 8 λεπτά."),
                new("Μουσείο Ακρόπολης", "Μουσείο", "Σύγχρονο μουσείο με ευρήματα από τον Ιερό Βράχο και πανοραμική θέα προς τον Παρθενώνα.", "Το μουσείο βρίσκεται απέναντι από τη στάση, σε απόσταση 3 λεπτών.")
            },
            new List<string> { "Acropolis View", "Dionysos Café" }),
        new("Μοναστηράκι", "Παλιά Αθήνα", "Σταθμός Μετρό Μοναστηράκι",
            new List<Attraction>
            {
                new("Αρχαία Αγορά", "Αρχαιολογία", "Η καρδιά της δημόσιας ζωής της αρχαίας Αθήνας με τον εντυπωσιακό Ναό του Ηφαίστου.", "Περπατήστε 6 λεπτά μέσω της οδού Αδριανού."),
                new("Βιβλιοθήκη Αδριανού", "Ιστορία", "Μνημειακό συγκρότημα που ίδρυσε ο Ρωμαίος αυτοκράτορας Αδριανός το 132 μ.Χ.", "Βρίσκεται 150 μέτρα βόρεια της πλατείας.")
            },
            new List<string> { "Agora Kitchen", "Monastiraki Rooftop" }),
        new("Εθνικό Αρχαιολογικό Μουσείο", "Εξάρχεια", "Είσοδος σταθμού Μετρό Ομόνοια",
            new List<Attraction>
            {
                new("Εθνικό Αρχαιολογικό Μουσείο", "Μουσείο", "Το μεγαλύτερο αρχαιολογικό μουσείο της Ελλάδας με εκθέματα από όλο τον αρχαίο ελληνικό κόσμο.", "Η είσοδος βρίσκεται επί της οδού 28ης Οκτωβρίου.")
            },
            new List<string> { "Museum Café", "Patission Deli" }),
        new("Καλλιμάρμαρο", "Παγκράτι", "Στεγασμένος χώρος αναμονής Ζαππείου",
            new List<Attraction>
            {
                new("Παναθηναϊκό Στάδιο", "Αθλητισμός", "Το μαρμάρινο στάδιο που φιλοξένησε τους πρώτους σύγχρονους Ολυμπιακούς Αγώνες το 1896.", "Η κεντρική είσοδος βρίσκεται απέναντι από τη στάση."),
                new("Ζάππειο Μέγαρο", "Αρχιτεκτονική", "Νεοκλασικό κτήριο μέσα στους Εθνικούς Κήπους, συνδεδεμένο με τη σύγχρονη ιστορία των Ολυμπιακών Αγώνων.", "Περπατήστε 7 λεπτά μέσα από τον κήπο.")
            },
            new List<string> { "Stadium Bites", "Pangrati Local" })
    };
}
