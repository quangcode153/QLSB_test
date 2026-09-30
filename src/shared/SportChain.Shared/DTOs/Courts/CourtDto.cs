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
    public bool IsApproved { get; set; }
    public bool IsActive { get; set; }
    public int TotalCourts { get; set; }
}

public class TimeSlotDto
{
    public int Id { get; set; }
    public string DisplayLabel { get; set; } = string.Empty; // "18:00 - 19:00"
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal Price { get; set; }
    public bool IsPeakHour { get; set; }
    public SlotStatus Status { get; set; } = SlotStatus.Available;
}

public class SlotMatrixCellDto
{
    public int TimeSlotId { get; set; }
    public string TimeLabel { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsPeakHour { get; set; }
    public SlotStatus Status { get; set; } = SlotStatus.Available;
    public int? BookingId { get; set; }
    public string? BookingCode { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
}

public class CourtMatrixRowDto
{
    public int CourtId { get; set; }
    public string CourtName { get; set; } = string.Empty;
    public SportType SportType { get; set; }
    public string SurfaceType { get; set; } = string.Empty;
    public List<SlotMatrixCellDto> Slots { get; set; } = new();
}

public class BranchMatrixDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public List<TimeSlotDto> TimeHeaders { get; set; } = new();
    public List<CourtMatrixRowDto> Rows { get; set; } = new();
}
