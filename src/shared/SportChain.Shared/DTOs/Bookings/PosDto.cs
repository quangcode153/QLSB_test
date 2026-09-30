using SportChain.Shared.Enums;

namespace SportChain.Shared.DTOs.Bookings;

public class ServiceItemDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Drink";
    public decimal Price { get; set; }
    public bool IsRental { get; set; }
}

public class AddServiceItemRequest
{
    public int BookingId { get; set; }
    public int ServiceItemId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class WalkInBookingRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public int CourtId { get; set; }
    public DateOnly BookingDate { get; set; }
    public List<int> TimeSlotIds { get; set; } = new();
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
}

public class CompletePaymentRequest
{
    public int BookingId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
}

public class PosInvoiceDto
{
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public DateOnly BookingDate { get; set; }
    public List<string> SlotLabels { get; set; } = new();
    public decimal CourtRentAmount { get; set; }
    public decimal DepositPaid { get; set; }
    public List<BookingServiceItemSummaryDto> Services { get; set; } = new();
    public decimal TotalServicesAmount { get; set; }
    public decimal RemainingAmountToPay { get; set; }
    public BookingStatus Status { get; set; }
}

public class BookingServiceItemSummaryDto
{
    public int ServiceItemId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal => Quantity * UnitPrice;
}
