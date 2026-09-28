using SportChain.Shared.Enums;
using SportChain.WebApi.Entities;

namespace SportChain.WebApi.Services;

public interface IBookingService
{
    Task<Booking> CreateHoldingBookingAsync(int customerId, int courtId, DateOnly date, List<int> slotIds);
    Task<bool> ConfirmDepositAsync(int bookingId, PaymentMethod paymentMethod);
    Task<bool> CheckInWithQrAsync(string checkInCode);
    Task<bool> CancelBookingAsync(int bookingId, string reason, int? cancelledByUserId);
    Task AutoReleaseExpiredHoldingsAsync();
    Task AutoMarkNoShowsAsync();
}
