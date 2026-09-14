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
    public string Id { get; } = $"STB-{DateTime.Now:HHmmss}";
    public DateTime CreatedAt { get; } = DateTime.Now;
    public List<OrderLine> Lines { get; } = new();
    public decimal Total => Lines.Sum(line => line.Total);
    public OrderStatus Status { get; set; } = OrderStatus.Submitted;
    public string DeliveryStop { get; set; } = "Δεν έχει οριστεί";
    public string PassengerSeat { get; set; } = "12A";
    public string MaskedCard { get; set; } = string.Empty;

    public override string ToString() => $"{Id} | {Total:C2} | {StatusLabel(Status)}";

    public static string StatusLabel(OrderStatus status) => status switch
    {
        OrderStatus.Submitted => "Υποβλήθηκε",
        OrderStatus.Accepted => "Έγινε αποδεκτή",
        OrderStatus.Preparing => "Προετοιμάζεται",
        OrderStatus.ReadyForDelivery => "Έτοιμη για παράδοση",
        OrderStatus.Delivered => "Παραδόθηκε",
        OrderStatus.Cancelled => "Ακυρώθηκε",
        _ => status.ToString()
    };
}

public sealed record LostItem(string Name, string Location, DateTime FoundAt)
{
    public override string ToString() => $"{Name} — {Location} ({FoundAt:HH:mm})";
}

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
}

public sealed record SystemAlert(string Source, string Message, DateTime Timestamp, bool Critical = false)
{
    public override string ToString() => $"{Timestamp:HH:mm:ss}  {Message}";
}
