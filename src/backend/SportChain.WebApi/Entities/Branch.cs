using System.ComponentModel.DataAnnotations;

namespace SportChain.WebApi.Entities;

public class Branch
{
    public int Id { get; set; }

    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    public TimeSpan OpenTime { get; set; } = new TimeSpan(6, 0, 0);   // 06:00
    public TimeSpan CloseTime { get; set; } = new TimeSpan(23, 0, 0); // 23:00

    public bool IsApproved { get; set; } = false; // Chờ Admin duyệt
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Court> Courts { get; set; } = new List<Court>();
    public ICollection<ServiceItem> ServiceItems { get; set; } = new List<ServiceItem>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
