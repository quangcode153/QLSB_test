using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SportChain.Shared.DTOs.Admin;
using SportChain.Shared.DTOs.Auth;
using SportChain.Shared.DTOs.Bookings;
using SportChain.Shared.DTOs.Common;
using SportChain.Shared.DTOs.Courts;
using SportChain.Shared.Enums;

namespace SportChain.WinForms.Services;

public class WinFormsApiClient
{
    private static WinFormsApiClient? _instance;
    public static WinFormsApiClient Instance => _instance ??= new WinFormsApiClient();

    private readonly HttpClient _http;
    private string BaseUrl => Common.AppConfig.BaseApiUrl;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public WinFormsApiClient()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
    }

    private async Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string path, object? body = null)
    {
        try
        {
            var url = path.StartsWith("http") ? path : $"{BaseUrl}{path}";
            using var req = new HttpRequestMessage(method, url);

            if (!string.IsNullOrEmpty(SessionContext.Token))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SessionContext.Token);
            }

            if (body != null)
            {
                req.Content = JsonContent.Create(body);
            }

            using var res = await _http.SendAsync(req);
            var content = await res.Content.ReadAsStringAsync();

            if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                SessionContext.ClearSession();
                return ApiResponse<T>.Fail("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                    if (result != null) return result;
                }
                catch
                {
                    // Trường hợp server trả text thuần
                }
            }

            if (!res.IsSuccessStatusCode)
            {
                return ApiResponse<T>.Fail($"Máy chủ phản hồi lỗi (HTTP {(int)res.StatusCode}): {content}");
            }

            return ApiResponse<T>.Fail("Không có dữ liệu phản hồi từ máy chủ.");
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.Fail($"Lỗi kết nối máy chủ ({BaseUrl}): {ex.Message}");
        }
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var result = await SendAsync<AuthResponse>(HttpMethod.Post, "/api/Auth/login", request);
        if (result.Success && result.Data != null)
        {
            SessionContext.SetSession(result.Data.Token, result.Data);
        }
        return result;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var result = await SendAsync<AuthResponse>(HttpMethod.Post, "/api/Auth/register", request);
        if (result.Success && result.Data != null)
        {
            SessionContext.SetSession(result.Data.Token, result.Data);
        }
        return result;
    }

    public Task<ApiResponse<List<BranchDto>>> GetBranchesAsync(string? city = null, bool onlyApproved = true)
    {
        var url = $"/api/Branches?onlyApproved={onlyApproved}";
        if (!string.IsNullOrEmpty(city)) url += $"&city={Uri.EscapeDataString(city)}";
        return SendAsync<List<BranchDto>>(HttpMethod.Get, url);
    }

    public Task<ApiResponse<BranchDto>> GetBranchByIdAsync(int id)
    {
        return SendAsync<BranchDto>(HttpMethod.Get, $"/api/Branches/{id}");
    }

    public Task<ApiResponse<List<CourtDto>>> GetCourtsByBranchAsync(int branchId, SportType? sportType = null)
    {
        var url = $"/api/Courts/branch/{branchId}";
        if (sportType.HasValue) url += $"?sportType={sportType.Value}";
        return SendAsync<List<CourtDto>>(HttpMethod.Get, url);
    }

    public Task<ApiResponse<CourtDto>> CreateCourtAsync(CourtDto dto)
    {
        return SendAsync<CourtDto>(HttpMethod.Post, "/api/Courts", dto);
    }

    public Task<ApiResponse<BranchMatrixDto>> GetBranchMatrixAsync(int branchId, DateOnly? date = null)
    {
        var url = $"/api/Courts/branch/{branchId}/matrix";
        if (date.HasValue) url += $"?date={date.Value:yyyy-MM-dd}";
        return SendAsync<BranchMatrixDto>(HttpMethod.Get, url);
    }

    public Task<ApiResponse<BookingDto>> CreateHoldingBookingAsync(CreateBookingRequest request)
    {
        return SendAsync<BookingDto>(HttpMethod.Post, "/api/Bookings/hold", request);
    }

    public Task<ApiResponse<BookingDto>> ConfirmDepositAsync(int bookingId, ConfirmDepositRequest request)
    {
        return SendAsync<BookingDto>(HttpMethod.Post, $"/api/Bookings/{bookingId}/confirm-deposit", request);
    }

    public Task<ApiResponse<bool>> CheckInQrAsync(CheckInRequest request)
    {
        return SendAsync<bool>(HttpMethod.Post, "/api/Bookings/check-in", request);
    }

    public Task<ApiResponse<bool>> CancelBookingAsync(int bookingId, CancelBookingRequest request)
    {
        return SendAsync<bool>(HttpMethod.Post, $"/api/Bookings/{bookingId}/cancel", request);
    }

    public Task<ApiResponse<BookingDto>> GetBookingByIdAsync(int bookingId)
    {
        return SendAsync<BookingDto>(HttpMethod.Get, $"/api/Bookings/{bookingId}");
    }

    public Task<ApiResponse<List<BookingDto>>> GetMyBookingsAsync()
    {
        return SendAsync<List<BookingDto>>(HttpMethod.Get, "/api/Bookings/my-bookings");
    }

    public Task<ApiResponse<List<BookingDto>>> GetBranchBookingsAsync(int branchId, DateOnly? date = null)
    {
        var url = $"/api/Bookings/branch/{branchId}";
        if (date.HasValue) url += $"?date={date.Value:yyyy-MM-dd}";
        return SendAsync<List<BookingDto>>(HttpMethod.Get, url);
    }

    public Task<ApiResponse<PosInvoiceDto>> CreateWalkInBookingAsync(WalkInBookingRequest request)
    {
        return SendAsync<PosInvoiceDto>(HttpMethod.Post, "/api/Pos/walk-in", request);
    }

    public Task<ApiResponse<bool>> AddServiceItemAsync(AddServiceItemRequest request)
    {
        return SendAsync<bool>(HttpMethod.Post, "/api/Pos/add-service", request);
    }

    public Task<ApiResponse<PosInvoiceDto>> GetInvoiceAsync(int bookingId)
    {
        return SendAsync<PosInvoiceDto>(HttpMethod.Get, $"/api/Pos/invoice/{bookingId}");
    }

    public Task<ApiResponse<bool>> CompletePaymentAsync(CompletePaymentRequest request)
    {
        return SendAsync<bool>(HttpMethod.Post, "/api/Pos/complete-payment", request);
    }

    public Task<ApiResponse<ChainStatsDto>> GetChainStatsAsync()
    {
        return SendAsync<ChainStatsDto>(HttpMethod.Get, "/api/Admin/stats");
    }

    public Task<ApiResponse<List<AuditLogDto>>> GetAuditLogsAsync(int limit = 50)
    {
        return SendAsync<List<AuditLogDto>>(HttpMethod.Get, $"/api/Admin/audit-logs?limit={limit}");
    }

    public Task<ApiResponse<List<UserDto>>> GetUsersAsync(string? search = null, UserRole? role = null)
    {
        var url = "/api/Admin/users?";
        if (!string.IsNullOrEmpty(search)) url += $"search={Uri.EscapeDataString(search)}&";
        if (role.HasValue) url += $"role={role.Value}&";
        return SendAsync<List<UserDto>>(HttpMethod.Get, url.TrimEnd('&', '?'));
    }

    public Task<ApiResponse<UserDto>> CreateStaffAsync(CreateStaffRequest request)
    {
        return SendAsync<UserDto>(HttpMethod.Post, "/api/Admin/users", request);
    }

    public Task<ApiResponse<bool>> ToggleUserStatusAsync(int id)
    {
        return SendAsync<bool>(HttpMethod.Patch, $"/api/Admin/users/{id}/toggle-status");
    }

    public Task<ApiResponse<List<ServiceItemDto>>> GetBranchServicesAsync(int branchId)
    {
        return SendAsync<List<ServiceItemDto>>(HttpMethod.Get, $"/api/Pos/services?branchId={branchId}");
    }

    public Task<ApiResponse<bool>> ApproveBranchAsync(int branchId)
    {
        return SendAsync<bool>(HttpMethod.Patch, $"/api/Branches/{branchId}/approve");
    }

    public Task<ApiResponse<bool>> ToggleBranchStatusAsync(int branchId)
    {
        return SendAsync<bool>(HttpMethod.Patch, $"/api/Branches/{branchId}/toggle-status");
    }

    public Task<ApiResponse<bool>> UpdateCourtStatusAsync(int courtId, SlotStatus status)
    {
        return SendAsync<bool>(HttpMethod.Patch, $"/api/Courts/{courtId}/status", status);
    }
}
