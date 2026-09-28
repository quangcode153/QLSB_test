using System.ComponentModel.DataAnnotations;
using SportChain.Shared.Enums;

namespace SportChain.WebApi.Entities;

public class Court
{
    public int Id { get; set; }

    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // Sân 1, Sân 2, Sân VIP

    public SportType SportType { get; set; } = SportType.Badminton;

    [MaxLength(50)]
    public string SurfaceType { get; set; } = "Trong nhà"; // Trong nhà / Ngoài trời / Cỏ nhân tạo

    public SlotStatus Status { get; set; } = SlotStatus.Available;

    public bool IsActive { get; set; } = true;

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!; // Concurrency Token chống trùng lịch

    public ICollection<PriceRule> PriceRules { get; set; } = new List<PriceRule>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<CourtIncident> Incidents { get; set; } = new List<CourtIncident>();
}
