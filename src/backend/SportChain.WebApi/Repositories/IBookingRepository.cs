using SportChain.WebApi.Entities;

namespace SportChain.WebApi.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int id);
    Task<Booking?> GetByBookingCodeAsync(string bookingCode);
    Task<Booking?> GetByCheckInCodeAsync(string checkInCode);
    Task<List<Booking>> GetExpiredHoldingsAsync(DateTime thresholdTime);
    Task<List<Booking>> GetNoShowBookingsAsync(DateOnly date, TimeSpan thresholdTime);
    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task SaveChangesAsync();
}
