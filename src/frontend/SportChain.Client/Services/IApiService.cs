using SportChain.Shared.DTOs.Admin;
using SportChain.Shared.DTOs.Auth;
using SportChain.Shared.DTOs.Bookings;
using SportChain.Shared.DTOs.Common;
using SportChain.Shared.DTOs.Courts;
using SportChain.Shared.Enums;

namespace SportChain.Client.Services;

public interface IApiService
{
    // Auth & Token Management
    string? Token { get; }
    AuthResponse? CurrentUser { get; }
    event Action? OnAuthStateChanged;
    void SetToken(string token, AuthResponse user);
    void Logout();

    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<AuthResponse>> GetCurrentUserAsync();

    // Branches & Courts
    Task<ApiResponse<List<BranchDto>>> GetBranchesAsync(string? city = null, bool onlyApproved = true);
    Task<ApiResponse<BranchDto>> GetBranchByIdAsync(int id);
    Task<ApiResponse<List<CourtDto>>> GetCourtsByBranchAsync(int branchId, SportType? sportType = null);
    Task<ApiResponse<BranchMatrixDto>> GetBranchMatrixAsync(int branchId, DateOnly? date = null);

    // Bookings
    Task<ApiResponse<BookingDto>> CreateHoldingBookingAsync(CreateBookingRequest request);
    Task<ApiResponse<BookingDto>> ConfirmDepositAsync(int bookingId, ConfirmDepositRequest request);
    Task<ApiResponse<bool>> CheckInQrAsync(CheckInRequest request);
    Task<ApiResponse<bool>> CancelBookingAsync(int bookingId, CancelBookingRequest request);
    Task<ApiResponse<BookingDto>> GetBookingByIdAsync(int bookingId);
    Task<ApiResponse<List<BookingDto>>> GetMyBookingsAsync();
    Task<ApiResponse<List<BookingDto>>> GetBranchBookingsAsync(int branchId, DateOnly? date = null);

    // POS
    Task<ApiResponse<PosInvoiceDto>> CreateWalkInBookingAsync(WalkInBookingRequest request);
    Task<ApiResponse<bool>> AddServiceItemAsync(AddServiceItemRequest request);
    Task<ApiResponse<PosInvoiceDto>> GetInvoiceAsync(int bookingId);
    Task<ApiResponse<bool>> CompletePaymentAsync(CompletePaymentRequest request);

    // Admin
    Task<ApiResponse<ChainStatsDto>> GetChainStatsAsync();
    Task<ApiResponse<List<AuditLogDto>>> GetAuditLogsAsync(int limit = 50);
    Task<ApiResponse<List<ComplaintDto>>> GetComplaintsAsync();
    Task<ApiResponse<List<UserDto>>> GetUsersAsync(string? search = null, UserRole? role = null);
    Task<ApiResponse<UserDto>> CreateStaffAsync(CreateStaffRequest request);
    Task<ApiResponse<bool>> ToggleUserStatusAsync(int id);
}
