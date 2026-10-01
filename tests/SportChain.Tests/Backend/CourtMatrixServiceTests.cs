using SportChain.Shared.Enums;
using SportChain.Tests.Helpers;
using SportChain.WebApi.Data;
using SportChain.WebApi.Entities;
using SportChain.WebApi.Repositories;
using SportChain.WebApi.Services;
using Xunit;

namespace SportChain.Tests.Backend;

public class CourtMatrixServiceTests
{
    private readonly AppDbContext _context;
    private readonly CourtRepository _courtRepo;
    private readonly CourtMatrixService _matrixService;

    public CourtMatrixServiceTests()
    {
        _context = TestDbHelper.CreateInMemoryDbContext();
        _courtRepo = new CourtRepository(_context);
        _matrixService = new CourtMatrixService(_context, _courtRepo);
    }

    [Fact]
    public async Task GetBranchMatrixAsync_GeneratesExactly17StandardTimeSlots()
    {
        // Arrange
        int branchId = 1;
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var matrix = await _matrixService.GetBranchMatrixAsync(branchId, today);

        // Assert (Rule BR-BOOK-01: 17 ca chuẩn hóa từ 06:00 đến 23:00)
        Assert.NotNull(matrix);
        Assert.Equal(17, matrix.TimeHeaders.Count);
        Assert.Equal(new TimeSpan(6, 0, 0), matrix.TimeHeaders[0].StartTime);
        Assert.Equal(new TimeSpan(23, 0, 0), matrix.TimeHeaders[^1].EndTime);
    }

    [Fact]
    public async Task GetBranchMatrixAsync_PeakHoursIdentifiedCorrectly()
    {
        // Arrange
        int branchId = 1;
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var matrix = await _matrixService.GetBranchMatrixAsync(branchId, today);

        // Assert (Rule BR-PRICE-01: 17:00 -> 21:00 là giờ cao điểm)
        foreach (var header in matrix.TimeHeaders)
        {
            if (header.StartTime >= new TimeSpan(17, 0, 0) && header.StartTime < new TimeSpan(21, 0, 0))
            {
                Assert.True(header.IsPeakHour, $"Ca {header.DisplayLabel} phải là giờ cao điểm");
            }
            else
            {
                Assert.False(header.IsPeakHour, $"Ca {header.DisplayLabel} phải là giờ thường");
            }
        }
    }

    [Fact]
    public async Task GetBranchMatrixAsync_StateMachineCompleteness_CompletedBookingRemainsOccupiedNeverRevertsToAvailable()
    {
        // Arrange: Tạo đơn Completed lúc sáng
        var today = DateOnly.FromDateTime(DateTime.Today);
        var completedBooking = new Booking
        {
            BookingCode = "SC-COMPLETED-TEST",
            CustomerId = 10,
            BranchId = 1,
            CourtId = 1,
            BookingDate = today,
            TotalAmount = 100000m,
            DepositAmount = 100000m,
            Status = BookingStatus.Completed, // Trận đấu đã hoàn tất
            RowVersion = new byte[] { 1 },
            Details = new List<BookingDetail>
            {
                new BookingDetail { TimeSlotId = 2, SlotPrice = 100000m } // Ca 07:00 - 08:00
            }
        };
        _context.Bookings.Add(completedBooking);
        await _context.SaveChangesAsync();

        // Act
        var matrix = await _matrixService.GetBranchMatrixAsync(1, today);

        // Assert (Rule BR-ANTI-BUG 01: Ca Completed không bao giờ được phép revert về Available)
        var courtRow = matrix.Rows.First(r => r.CourtId == 1);
        var slot2 = courtRow.Slots.First(s => s.TimeSlotId == 2);

        Assert.NotEqual(SlotStatus.Available, slot2.Status);
        Assert.Equal(SlotStatus.Booked, slot2.Status);
    }

    [Fact]
    public async Task GetBranchMatrixAsync_ActiveHoldingBooking_RendersAsHolding()
    {
        // Arrange: Đơn PendingPayment còn hiệu lực trong 15 phút
        var today = DateOnly.FromDateTime(DateTime.Today);
        var holdingBooking = new Booking
        {
            BookingCode = "SC-HOLDING-TEST",
            CustomerId = 10,
            BranchId = 1,
            CourtId = 1,
            BookingDate = today,
            TotalAmount = 100000m,
            DepositAmount = 30000m,
            Status = BookingStatus.PendingPayment,
            ExpireHoldingAt = DateTime.UtcNow.AddMinutes(12), // Còn 12 phút
            RowVersion = new byte[] { 1 },
            Details = new List<BookingDetail>
            {
                new BookingDetail { TimeSlotId = 4, SlotPrice = 100000m }
            }
        };
        _context.Bookings.Add(holdingBooking);
        await _context.SaveChangesAsync();

        // Act
        var matrix = await _matrixService.GetBranchMatrixAsync(1, today);

        // Assert (Rule BR-BOOK-02: Đổi màu sang Holding / Vàng cam)
        var courtRow = matrix.Rows.First(r => r.CourtId == 1);
        var slot4 = courtRow.Slots.First(s => s.TimeSlotId == 4);

        Assert.Equal(SlotStatus.Holding, slot4.Status);
    }

    [Fact]
    public async Task GetBranchMatrixAsync_MaintenanceCourt_AllSlotsMarkedMaintenance()
    {
        // Arrange: Sân 3 đang bảo trì
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var matrix = await _matrixService.GetBranchMatrixAsync(1, today);

        // Assert: Mọi ca của sân 3 đều là Maintenance
        var courtRow = matrix.Rows.First(r => r.CourtId == 3);
        Assert.All(courtRow.Slots, slot => Assert.Equal(SlotStatus.Maintenance, slot.Status));
    }
}
