using SportChain.Shared.Enums;

namespace SportChain.Shared.DTOs.Bookings;

public class CreateBookingRequest
{
    public int CustomerId { get; set; }
    public int BranchId { get; set; }
    public int CourtId { get; set; }
    public DateOnly BookingDate { get; set; }
    public List<int> TimeSlotIds { get; set; } = new();
    public List<int> ServiceItemIds { get; set; } = new();
}

public class ConfirmDepositRequest
{
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.VietQr;
}

public class BookingDto
{
    public int Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string CheckInCode { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int CourtId { get; set; }
    public string CourtName { get; set; } = string.Empty;
    public DateOnly BookingDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal ServiceAmount { get; set; }
    public decimal RemainingAmount => TotalAmount - DepositAmount;
    public BookingStatus Status { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpireHoldingAt { get; set; }
    public List<int> TimeSlotIds { get; set; } = new();
    public List<string> SlotLabels { get; set; } = new();
    public string? QrCodeBase64 { get; set; }
}

public class CheckInRequest
{
    public string CheckInCode { get; set; } = string.Empty;
}

public class CancelBookingRequest
{
    public int BookingId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
