using System.ComponentModel.DataAnnotations;

namespace SportChain.WebApi.Entities;

public class Complaint
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public int? BookingId { get; set; }
    public Booking? Booking { get; set; }

    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Status { get; set; } = "Chờ xử lý"; // Chờ xử lý, Đang giải quyết, Đã giải quyết, Từ chối

    [MaxLength(1000)]
    public string? Resolution { get; set; } // Phán quyết từ Admin

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
