using Microsoft.EntityFrameworkCore;
using SportChain.Shared.DTOs.Courts;
using SportChain.Shared.Enums;
using SportChain.WebApi.Data;
using SportChain.WebApi.Repositories;

namespace SportChain.WebApi.Services;

public class CourtMatrixService : ICourtMatrixService
{
    private readonly AppDbContext _context;
    private readonly ICourtRepository _courtRepo;

    public CourtMatrixService(AppDbContext context, ICourtRepository courtRepo)
    {
        _context = context;
        _courtRepo = courtRepo;
    }

    public async Task<BranchMatrixDto> GetBranchMatrixAsync(int branchId, DateOnly date)
    {
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == branchId);
        if (branch == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy chi nhánh với ID {branchId}");
        }

        var courts = await _courtRepo.GetCourtsByBranchAsync(branchId);
        var timeSlots = await _courtRepo.GetAllTimeSlotsAsync();
        var now = DateTime.UtcNow;

        // Lấy tất cả các đơn đặt của chi nhánh trong ngày này
        var activeBookings = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Details)
            .Where(b => b.BranchId == branchId && b.BookingDate == date)
            .Where(b => b.Status == BookingStatus.Confirmed || 
                        b.Status == BookingStatus.InUse || 
                        b.Status == BookingStatus.Completed ||
                        (b.Status == BookingStatus.PendingPayment && b.ExpireHoldingAt > now))
            .ToListAsync();

        // Lấy tất cả price rules
        var courtIds = courts.Select(c => c.Id).ToList();
        var priceRules = await _context.PriceRules
            .Where(p => courtIds.Contains(p.CourtId))
            .ToListAsync();

        var matrix = new BranchMatrixDto
        {
            BranchId = branch.Id,
            BranchName = branch.Name,
            Date = date,
            TimeHeaders = timeSlots.Select(t => new TimeSlotDto
            {
                Id = t.Id,
                DisplayLabel = t.DisplayLabel,
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                IsPeakHour = t.StartTime >= new TimeSpan(17, 0, 0) && t.StartTime < new TimeSpan(21, 0, 0)
            }).ToList()
        };

        foreach (var court in courts)
        {
            var row = new CourtMatrixRowDto
            {
                CourtId = court.Id,
                CourtName = court.Name,
                SportType = court.SportType,
                SurfaceType = court.SurfaceType
            };

            foreach (var slot in timeSlots)
            {
                // Kiểm tra xem slot này thuộc booking nào (ưu tiên đơn mới nhất)
                var bookingDetail = activeBookings
                    .Where(b => b.CourtId == court.Id)
                    .OrderByDescending(b => b.CreatedAt)
                    .SelectMany(b => b.Details, (b, d) => new { Booking = b, Detail = d })
                    .FirstOrDefault(x => x.Detail.TimeSlotId == slot.Id);

                // Tính giá: Ưu tiên price rule theo thứ cụ thể hơn rule chung
                var rule = priceRules
                    .Where(r => r.CourtId == court.Id && r.TimeSlotId == slot.Id && (r.DayOfWeek == null || r.DayOfWeek == date.DayOfWeek))
                    .OrderByDescending(r => r.DayOfWeek.HasValue)
                    .FirstOrDefault();
                bool isPeak = rule?.IsPeakHour ?? (slot.StartTime >= new TimeSpan(17, 0, 0) && slot.StartTime < new TimeSpan(21, 0, 0));
                decimal price = rule?.PricePerHour ?? SportChain.Shared.Constants.SystemPolicies.GetStandardPrice(court.SportType, isPeak);

                SlotStatus cellStatus = SlotStatus.Available;
                if (court.Status == SlotStatus.Maintenance)
                {
                    cellStatus = SlotStatus.Maintenance;
                }
                else if (bookingDetail != null)
                {
                    if (bookingDetail.Booking.Status == BookingStatus.PendingPayment)
                    {
                        cellStatus = SlotStatus.Holding;
                    }
                    else if (bookingDetail.Booking.Status == BookingStatus.Confirmed)
                    {
                        cellStatus = SlotStatus.Booked;
                    }
                    else if (bookingDetail.Booking.Status == BookingStatus.InUse)
                    {
                        cellStatus = SlotStatus.InUse;
                    }
                    else if (bookingDetail.Booking.Status == BookingStatus.Completed)
                    {
                        cellStatus = SlotStatus.Booked;
                    }
                }

                row.Slots.Add(new SlotMatrixCellDto
                {
                    TimeSlotId = slot.Id,
                    TimeLabel = slot.DisplayLabel,
                    Price = price,
                    IsPeakHour = isPeak,
                    Status = cellStatus,
                    BookingId = bookingDetail?.Booking.Id,
                    BookingCode = bookingDetail?.Booking.BookingCode,
                    CustomerId = bookingDetail?.Booking.CustomerId,
                    CustomerName = bookingDetail?.Booking.Customer?.FullName
                });
            }

            matrix.Rows.Add(row);
        }

        return matrix;
    }
}
