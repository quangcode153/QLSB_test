using System.ComponentModel.DataAnnotations;

namespace SportChain.WebApi.Entities;

public class CourtIncident
{
    public int Id { get; set; }

    public int CourtId { get; set; }
    public Court Court { get; set; } = null!;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty; // Hỏng đèn cột 2, rách lưới, mặt sân ẩm ướt

    [MaxLength(50)]
    public string Severity { get; set; } = "Trung bình"; // Nhẹ, Trung bình, Nghiêm trọng

    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public bool IsResolved => ResolvedAt.HasValue;
}
