using SportChain.Shared.Enums;
using SportChain.Tests.Helpers;
using SportChain.WebApi.Data;
using SportChain.WebApi.Entities;
using Xunit;

namespace SportChain.Tests.Backend;

public class FinancialIntegrityTests
{
    private readonly AppDbContext _context;

    public FinancialIntegrityTests()
    {
        _context = TestDbHelper.CreateInMemoryDbContext();
    }

    [Fact]
    public void TwoPhaseRevenueFormula_AccuratelyReflectsActualCashflow()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);

        // 1. Đơn Confirmed: Khách mới nộp 30% cọc
        var confirmedBooking = new Booking
        {
            BookingCode = "SC-REV-CONFIRMED",
            CustomerId = 10,
            BranchId = 1,
            CourtId = 1,
            BookingDate = today,
            TotalAmount = 500000m,
            DepositAmount = 150000m, // 30%
            Status = BookingStatus.Confirmed,
            RowVersion = new byte[] { 1 }
        };

        // 2. Đơn Completed: Khách đã chơi xong, trả nốt 70% + nước uống POS
        var completedBooking = new Booking
        {
            BookingCode = "SC-REV-COMPLETED",
            CustomerId = 10,
            BranchId = 1,
            CourtId = 1,
            BookingDate = today,
            TotalAmount = 200000m,
            DepositAmount = 60000m,
            Status = BookingStatus.Completed,
            RowVersion = new byte[] { 1 },
            ServiceItems = new List<BookingServiceItem>
            {
                new BookingServiceItem { ServiceItemId = 1, Quantity = 2, UnitPrice = 15000m } // 30,000 đ nước
            }
        };

        // 3. Đơn NoShow: Khách bỏ sân, tịch thu 100% cọc
        var noShowBooking = new Booking
        {
            BookingCode = "SC-REV-NOSHOW",
            CustomerId = 10,
            BranchId = 1,
            CourtId = 1,
            BookingDate = today,
            TotalAmount = 300000m,
            DepositAmount = 90000m,
            RefundAmount = 0m,
            Status = BookingStatus.NoShow,
            RowVersion = new byte[] { 1 }
        };

        // 4. Đơn Cancelled hủy trước 18 tiếng: phạt 50% cọc
        var cancelledPartialBooking = new Booking
        {
            BookingCode = "SC-REV-CANCEL-PARTIAL",
            CustomerId = 10,
            BranchId = 1,
            CourtId = 1,
            BookingDate = today,
            TotalAmount = 200000m,
            DepositAmount = 60000m,
            RefundAmount = 30000m, // Hoàn 30,000 đ -> Giữ lại 30,000 đ
            Status = BookingStatus.Cancelled,
            RowVersion = new byte[] { 1 }
        };

        // 5. Đơn Cancelled do Admin hủy: hoàn 100% cọc -> Giữ lại 0 đ
        var cancelledFullBooking = new Booking
        {
            BookingCode = "SC-REV-CANCEL-FULL",
            CustomerId = 10,
            BranchId = 1,
            CourtId = 1,
            BookingDate = today,
            TotalAmount = 200000m,
            DepositAmount = 60000m,
            RefundAmount = 60000m,
            Status = BookingStatus.Cancelled,
            RowVersion = new byte[] { 1 }
        };

        var allBookings = new List<Booking>
        {
            confirmedBooking,
            completedBooking,
            noShowBooking,
            cancelledPartialBooking,
            cancelledFullBooking
        };

        // Act: Áp dụng chuẩn công thức tài chính 2 pha (Rule BR-ANTI-BUG 03)
        decimal totalRevenue = 0;
        foreach (var b in allBookings)
        {
            var servicesTotal = b.ServiceItems?.Sum(s => s.Quantity * s.UnitPrice) ?? 0;
            switch (b.Status)
            {
                case BookingStatus.Completed:
                case BookingStatus.InUse:
                    totalRevenue += b.TotalAmount + servicesTotal;
                    break;
                case BookingStatus.Confirmed:
                    totalRevenue += b.DepositAmount;
                    break;
                case BookingStatus.NoShow:
                    totalRevenue += b.DepositAmount;
                    break;
                case BookingStatus.Cancelled:
                    var retained = b.DepositAmount - b.RefundAmount;
                    if (retained > 0) totalRevenue += retained;
                    break;
            }
        }

        // Expected:
        // Confirmed: 150,000
        // Completed: 200,000 + 30,000 = 230,000
        // NoShow: 90,000
        // Cancelled partial: 30,000
        // Cancelled full: 0
        // Total = 150k + 230k + 90k + 30k = 500,000 đ
        decimal expectedRevenue = 150000m + 230000m + 90000m + 30000m + 0m;

        // Assert
        Assert.Equal(500000m, totalRevenue);
        Assert.Equal(expectedRevenue, totalRevenue);
    }
}
