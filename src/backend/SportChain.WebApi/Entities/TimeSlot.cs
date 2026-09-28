using System.ComponentModel.DataAnnotations;

namespace SportChain.WebApi.Entities;

public class TimeSlot
{
    public int Id { get; set; }

    public TimeSpan StartTime { get; set; } // Ví dụ: 06:00
    public TimeSpan EndTime { get; set; }   // Ví dụ: 07:00

    [MaxLength(20)]
    public string DisplayLabel => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";

    public ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
}
