using Microsoft.EntityFrameworkCore;
using SportChain.Shared.Enums;
using SportChain.WebApi.Data;
using SportChain.WebApi.Entities;

namespace SportChain.WebApi.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(int id)
    {
        return await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Branch)
            .Include(b => b.Court)
            .Include(b => b.Details)
                .ThenInclude(d => d.TimeSlot)
            .Include(b => b.ServiceItems)
                .ThenInclude(s => s.ServiceItem)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Booking?> GetByBookingCodeAsync(string bookingCode)
    {
        return await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Branch)
            .Include(b => b.Court)
            .Include(b => b.Details)
                .ThenInclude(d => d.TimeSlot)
            .FirstOrDefaultAsync(b => b.BookingCode == bookingCode);
    }

    public async Task<Booking?> GetByCheckInCodeAsync(string checkInCode)
    {
        return await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Branch)
            .Include(b => b.Court)
            .Include(b => b.Details)
                .ThenInclude(d => d.TimeSlot)
            .FirstOrDefaultAsync(b => b.CheckInCode == checkInCode);
    }

    public async Task<List<Booking>> GetExpiredHoldingsAsync(DateTime thresholdTime)
    {
        return await _context.Bookings
            .Include(b => b.Details)
            .Include(b => b.Court)
            .Where(b => b.Status == BookingStatus.PendingPayment && b.ExpireHoldingAt <= thresholdTime)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetNoShowBookingsAsync(DateOnly date, TimeSpan thresholdTime)
    {
        return await _context.Bookings
            .Include(b => b.Details)
                .ThenInclude(d => d.TimeSlot)
            .Include(b => b.Court)
            .Where(b => (b.BookingDate < date || (b.BookingDate == date && b.Details.Any(d => d.TimeSlot.StartTime <= thresholdTime)))
                     && b.Status == BookingStatus.Confirmed)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetBookingsByCustomerAsync(int customerId)
    {
        return await _context.Bookings
            .Include(b => b.Branch)
            .Include(b => b.Court)
            .Include(b => b.Details)
                .ThenInclude(d => d.TimeSlot)
            .Include(b => b.ServiceItems)
                .ThenInclude(s => s.ServiceItem)
            .Where(b => b.CustomerId == customerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetBookingsByBranchAndDateAsync(int branchId, DateOnly date)
    {
        return await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Court)
            .Include(b => b.Details)
                .ThenInclude(d => d.TimeSlot)
            .Include(b => b.ServiceItems)
                .ThenInclude(s => s.ServiceItem)
            .Where(b => b.BranchId == branchId && b.BookingDate == date)
            .OrderBy(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> IsSlotAlreadyBookedAsync(int courtId, DateOnly date, int timeSlotId)
    {
        var now = DateTime.UtcNow;

        return await _context.BookingDetails
            .Include(d => d.Booking)
            .AnyAsync(d => d.Booking.CourtId == courtId 
                        && d.Booking.BookingDate == date 
                        && d.TimeSlotId == timeSlotId
                        && (
                            d.Booking.Status == BookingStatus.Confirmed ||
                            d.Booking.Status == BookingStatus.InUse ||
                            d.Booking.Status == BookingStatus.Completed ||
                            (d.Booking.Status == BookingStatus.PendingPayment && d.Booking.ExpireHoldingAt > now)
                        ));
    }

    public async Task AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
    }

    public Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
