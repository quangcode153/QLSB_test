using System.Text.Json;
using SportChain.Shared.DTOs.Bookings;
using SportChain.Shared.DTOs.Common;
using SportChain.Shared.DTOs.Courts;
using SportChain.Shared.Enums;
using Xunit;

namespace SportChain.Tests.Frontend;

public class DtoConsistencyTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void ApiResponse_SerializationRoundTrip_PreservesAllProperties()
    {
        // Arrange
        var invoice = new PosInvoiceDto
        {
            BookingId = 123,
            BookingCode = "SC-20261001-9999",
            CustomerName = "Trần Khách Hàng",
            CustomerPhone = "0988111222",
            BranchName = "SportChain Cầu Giấy",
            CourtName = "Sân Cầu Lông 1",
            BookingDate = new DateOnly(2026, 10, 1),
            SlotLabels = new List<string> { "18:00 - 19:00", "19:00 - 20:00" },
            CourtRentAmount = 320000m,
            DepositPaid = 96000m,
            TotalServicesAmount = 30000m,
            RemainingAmountToPay = 254000m,
            Status = BookingStatus.InUse,
            Services = new List<BookingServiceItemSummaryDto>
            {
                new BookingServiceItemSummaryDto
                {
                    ServiceItemId = 1,
                    ServiceName = "Revive",
                    Quantity = 2,
                    UnitPrice = 15000m
                }
            }
        };

        var response = ApiResponse<PosInvoiceDto>.Ok(invoice, "Thành công");

        // Act
        string json = JsonSerializer.Serialize(response, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<ApiResponse<PosInvoiceDto>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.True(deserialized.Success);
        Assert.Equal("Thành công", deserialized.Message);
        Assert.NotNull(deserialized.Data);
        Assert.Equal(123, deserialized.Data.BookingId);
        Assert.Equal("SC-20261001-9999", deserialized.Data.BookingCode);
        Assert.Equal(254000m, deserialized.Data.RemainingAmountToPay);
        Assert.Single(deserialized.Data.Services);
        Assert.Equal(30000m, deserialized.Data.Services[0].SubTotal);
    }

    [Fact]
    public void BranchMatrixDto_SerializationRoundTrip_MaintainsSlotAndHeaderStructure()
    {
        // Arrange
        var matrix = new BranchMatrixDto
        {
            BranchId = 1,
            BranchName = "SportChain Cầu Giấy",
            Date = new DateOnly(2026, 10, 1),
            TimeHeaders = new List<TimeSlotDto>
            {
                new TimeSlotDto { Id = 1, DisplayLabel = "06:00 - 07:00", StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(7, 0, 0), IsPeakHour = false },
                new TimeSlotDto { Id = 12, DisplayLabel = "17:00 - 18:00", StartTime = new TimeSpan(17, 0, 0), EndTime = new TimeSpan(18, 0, 0), IsPeakHour = true }
            },
            Rows = new List<CourtMatrixRowDto>
            {
                new CourtMatrixRowDto
                {
                    CourtId = 1,
                    CourtName = "Sân 1",
                    SportType = SportType.Badminton,
                    SurfaceType = "Thảm PVC",
                    Slots = new List<SlotMatrixCellDto>
                    {
                        new SlotMatrixCellDto { TimeSlotId = 1, TimeLabel = "06:00 - 07:00", Status = SlotStatus.Available, Price = 100000m },
                        new SlotMatrixCellDto { TimeSlotId = 12, TimeLabel = "17:00 - 18:00", Status = SlotStatus.Holding, Price = 160000m }
                    }

                }
            }
        };

        // Act
        string json = JsonSerializer.Serialize(matrix, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<BranchMatrixDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(1, deserialized.BranchId);
        Assert.Equal(2, deserialized.TimeHeaders.Count);
        Assert.True(deserialized.TimeHeaders[1].IsPeakHour);
        Assert.Single(deserialized.Rows);
        Assert.Equal(SlotStatus.Holding, deserialized.Rows[0].Slots[1].Status);
    }
}
