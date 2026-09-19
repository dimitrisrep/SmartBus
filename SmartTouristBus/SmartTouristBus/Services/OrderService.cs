using SmartTouristBus.Models;

namespace SmartTouristBus.Services;

public sealed class OrderService
{
    public static OrderService Instance { get; } = new();
//dimiourgise ena koino antikeimeno orderservice pou mporei na xrisimopoihthei se oli tin efarmogi

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

    public IReadOnlyList<CafeOrder> Orders => _orders.AsReadOnly(); //to => einai get
//h pragmatiki lista einai _orders kai einai private. opote dinoume thn morfi Orders mono se read-only morfi

    public event EventHandler<CafeOrder>? OrderPlaced;//dilwnw ena public event me onoma OrderChanged. Otan energopoieitai
    //metaferei mazit tou ena CafeOrder. To ? shmainei oti mporei na einai null. An den exei kapoios eggraftei sto event, tote den tha ginei tipota.
    //subscribe kanw se event me += 

    public event EventHandler<CafeOrder>? OrderChanged;

    private OrderService() { } //gia na min dimiourgoun oi alloi alla instances

    public CafeOrder PlaceOrder(
        IEnumerable<OrderLine> lines, //mia sillogi apo orderline
        string seat, 
        string maskedCard)
    {
        var order = new CafeOrder {
            PassengerSeat = seat, 
            MaskedCard = maskedCard 
        };
        order.Lines.AddRange( //apo ola ta lines, krata mono osa exoun quantity>0 kai prosthese ta sti lista order.lines
            lines.Where(line => line.Quantity > 0)//filtrare ti sillogi kai krata mono tis grammes opou h posotita einai >0
         );
        if (order.Lines.Count == 0)
            throw new InvalidOperationException("Το καλάθι είναι κενό.");
//throw= stamata edw kai peta sfalma.

        _orders.Insert(0, order); //vale to order sti thesi 0 tis listas _orders
        OrderPlaced?.Invoke(this, order);//invoke to energopoiei an den einai null (?.)
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
