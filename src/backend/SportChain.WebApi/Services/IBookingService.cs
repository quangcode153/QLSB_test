using SportChain.Shared.Enums;
using SportChain.WebApi.Entities;

namespace SportChain.WebApi.Services;

public interface IBookingService
{
    Task<Booking> CreateHoldingBookingAsync(int customerId, int courtId, DateOnly date, List<int> slotIds);
    Task<bool> ConfirmDepositAsync(int bookingId, PaymentMethod paymentMethod);
    Task<(bool Success, string Message)> CheckInWithQrAsync(string checkInCode, int? staffBranchId = null, bool isSuperAdmin = false, int? staffUserId = null);
    Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, string reason, int? currentUserId = null, UserRole? userRole = null, int? staffBranchId = null);
    Task AutoReleaseExpiredHoldingsAsync();
    Task AutoMarkNoShowsAsync();
}
