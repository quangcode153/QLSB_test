namespace SportChain.WebApi.Entities;

public class PriceRule
{
    public int Id { get; set; }

    public int CourtId { get; set; }
    public Court Court { get; set; } = null!;

    public int TimeSlotId { get; set; }
    public TimeSlot TimeSlot { get; set; } = null!;

    public DayOfWeek? DayOfWeek { get; set; } // Null: áp dụng tất cả các ngày, hoặc chỉ định T7, CN
    public bool IsPeakHour { get; set; } = false; // Ca cao điểm
    public decimal PricePerHour { get; set; }    // Giá tiền (VND)
}
