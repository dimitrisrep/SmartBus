namespace SmartTouristBus.Models;

public enum WeatherKind { Sunny, Cloudy, Rainy }
public enum ClimateMode { Off, Cooling, Heating }
public enum RobotState { Idle, Cleaning, Paused, Completed }
public enum OrderStatus { Submitted, Accepted, Preparing, ReadyForDelivery, Delivered, Cancelled }

public sealed record Attraction(string Name, string Category, string Description, string WalkingDirections);

public sealed record BusStop(
    string Name,
    string Area,
    string SafeShelter,
    IReadOnlyList<Attraction> Attractions,
    IReadOnlyList<string> Restaurants);

public sealed record CafeProduct(int Id, string Name, string Category, decimal Price);

public sealed record OrderLine(CafeProduct Product, int Quantity)
{
    public decimal Total => Product.Price * Quantity;
}

public sealed class CafeOrder
{
    public string Id { get; } = $"STB-{DateTime.Now:HHmmss}"; //to get shmainei oti mporw na dw tin timi, alla den mporw meta na tin allaksw,
    //den yparxei setter gia na to kanw. to ypolipo dimiourgei ID basei time. me to $ bazw times se string me {}.
    public DateTime CreatedAt { get; } = DateTime.Now;
    public List<OrderLine> Lines { get; } = new(); //exw ena public property pou legetai Lines, kai einai lista apo to OrderLine. me to get simainei
    //oti mporw na valw kai na vgalw items stin lista, alla den mporw na allaksw to Lines se allo object. me to new() dimiourgei ena kainourgio object apo tin lista.
    public decimal Total => Lines.Sum(line => line.Total);
    public OrderStatus Status { get; set; } = OrderStatus.Submitted;
    public string DeliveryStop { get; set; } = "Δεν έχει οριστεί";
    public string PassengerSeat { get; set; } = "12A";
    public string MaskedCard { get; set; } = "";

    public override string ToString() => $"{Id} | {Total:C2} | {StatusLabel(Status)}";
    //episteftei string pou periexei paraggelia. C2 einai format gia C= currency,2= dyo dekadika psifia.
    //To $ simainei oti mporw na valw times se string me {}. to StatusLabel einai methodos pou pairnei status kai epistrefei string.
    // yparxei {} giati oi times prepei na ypologistoun kai meta na ginoun string

    public static string StatusLabel(OrderStatus status) => status switch
    {
        OrderStatus.Submitted => "Υποβλήθηκε",
        OrderStatus.Accepted => "Έγινε αποδεκτή",
        OrderStatus.Preparing => "Προετοιμάζεται",
        OrderStatus.ReadyForDelivery => "Έτοιμη για παράδοση",
        OrderStatus.Delivered => "Παραδόθηκε",
        OrderStatus.Cancelled => "Ακυρώθηκε",
        _ => status.ToString() //an gia kapoion logo den yparxei status, epestrepse to idio status san string
    };
}
//dimiourgw record me ta xamena items pou vriskei h skoupa. exoun name, location,time pou vrethikan
//
public sealed record LostItem(string Name, string Location, DateTime FoundAt)
{
    public override string ToString() => $"{Name} — {Location} ({FoundAt:HH:mm})";
}
//to init moiazei me set, alla epitrepei na dwseis timi mono otan dimiourgeitai to object. onomazetai objecxt initializer, dineis timi stin arxi, kai meta tin afineis opws einai, den allazei

public sealed class BusSnapshot
{
    public bool IsRunning { get; init; }
    public int CurrentStopIndex { get; init; }
    public double RouteProgress { get; init; }
    public double Speed { get; init; }
    public int SpeedLimit { get; init; }
    public double LaneOffset { get; init; }
    public double Fatigue { get; init; }
    public bool DoorsOpen { get; init; }
    public bool PassengersAlighting { get; init; }
    public WeatherKind Weather { get; init; }
    public double OutsideTemperature { get; init; }
    public double CabinTemperature { get; init; }
    public double TargetTemperature { get; init; }
    public ClimateMode ClimateMode { get; init; }
    public bool RoofOpen { get; init; }
    public double SolarProduction { get; init; }
    public double EnergyConsumption { get; init; }
    public double BatteryPercent { get; init; }
    public RobotState RobotState { get; init; }
    public double RobotProgress { get; init; }
    public IReadOnlyList<LostItem> LostItems { get; init; } = Array.Empty<LostItem>();
} //ksekina me mia lista list[] pou einai kenh kai tha exei lost items.

public sealed record SystemAlert(
    string Source, 
    string Message, 
    DateTime Timestamp, 
    bool Critical = false)
{
    public override string ToString() => $"{Timestamp:HH:mm:ss}  {Message}";
}
//perigrafei ena system alert. kathe system alert exei 4 plirofories= source: apo pou proerxetai h eidopoihsh
// message: to minima pou emfanizetai, timestamp: h wra pou dimiourgithike, critical: an einai kritiko to minima. to critical exei default timi false, opote an den dwsei o xristis timi, tha einai false.