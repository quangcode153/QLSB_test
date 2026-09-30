using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportChain.Shared.DTOs.Bookings;
using SportChain.Shared.DTOs.Common;
using SportChain.WebApi.Entities;
using SportChain.WebApi.Repositories;
using SportChain.WebApi.Services;

namespace SportChain.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IBookingRepository _bookingRepo;
    private readonly IQrCodeService _qrCodeService;

    public BookingsController(IBookingService bookingService, IBookingRepository bookingRepo, IQrCodeService qrCodeService)
    {
        _bookingService = bookingService;
        _bookingRepo = bookingRepo;
        _qrCodeService = qrCodeService;
    }

    [Authorize]
    [HttpPost("hold")]
    public async Task<ActionResult<ApiResponse<BookingDto>>> CreateHoldingBooking([FromBody] CreateBookingRequest request)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var currentUserId))
        {
            return Unauthorized(ApiResponse<BookingDto>.Fail("Vui lòng đăng nhập trước khi đặt sân."));
        }

        try
        {
            var booking = await _bookingService.CreateHoldingBookingAsync(
                currentUserId, 
                request.CourtId, 
                request.BookingDate, 
                request.TimeSlotIds);

            var fullBooking = await _bookingRepo.GetByIdAsync(booking.Id);
            return Ok(ApiResponse<BookingDto>.Ok(MapToDto(fullBooking!), "Giữ chỗ thành công! Vui lòng đặt cọc trong 15 phút."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<BookingDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<BookingDto>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpPost("{id}/confirm-deposit")]
    public async Task<ActionResult<ApiResponse<BookingDto>>> ConfirmDeposit(int id, [FromBody] ConfirmDepositRequest request)
    {
        var existingBooking = await _bookingRepo.GetByIdAsync(id);
        if (existingBooking == null) return NotFound(ApiResponse<BookingDto>.Fail("Không tìm thấy đơn đặt sân."));

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? currentUserId = int.TryParse(userIdStr, out var uid) ? uid : null;
        var isStaff = User.IsInRole("SuperAdmin") || User.IsInRole("BranchManager") || User.IsInRole("Receptionist");

        if (!isStaff && existingBooking.CustomerId != currentUserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<BookingDto>.Fail("Bạn không có quyền thanh toán cọc cho đơn của người khác!"));
        }

        var success = await _bookingService.ConfirmDepositAsync(id, request.PaymentMethod);
        if (!success)
        {
            return BadRequest(ApiResponse<BookingDto>.Fail("Không thể xác nhận đặt cọc (Đơn đã hết hạn hoặc không hợp lệ)."));
        }

        var booking = await _bookingRepo.GetByIdAsync(id);
        return Ok(ApiResponse<BookingDto>.Ok(MapToDto(booking!), "Thanh toán cọc thành công! Vé điện tử và mã QR đã được tạo."));
    }

    [Authorize(Roles = "SuperAdmin,BranchManager,Receptionist")]
    [HttpPost("check-in")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckInWithQr([FromBody] CheckInRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CheckInCode))
        {
            return BadRequest(ApiResponse<bool>.Fail("Mã Check-in không được để trống."));
        }

        var branchIdClaim = User.FindFirstValue("BranchId");
        int? staffBranchId = int.TryParse(branchIdClaim, out var bid) ? bid : null;
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? staffUserId = int.TryParse(userIdStr, out var uid) ? uid : null;

        var (success, message) = await _bookingService.CheckInWithQrAsync(request.CheckInCode.Trim(), staffBranchId, isSuperAdmin, staffUserId);
        if (!success)
        {
            return BadRequest(ApiResponse<bool>.Fail(message));
        }

        return Ok(ApiResponse<bool>.Ok(true, message));
    }

    [Authorize]
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<bool>>> CancelBooking(int id, [FromBody] CancelBookingRequest request)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? currentUserId = int.TryParse(userIdStr, out var uid) ? uid : null;
        var roleStr = User.FindFirstValue(ClaimTypes.Role);
        SportChain.Shared.Enums.UserRole? userRole = Enum.TryParse<SportChain.Shared.Enums.UserRole>(roleStr, out var r) ? r : null;
        var branchIdClaim = User.FindFirstValue("BranchId");
        int? staffBranchId = int.TryParse(branchIdClaim, out var bid) ? bid : null;

        var (success, message) = await _bookingService.CancelBookingAsync(id, request.Reason, currentUserId, userRole, staffBranchId);
        if (!success)
        {
            return BadRequest(ApiResponse<bool>.Fail(message));
        }

        return Ok(ApiResponse<bool>.Ok(true, message));
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BookingDto>>> GetBookingById(int id)
    {
        var booking = await _bookingRepo.GetByIdAsync(id);
        if (booking == null) return NotFound(ApiResponse<BookingDto>.Fail("Không tìm thấy đơn đặt sân."));

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? currentUserId = int.TryParse(userIdStr, out var uid) ? uid : null;
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var branchIdClaim = User.FindFirstValue("BranchId");
        int? staffBranchId = int.TryParse(branchIdClaim, out var bid) ? bid : null;

        // Nếu là Customer: Chỉ xem được đơn của chính mình
        if (!isSuperAdmin && !staffBranchId.HasValue && booking.CustomerId != currentUserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<BookingDto>.Fail("Bạn không có quyền xem thông tin đơn của người khác!"));
        }

        // Nếu là Nhân viên cơ sở: Chỉ xem được đơn của cơ sở mình
        if (!isSuperAdmin && staffBranchId.HasValue && booking.BranchId != staffBranchId.Value)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<BookingDto>.Fail("Bạn không có quyền xem đơn của cơ sở khác!"));
        }

        return Ok(ApiResponse<BookingDto>.Ok(MapToDto(booking)));
    }

    [Authorize]
    [HttpGet("my-bookings")]
    public async Task<ActionResult<ApiResponse<List<BookingDto>>>> GetMyBookings()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var currentUserId))
        {
            return Unauthorized(ApiResponse<List<BookingDto>>.Fail("Vui lòng đăng nhập."));
        }

        var bookings = await _bookingRepo.GetBookingsByCustomerAsync(currentUserId);
        var dtos = bookings.Select(MapToDto).ToList();

        return Ok(ApiResponse<List<BookingDto>>.Ok(dtos));
    }

    [Authorize(Roles = "SuperAdmin,BranchManager,Receptionist")]
    [HttpGet("branch/{branchId}")]
    public async Task<ActionResult<ApiResponse<List<BookingDto>>>> GetBranchBookings(int branchId, [FromQuery] DateOnly? date = null)
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var branchIdClaim = User.FindFirstValue("BranchId");
        int? staffBranchId = int.TryParse(branchIdClaim, out var bid) ? bid : null;

        if (!isSuperAdmin && staffBranchId.HasValue && staffBranchId.Value != branchId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<List<BookingDto>>.Fail("Bạn không có quyền xem dữ liệu của cơ sở khác!"));
        }

        var targetDate = date ?? DateOnly.FromDateTime(DateTime.Now);
        var bookings = await _bookingRepo.GetBookingsByBranchAndDateAsync(branchId, targetDate);
        var dtos = bookings.Select(MapToDto).ToList();

        return Ok(ApiResponse<List<BookingDto>>.Ok(dtos));
    }

    private BookingDto MapToDto(Booking b)
    {
        string? qrBase64 = null;
        if (!string.IsNullOrEmpty(b.CheckInCode))
        {
            try
            {
                qrBase64 = _qrCodeService.GenerateQrCodeBase64(b.CheckInCode);
            }
            catch
            {
                // Bỏ qua nếu lỗi sinh qr
            }
        }

        return new BookingDto
        {
            Id = b.Id,
            BookingCode = b.BookingCode,
            CheckInCode = b.CheckInCode,
            CustomerId = b.CustomerId,
            CustomerName = b.Customer?.FullName ?? string.Empty,
            CustomerPhone = b.Customer?.PhoneNumber ?? string.Empty,
            BranchId = b.BranchId,
            BranchName = b.Branch?.Name ?? string.Empty,
            CourtId = b.CourtId,
            CourtName = b.Court?.Name ?? string.Empty,
            BookingDate = b.BookingDate,
            TotalAmount = b.TotalAmount,
            DepositAmount = b.DepositAmount,
            RefundAmount = b.RefundAmount,
            ServiceAmount = b.ServiceItems?.Sum(s => s.Quantity * s.UnitPrice) ?? 0,
            Status = b.Status,
            PaymentMethod = b.PaymentMethod,
            CreatedAt = b.CreatedAt,
            ExpireHoldingAt = b.ExpireHoldingAt,
            TimeSlotIds = b.Details.Select(d => d.TimeSlotId).ToList(),
            SlotLabels = b.Details.Select(d => d.TimeSlot?.DisplayLabel ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)).ToList(),
            QrCodeBase64 = qrBase64
        };
    }
}
