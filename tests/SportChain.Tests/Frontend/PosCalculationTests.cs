using SportChain.Shared.DTOs.Bookings;
using Xunit;

namespace SportChain.Tests.Frontend;

public class PosCalculationTests
{
    [Fact]
    public void ServiceItemSummaryDto_SubTotal_CalculatesQuantityTimesUnitPrice()
    {
        // Arrange
        var item1 = new BookingServiceItemSummaryDto
        {
            ServiceItemId = 1,
            ServiceName = "Nước Revive Chanh Muối",
            Quantity = 3,
            UnitPrice = 15000m
        };

        var item2 = new BookingServiceItemSummaryDto
        {
            ServiceItemId = 2,
            ServiceName = "Thuê Vợt Yonex Carbon",
            Quantity = 2,
            UnitPrice = 40000m
        };

        // Assert
        Assert.Equal(45000m, item1.SubTotal);
        Assert.Equal(80000m, item2.SubTotal);
    }

    [Fact]
    public void PosInvoiceCalculation_PreBookedWith30PercentDeposit_ComputesRemainingBalanceAccurately()
    {
        // Arrange: Sân 300,000 đ, đã cọc 30% = 90,000 đ
        decimal courtRent = 300000m;
        decimal depositPaid = 90000m;

        var services = new List<BookingServiceItemSummaryDto>
        {
            new BookingServiceItemSummaryDto { ServiceItemId = 1, ServiceName = "Revive", Quantity = 4, UnitPrice = 15000m }, // 60,000
            new BookingServiceItemSummaryDto { ServiceItemId = 2, ServiceName = "Vợt", Quantity = 1, UnitPrice = 40000m }    // 40,000
        };

        // Act
        decimal totalServices = services.Sum(s => s.SubTotal); // 100,000 đ
        decimal remainingRent = Math.Max(0, courtRent - depositPaid); // 210,000 đ
        decimal remainingToPay = remainingRent + totalServices; // 310,000 đ

        var invoice = new PosInvoiceDto
        {
            BookingCode = "SC-TEST-POS",
            CourtRentAmount = courtRent,
            DepositPaid = depositPaid,
            Services = services,
            TotalServicesAmount = totalServices,
            RemainingAmountToPay = remainingToPay
        };

        // Assert
        Assert.Equal(100000m, invoice.TotalServicesAmount);
        Assert.Equal(310000m, invoice.RemainingAmountToPay);
    }

    [Fact]
    public void PosInvoiceCalculation_WalkInCustomer_Requires100PercentPaymentAtCounter()
    {
        // Arrange: Khách vãng lai thu tiền trực tiếp 100% tại quầy
        decimal courtRent = 200000m;
        decimal depositPaid = 200000m; // Thu 100% lúc nhận sân

        var services = new List<BookingServiceItemSummaryDto>
        {
            new BookingServiceItemSummaryDto { ServiceItemId = 1, ServiceName = "Nước suối Aquafina", Quantity = 2, UnitPrice = 10000m } // 20,000
        };

        // Act
        decimal totalServices = services.Sum(s => s.SubTotal); // 20,000
        decimal remainingRent = Math.Max(0, courtRent - depositPaid); // 0
        decimal remainingToPay = remainingRent + totalServices; // 20,000 (chỉ thu thêm tiền nước)

        // Assert
        Assert.Equal(0m, remainingRent);
        Assert.Equal(20000m, remainingToPay);
    }
}
