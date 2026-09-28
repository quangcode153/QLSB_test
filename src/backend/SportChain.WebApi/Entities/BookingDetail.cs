namespace SportChain.WebApi.Entities;

public class BookingDetail
{
    public int Id { get; set; }

    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public int TimeSlotId { get; set; }
    public TimeSlot TimeSlot { get; set; } = null!;

    public decimal SlotPrice { get; set; }
}
