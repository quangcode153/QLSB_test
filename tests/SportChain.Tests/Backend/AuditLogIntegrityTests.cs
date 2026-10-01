using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SportChain.Shared.Enums;
using SportChain.Tests.Helpers;
using SportChain.WebApi.Data;
using SportChain.WebApi.Entities;
using SportChain.WebApi.Hubs;
using SportChain.WebApi.Repositories;
using SportChain.WebApi.Services;
using Xunit;

namespace SportChain.Tests.Backend;

public class AuditLogIntegrityTests
{
    private readonly AppDbContext _context;
    private readonly BookingService _bookingService;

    public AuditLogIntegrityTests()
    {
        _context = TestDbHelper.CreateInMemoryDbContext();
        var bookingRepo = new BookingRepository(_context);
        var courtRepo = new CourtRepository(_context);

        var mockQr = new Mock<IQrCodeService>();
        mockQr.Setup(q => q.GenerateQrCodeBase64(It.IsAny<string>())).Returns("QR");

        var mockEmail = new Mock<IEmailService>();

        var mockHubContext = new Mock<IHubContext<CourtHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
        mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        var mockLogger = new Mock<ILogger<BookingService>>();

        _bookingService = new BookingService(
            bookingRepo,
            courtRepo,
            mockQr.Object,
            mockEmail.Object,
            mockHubContext.Object,
            _context,
            mockLogger.Object);
    }

    [Fact]
    public async Task CheckInWithQrAsync_GeneratesMandatoryQrCheckInAuditLog()
    {
        // Arrange: Tạo ca giờ bắt đầu trước hiện tại 5 phút
        var nowTime = DateTime.Now.TimeOfDay;
        var validSlot = new TimeSlot
        {
            StartTime = nowTime.Subtract(TimeSpan.FromMinutes(5)),
            EndTime = nowTime.Add(TimeSpan.FromMinutes(55))
        };
        _context.TimeSlots.Add(validSlot);
        await _context.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var booking = new Booking
        {
            BookingCode = "SC-AUDIT-CHECKIN",
            CheckInCode = "CHECKIN_AUDIT_1",
            CustomerId = 10,
            BranchId = 1,
            CourtId = 1,
            BookingDate = today,
            TotalAmount = 100000m,
            DepositAmount = 30000m,
            Status = BookingStatus.Confirmed,
            RowVersion = new byte[] { 1 },
            Details = new List<BookingDetail>
            {
                new BookingDetail { TimeSlotId = validSlot.Id, SlotPrice = 100000m }
            }
        };
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();


        // Act
        await _bookingService.CheckInWithQrAsync(
            booking.CheckInCode,
            staffBranchId: 1,
            isSuperAdmin: false,
            staffUserId: 20);

        // Assert (Rule BR-AUDIT-03 & Rule 06: Bắt buộc ghi nhận vết)
        var auditLog = await _context.AuditLogs
            .FirstOrDefaultAsync(a => a.EntityId == booking.Id && a.Action == "QrCheckIn");

        Assert.NotNull(auditLog);
        Assert.Equal(20, auditLog.UserId);
        Assert.Equal(nameof(Booking), auditLog.EntityName);
        Assert.Contains("thành công", auditLog.Details);
    }

    [Fact]
    public async Task CancelBookingAsync_CustomerCancel_GeneratesAuditLog()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
        var booking = new Booking
        {
            BookingCode = "SC-AUDIT-CANCEL",
            CustomerId = 10,
            BranchId = 1,
            CourtId = 1,
            BookingDate = futureDate,
            TotalAmount = 100000m,
            DepositAmount = 30000m,
            Status = BookingStatus.Confirmed,
            RowVersion = new byte[] { 1 },
            Details = new List<BookingDetail>
            {
                new BookingDetail { TimeSlotId = 2, SlotPrice = 100000m }
            }
        };
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Act: Khách hàng tự hủy
        await _bookingService.CancelBookingAsync(
            booking.Id,
            reason: "Bận việc đột xuất",
            currentUserId: 10,
            userRole: UserRole.Customer);

        // Assert
        var auditLog = await _context.AuditLogs
            .FirstOrDefaultAsync(a => a.EntityId == booking.Id && a.Action == "CustomerCancelBooking");

        Assert.NotNull(auditLog);
        Assert.Equal(10, auditLog.UserId);
        Assert.Contains("Bận việc đột xuất", auditLog.Details);
    }

    [Fact]
    public async Task CancelBookingAsync_AdminForceCancel_GeneratesAdminAuditLog()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var booking = new Booking
        {
            BookingCode = "SC-AUDIT-ADMIN-CANCEL",
            CustomerId = 10,
            BranchId = 1,
            CourtId = 1,
            BookingDate = futureDate,
            TotalAmount = 100000m,
            DepositAmount = 30000m,
            Status = BookingStatus.Confirmed,
            RowVersion = new byte[] { 1 },
            Details = new List<BookingDetail>
            {
                new BookingDetail { TimeSlotId = 2, SlotPrice = 100000m }
            }
        };
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Act: Admin userId 40 hủy
        await _bookingService.CancelBookingAsync(
            booking.Id,
            reason: "Bão to ngập sân",
            currentUserId: 40,
            userRole: UserRole.SuperAdmin);

        // Assert
        var auditLog = await _context.AuditLogs
            .FirstOrDefaultAsync(a => a.EntityId == booking.Id && a.Action == "AdminForceCancel");

        Assert.NotNull(auditLog);
        Assert.Equal(40, auditLog.UserId);
        Assert.Contains("Bão to ngập sân", auditLog.Details);
    }
}
