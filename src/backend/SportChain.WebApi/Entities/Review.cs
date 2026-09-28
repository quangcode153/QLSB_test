using System.ComponentModel.DataAnnotations;

namespace SportChain.WebApi.Entities;

public class Review
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; } = 5; // 1 đến 5 sao

    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? ManagerReply { get; set; } // Quản lý trả lời đánh giá

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
