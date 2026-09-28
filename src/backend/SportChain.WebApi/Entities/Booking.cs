using System.ComponentModel.DataAnnotations;
using SportChain.Shared.Enums;

namespace SportChain.WebApi.Entities;

public class Booking
{
    public int Id { get; set; }

    [MaxLength(30)]
    public string BookingCode { get; set; } = string.Empty; // Mã hiển thị: SC-20261001-XXXX

    [MaxLength(64)]
    public string CheckInCode { get; set; } = Guid.NewGuid().ToString("N"); // Mã QR bảo mật

    public int CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public int CourtId { get; set; }
    public Court Court { get; set; } = null!;

    public DateOnly BookingDate { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal RemainingAmount => TotalAmount - DepositAmount;

    public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;
    public PaymentMethod? PaymentMethod { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpireHoldingAt { get; set; } // Hết 15 phút giữ chỗ
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    [MaxLength(500)]
    public string? CancellationReason { get; set; }
    public decimal RefundAmount { get; set; } = 0;

    public bool IsRecurring { get; set; } = false; // Lịch định kỳ

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    public ICollection<BookingDetail> Details { get; set; } = new List<BookingDetail>();
    public ICollection<BookingServiceItem> ServiceItems { get; set; } = new List<BookingServiceItem>();
}
