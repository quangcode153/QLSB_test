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

public class BookingServiceTests
{
    private readonly AppDbContext _context;
    private readonly BookingRepository _bookingRepo;
    private readonly CourtRepository _courtRepo;
    private readonly Mock<IQrCodeService> _mockQrCodeService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IHubContext<CourtHub>> _mockHubContext;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<ILogger<BookingService>> _mockLogger;
    private readonly BookingService _bookingService;

    public BookingServiceTests()
    {
        _context = TestDbHelper.CreateInMemoryDbContext();
        _bookingRepo = new BookingRepository(_context);
        _courtRepo = new CourtRepository(_context);

        _mockQrCodeService = new Mock<IQrCodeService>();
        _mockQrCodeService.Setup(q => q.GenerateQrCodeBase64(It.IsAny<string>())).Returns("MOCK_BASE64_QR");

        _mockEmailService = new Mock<IEmailService>();

        _mockHubContext = new Mock<IHubContext<CourtHub>>();
        var mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        mockClients.Setup(c => c.All).Returns(_mockClientProxy.Object);
        _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        _mockLogger = new Mock<ILogger<BookingService>>();

        _bookingService = new BookingService(
            _bookingRepo,
            _courtRepo,
            _mockQrCodeService.Object,
            _mockEmailService.Object,
            _mockHubContext.Object,
            _context,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateHoldingBookingAsync_ValidRequest_CreatesBookingWithHoldingStatusAnd15MinExpiry()
    {
        // Arrange
        int customerId = 10;
        int courtId = 1;
        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var slotIds = new List<int> { 12 }; // 17:00 - 18:00 (Peak hour)

        // Act
        var booking = await _bookingService.CreateHoldingBookingAsync(customerId, courtId, tomorrow, slotIds);

        // Assert
        Assert.NotNull(booking);
        Assert.Equal(BookingStatus.PendingPayment, booking.Status);
        Assert.True(booking.ExpireHoldingAt > DateTime.UtcNow.AddMinutes(14));
        Assert.True(booking.ExpireHoldingAt <= DateTime.UtcNow.AddMinutes(16));
        Assert.Equal(160000m, booking.TotalAmount); // Peak hour for Badminton
        Assert.Equal(48000m, booking.DepositAmount); // 30% of 160,000
        Assert.Equal(112000m, booking.RemainingAmount); // 70% remaining
    }

    [Fact]
    public async Task CreateHoldingBookingAsync_PastDate_ThrowsInvalidOperationException()
    {
        // Arrange: Ngày hôm qua
        int customerId = 10;
        int courtId = 1;
        var yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var slotIds = new List<int> { 1 };

        // Act & Assert (Rule BR-ANTI-BUG: Chặn ngày quá khứ)
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _bookingService.CreateHoldingBookingAsync(customerId, courtId, yesterday, slotIds));
        Assert.Contains("quá khứ", ex.Message);
    }

    [Fact]
    public async Task CreateHoldingBookingAsync_MaintenanceCourt_ThrowsInvalidOperationException()
    {
        // Arrange: Sân 3 đang bảo trì
        int customerId = 10;
        int courtId = 3;
        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var slotIds = new List<int> { 1 };

        // Act & Assert (Rule BR-ANTI-BUG: Chặn sân bảo trì)
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _bookingService.CreateHoldingBookingAsync(customerId, courtId, tomorrow, slotIds));
        Assert.Contains("bảo trì", ex.Message);
    }

    [Fact]
    public async Task CreateHoldingBookingAsync_AntiHoarding_CustomerHasActivePending_ThrowsInvalidOperationException()
    {
        // Arrange: Khách đã có 1 đơn PendingPayment còn hiệu lực
        int customerId = 10;
        var existingBooking = new Booking
        {
            BookingCode = "SC-TEST-PENDING",
            CustomerId = customerId,
            BranchId = 1,
            CourtId = 1,
            BookingDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            TotalAmount = 100000m,
            DepositAmount = 30000m,
            Status = BookingStatus.PendingPayment,
            ExpireHoldingAt = DateTime.UtcNow.AddMinutes(10),
            RowVersion = new byte[] { 1 }
        };
        _context.Bookings.Add(existingBooking);
        await _context.SaveChangesAsync();

        // Act & Assert: Thử tạo thêm đơn thứ 2
        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _bookingService.CreateHoldingBookingAsync(customerId, 1, tomorrow, new List<int> { 5 }));
        Assert.Contains("tối đa 1 đơn giữ chỗ", ex.Message);
    }

    [Fact]
    public async Task CreateHoldingBookingAsync_SlotAlreadyBooked_ThrowsInvalidOperationException()
    {
        // Arrange: Ca 10 đã được người khác Confirmed
        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var existingBooking = new Booking
        {
            BookingCode = "SC-TEST-EXISTING",
            CustomerId = 11,
            BranchId = 1,
            CourtId = 1,
            BookingDate = tomorrow,
            TotalAmount = 100000m,
            DepositAmount = 30000m,
            Status = BookingStatus.Confirmed,
            RowVersion = new byte[] { 1 },
            Details = new List<BookingDetail>
            {
                new BookingDetail { TimeSlotId = 10, SlotPrice = 100000m }
            }
        };
        _context.Bookings.Add(existingBooking);
        await _context.SaveChangesAsync();

        // Act & Assert (Quy tắc chống Double-Booking)
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _bookingService.CreateHoldingBookingAsync(10, 1, tomorrow, new List<int> { 10 }));
        Assert.Contains("vừa được một khách hàng khác thao tác giữ", ex.Message);
    }


    [Fact]
    public async Task ConfirmDepositAsync_ValidBooking_SetsConfirmedAndGeneratesTicket()
    {
        // Arrange
        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var booking = new Booking
        {
            BookingCode = "SC-CONFIRM-TEST",
            CustomerId = 10,
            BranchId = 1,
            CourtId = 1,
            BookingDate = tomorrow,
            TotalAmount = 100000m,
            DepositAmount = 30000m,
            Status = BookingStatus.PendingPayment,
            ExpireHoldingAt = DateTime.UtcNow.AddMinutes(10),
            RowVersion = new byte[] { 1 },
            Details = new List<BookingDetail>
            {
                new BookingDetail { TimeSlotId = 2, SlotPrice = 100000m }
            }
        };
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Act
        var success = await _bookingService.ConfirmDepositAsync(booking.Id, PaymentMethod.VietQr);

        // Assert
        Assert.True(success);
        var updated = await _context.Bookings.FindAsync(booking.Id);
        Assert.NotNull(updated);
        Assert.Equal(BookingStatus.Confirmed, updated.Status);
        Assert.Equal(PaymentMethod.VietQr, updated.PaymentMethod);
        _mockQrCodeService.Verify(q => q.GenerateQrCodeBase64(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CancelBookingAsync_CustomerCancelsMoreThan24HoursAhead_Full100PercentRefund()
    {
        // Arrange: Hủy trước 3 ngày (> 24 tiếng)
        var targetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3));
        var booking = new Booking
        {
            BookingCode = "SC-CANCEL-24H",
            CustomerId = 10,
            BranchId = 1,
            CourtId = 1,
            BookingDate = targetDate,
            TotalAmount = 200000m,
            DepositAmount = 60000m,
            Status = BookingStatus.Confirmed,
            RowVersion = new byte[] { 1 },
            Details = new List<BookingDetail>
            {
                new BookingDetail { TimeSlotId = 7, SlotPrice = 200000m }
            }
        };
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Act
        var (success, _) = await _bookingService.CancelBookingAsync(
            booking.Id,
            reason: "Hủy trước 3 ngày",
            currentUserId: 10,
            userRole: UserRole.Customer);

        // Assert (Rule BR-PRICE-02: Hoàn 100% cọc)
        Assert.True(success);
        var updated = await _context.Bookings.FindAsync(booking.Id);
        Assert.Equal(BookingStatus.Cancelled, updated!.Status);
        Assert.Equal(booking.DepositAmount, updated.RefundAmount);
    }

    [Fact]
    public async Task CancelBookingAsync_AdminForceCancel_ReturnsFull100PercentRefundRegardlessOfTime()
    {
        // Arrange: Đơn ngày mai, Admin hủy do bảo trì đột xuất
        var targetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var booking = new Booking
        {
            BookingCode = "SC-ADMIN-CANCEL",
            CustomerId = 10,
            BranchId = 1,
            CourtId = 1,
            BookingDate = targetDate,
            TotalAmount = 100000m,
            DepositAmount = 30000m,
            Status = BookingStatus.Confirmed,
            RowVersion = new byte[] { 1 },
            Details = new List<BookingDetail>
            {
                new BookingDetail { TimeSlotId = 3, SlotPrice = 100000m }
            }
        };
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Act: Admin userId = 40 (SuperAdmin) hủy
        var (success, _) = await _bookingService.CancelBookingAsync(
            booking.Id,
            reason: "Sự cố chập điện hệ thống đèn chiếu sáng",
            currentUserId: 40,
            userRole: UserRole.SuperAdmin);

        // Assert (Rule BR-PRICE-03: Hoàn đủ 100% cọc)
        Assert.True(success);
        var updated = await _context.Bookings.FindAsync(booking.Id);
        Assert.Equal(BookingStatus.Cancelled, updated!.Status);
        Assert.Equal(booking.DepositAmount, updated.RefundAmount);
    }

    [Fact]
    public async Task CheckInWithQrAsync_Within15MinWindow_SucceedsAndSetsInUse()
    {
        // Arrange: Tạo ca giờ bắt đầu trước thời điểm hiện tại 5 phút (nằm trong [-15p, +15p])
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
            BookingCode = "SC-CHECKIN-VALID",
            CheckInCode = "CHECKIN_SECRET_123",
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

        // Act: Lễ tân quét mã check-in
        var (success, _) = await _bookingService.CheckInWithQrAsync(
            booking.CheckInCode,
            staffBranchId: 1,
            isSuperAdmin: false,
            staffUserId: 20);

        // Assert
        Assert.True(success);
        var updated = await _context.Bookings.FindAsync(booking.Id);
        Assert.Equal(BookingStatus.InUse, updated!.Status);
        Assert.NotNull(updated.CheckedInAt);
    }


    [Fact]
    public async Task CheckInWithQrAsync_FutureDay_ReturnsFalse()
    {
        // Arrange: Đơn đặt cho ngày mai
        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var booking = new Booking
        {
            BookingCode = "SC-CHECKIN-TOMORROW",
            CheckInCode = "CHECKIN_TOMORROW_SECRET",
            CustomerId = 10,
            BranchId = 1,
            CourtId = 1,
            BookingDate = tomorrow,
            TotalAmount = 100000m,
            DepositAmount = 30000m,
            Status = BookingStatus.Confirmed,
            RowVersion = new byte[] { 1 },
            Details = new List<BookingDetail>
            {
                new BookingDetail { TimeSlotId = 5, SlotPrice = 100000m }
            }
        };
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Act
        var (success, msg) = await _bookingService.CheckInWithQrAsync(
            booking.CheckInCode,
            staffBranchId: 1,
            isSuperAdmin: false,
            staffUserId: 20);

        // Assert (Rule BR-POS-01: Chưa đến ngày chơi)
        Assert.False(success);
        Assert.Contains("Chưa đến ngày nhận sân", msg);
    }

    [Fact]
    public async Task AutoReleaseExpiredHoldingsAsync_CancelsExpiredBookings()
    {
        // Arrange: Đơn PendingPayment đã hết hạn 15 phút từ 10 phút trước
        var expiredBooking = new Booking
        {
            BookingCode = "SC-EXPIRED-HOLDING",
            CustomerId = 10,
            BranchId = 1,
            CourtId = 1,
            BookingDate = DateOnly.FromDateTime(DateTime.Today),
            TotalAmount = 100000m,
            DepositAmount = 30000m,
            Status = BookingStatus.PendingPayment,
            ExpireHoldingAt = DateTime.UtcNow.AddMinutes(-10), // Hết hạn rồi
            RowVersion = new byte[] { 1 },
            Details = new List<BookingDetail>
            {
                new BookingDetail { TimeSlotId = 3, SlotPrice = 100000m }
            }
        };
        _context.Bookings.Add(expiredBooking);
        await _context.SaveChangesAsync();

        // Act
        await _bookingService.AutoReleaseExpiredHoldingsAsync();

        // Assert (Rule BR-BOOK-04: Tự động thu hồi slot hết hạn giữ cọc)
        var updated = await _context.Bookings.FindAsync(expiredBooking.Id);
        Assert.Equal(BookingStatus.Cancelled, updated!.Status);
    }
}
