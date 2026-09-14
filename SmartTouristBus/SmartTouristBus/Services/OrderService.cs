using SmartTouristBus.Models;

namespace SmartTouristBus.Services;

public sealed class OrderService
{
    public static OrderService Instance { get; } = new();

    private readonly List<CafeOrder> _orders = new();

    public IReadOnlyList<CafeProduct> Products { get; } = new List<CafeProduct>
    {
        new(1, "Espresso", "Καφές", 2.20m),
        new(2, "Cappuccino", "Καφές", 3.20m),
        new(3, "Freddo Espresso", "Καφές", 3.00m),
        new(4, "Τσάι βοτάνων", "Ρόφημα", 2.60m),
        new(5, "Ζεστή σοκολάτα", "Ρόφημα", 3.40m),
        new(6, "Νερό 500 ml", "Αναψυκτικό", 0.80m),
        new(7, "Φυσικός χυμός", "Αναψυκτικό", 3.50m),
        new(8, "Κουλούρι Θεσσαλονίκης", "Μικρό γεύμα", 1.50m),
        new(9, "Τοστ γαλοπούλα-τυρί", "Μικρό γεύμα", 3.80m),
        new(10, "Μπάρα δημητριακών", "Μικρό γεύμα", 1.90m)
    };

    public IReadOnlyList<CafeOrder> Orders => _orders.AsReadOnly();

    public event EventHandler<CafeOrder>? OrderPlaced;
    public event EventHandler<CafeOrder>? OrderChanged;

    private OrderService() { }

    public CafeOrder PlaceOrder(IEnumerable<OrderLine> lines, string seat, string maskedCard)
    {
        var order = new CafeOrder { PassengerSeat = seat, MaskedCard = maskedCard };
        order.Lines.AddRange(lines.Where(line => line.Quantity > 0));
        if (order.Lines.Count == 0)
            throw new InvalidOperationException("Το καλάθι είναι κενό.");

        _orders.Insert(0, order);
        OrderPlaced?.Invoke(this, order);
        return order;
    }

    public void Update(CafeOrder order, OrderStatus status, string? deliveryStop = null)
    {
        order.Status = status;
        if (!string.IsNullOrWhiteSpace(deliveryStop))
            order.DeliveryStop = deliveryStop;
        OrderChanged?.Invoke(this, order);
    }
}
