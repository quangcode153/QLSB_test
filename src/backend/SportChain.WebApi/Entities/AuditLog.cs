using System.ComponentModel.DataAnnotations;

namespace SportChain.WebApi.Entities;

public class AuditLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    [MaxLength(100)]
    public string Action { get; set; } = string.Empty; // CancelBooking, ModifyPrice, ApproveBranch

    [MaxLength(100)]
    public string EntityName { get; set; } = string.Empty; // Booking, PriceRule, Branch

    public int? EntityId { get; set; }

    [MaxLength(1000)]
    public string Details { get; set; } = string.Empty; // Lý do hủy, giá trị cũ -> giá trị mới

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
