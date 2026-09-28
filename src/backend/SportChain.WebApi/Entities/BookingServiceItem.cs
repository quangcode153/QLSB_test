namespace SportChain.WebApi.Entities;

public class BookingServiceItem
{
    public int Id { get; set; }

    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public int ServiceItemId { get; set; }
    public ServiceItem ServiceItem { get; set; } = null!;

    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal SubTotal => Quantity * UnitPrice;
}
