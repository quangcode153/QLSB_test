using System.ComponentModel.DataAnnotations;

namespace SportChain.WebApi.Entities;

public class FacilityEquipment
{
    public int Id { get; set; }

    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public int? CourtId { get; set; }
    public Court? Court { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // Đèn cao áp, Lưới thi đấu, Bảng điểm điện tử

    [MaxLength(50)]
    public string Condition { get; set; } = "Tốt"; // Tốt / Xuống cấp / Cần sửa chữa

    public DateTime LastInspectedAt { get; set; } = DateTime.UtcNow;
}
