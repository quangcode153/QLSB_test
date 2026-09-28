using System.ComponentModel.DataAnnotations;

namespace SportChain.WebApi.Entities;

public class ServiceItem
{
    public int Id { get; set; }

    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // Nước tăng lực, thuê vợt, bóng thi đấu

    [MaxLength(50)]
    public string Category { get; set; } = "Drink"; // Drink, Rental, Equipment, Clothing

    public decimal Price { get; set; }
    public bool IsRental { get; set; } = false; // Bán đứt hay Cho thuê
    public bool IsActive { get; set; } = true;
}
