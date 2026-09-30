using SportChain.WebApi.Entities;

namespace SportChain.WebApi.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int id);
    Task<Booking?> GetByBookingCodeAsync(string bookingCode);
    Task<Booking?> GetByCheckInCodeAsync(string checkInCode);
    Task<List<Booking>> GetExpiredHoldingsAsync(DateTime thresholdTime);
    Task<List<Booking>> GetNoShowBookingsAsync(DateOnly date, TimeSpan thresholdTime);
    Task<List<Booking>> GetBookingsByCustomerAsync(int customerId);
    Task<List<Booking>> GetBookingsByBranchAndDateAsync(int branchId, DateOnly date);
    Task<bool> IsSlotAlreadyBookedAsync(int courtId, DateOnly date, int timeSlotId);
    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task SaveChangesAsync();
}
