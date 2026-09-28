using SportChain.Shared.Enums;

namespace SportChain.Shared.DTOs.Courts;

public class CourtDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public SportType SportType { get; set; }
    public string SurfaceType { get; set; } = string.Empty;
    public SlotStatus Status { get; set; }
    public decimal DefaultPrice { get; set; }
}

public class BranchDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string OpenTimeLabel { get; set; } = "06:00";
    public string CloseTimeLabel { get; set; } = "23:00";
}

public class TimeSlotDto
{
    public int Id { get; set; }
    public string DisplayLabel { get; set; } = string.Empty; // "18:00 - 19:00"
    public decimal Price { get; set; }
    public bool IsPeakHour { get; set; }
    public SlotStatus Status { get; set; } = SlotStatus.Available;
}
