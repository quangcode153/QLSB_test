using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SportChain.Shared.DTOs.Bookings;
using SportChain.Shared.DTOs.Common;
using SportChain.Shared.Enums;
using SportChain.WebApi.Data;
using SportChain.WebApi.Entities;
using SportChain.WebApi.Hubs;

namespace SportChain.WebApi.Controllers;

[Authorize(Roles = "SuperAdmin,BranchManager,Receptionist")]
[ApiController]
[Route("api/[controller]")]
public class PosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<CourtHub> _hubContext;

    public PosController(AppDbContext context, IHubContext<CourtHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpGet("services")]
    public async Task<ActionResult<ApiResponse<List<ServiceItemDto>>>> GetBranchServices([FromQuery] int branchId)
    {
        var items = await _context.ServiceItems
            .Where(s => s.BranchId == branchId && s.IsActive)
            .Select(s => new ServiceItemDto
            {
                Id = s.Id,
                BranchId = s.BranchId,
                Name = s.Name,
                Category = s.Category,
                Price = s.Price,
                IsRental = s.IsRental
            }).ToListAsync();

        return Ok(ApiResponse<List<ServiceItemDto>>.Ok(items));
    }

    [HttpPost("add-service")]
    public async Task<ActionResult<ApiResponse<bool>>> AddServiceToBooking([FromBody] AddServiceItemRequest request)
    {
        var booking = await _context.Bookings.FindAsync(request.BookingId);
        if (booking == null) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy đơn đặt sân."));

        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var branchIdClaim = User.FindFirstValue("BranchId");
        int? userBranchId = int.TryParse(branchIdClaim, out var bid) ? bid : null;

        if (!isSuperAdmin && userBranchId.HasValue && booking.BranchId != userBranchId.Value)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<bool>.Fail("Bạn không có quyền thao tác trên đơn của cơ sở khác!"));
        }

        if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
        {
            return BadRequest(ApiResponse<bool>.Fail("Không thể thêm dịch vụ vào đơn đã kết thúc hoặc đã hủy."));
        }

        var service = await _context.ServiceItems.FindAsync(request.ServiceItemId);
        if (service == null) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy dịch vụ/mặt hàng."));

        var item = await _context.BookingServiceItems
            .FirstOrDefaultAsync(b => b.BookingId == request.BookingId && b.ServiceItemId == request.ServiceItemId);

        if (item != null)
        {
            item.Quantity += request.Quantity;
        }
        else
        {
            await _context.BookingServiceItems.AddAsync(new BookingServiceItem
            {
                BookingId = request.BookingId,
                ServiceItemId = request.ServiceItemId,
                Quantity = request.Quantity,
                UnitPrice = service.Price
            });
        }

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, $"Đã thêm {request.Quantity}x {service.Name} vào đơn đặt sân."));
    }

    [HttpPost("walk-in")]
    public async Task<ActionResult<ApiResponse<PosInvoiceDto>>> CreateWalkInBooking([FromBody] WalkInBookingRequest request)
    {
        if (request.TimeSlotIds == null || request.TimeSlotIds.Count == 0)
        {
            return BadRequest(ApiResponse<PosInvoiceDto>.Fail("Vui lòng chọn ít nhất một ca."));
        }

        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var branchIdClaim = User.FindFirstValue("BranchId");
        int? userBranchId = int.TryParse(branchIdClaim, out var bid) ? bid : null;

        if (!isSuperAdmin && userBranchId.HasValue && userBranchId.Value != request.BranchId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<PosInvoiceDto>.Fail("Bạn không có quyền tạo đơn cho cơ sở khác!"));
        }

        if (request.BookingDate < DateOnly.FromDateTime(DateTime.Today))
        {
            return BadRequest(ApiResponse<PosInvoiceDto>.Fail("Không thể tạo đơn cho ngày đã qua trong quá khứ!"));
        }

        var court = await _context.Courts.FindAsync(request.CourtId);
        if (court == null || !court.IsActive)
        {
            return BadRequest(ApiResponse<PosInvoiceDto>.Fail("Sân không tồn tại hoặc đã ngừng hoạt động."));
        }

        if (court.Status == SlotStatus.Maintenance)
        {
            return BadRequest(ApiResponse<PosInvoiceDto>.Fail("Sân này hiện đang trong trạng thái bảo trì, không thể nhận đơn!"));
        }

        var timeSlots = await _context.TimeSlots.Where(s => request.TimeSlotIds.Contains(s.Id)).ToListAsync();
        if (timeSlots.Count != request.TimeSlotIds.Count)
        {
            return BadRequest(ApiResponse<PosInvoiceDto>.Fail("Một hoặc nhiều ca giờ được chọn không tồn tại!"));
        }

        if (request.BookingDate == DateOnly.FromDateTime(DateTime.Today))
        {
            var nowTime = DateTime.Now.TimeOfDay;
            var pastSlot = timeSlots.FirstOrDefault(s => s.EndTime <= nowTime);
            if (pastSlot != null)
            {
                return BadRequest(ApiResponse<PosInvoiceDto>.Fail($"Ca '{pastSlot.DisplayLabel}' đã kết thúc trong ngày hôm nay, không thể đặt!"));
            }
        }

        // Quy tắc BR-POS-03 & BR-CONCUR-02: Kiểm tra chống trùng lịch với đơn online/cọc trước hoặc ca đã hoàn thành
        var isConflict = await _context.BookingDetails
            .Include(d => d.Booking)
            .AnyAsync(d => d.Booking.CourtId == request.CourtId
                        && d.Booking.BookingDate == request.BookingDate
                        && request.TimeSlotIds.Contains(d.TimeSlotId)
                        && (d.Booking.Status == BookingStatus.Confirmed
                            || d.Booking.Status == BookingStatus.InUse
                            || d.Booking.Status == BookingStatus.Completed
                            || (d.Booking.Status == BookingStatus.PendingPayment && d.Booking.ExpireHoldingAt > DateTime.UtcNow)));

        if (isConflict)
        {
            return Conflict(ApiResponse<PosInvoiceDto>.Fail("Một hoặc nhiều ca sân vừa được khách khác đặt hoặc giữ cọc! Vui lòng làm mới ma trận."));
        }

        // Tìm hoặc tạo tài khoản khách vãng lai
        var customer = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.CustomerPhone);
        if (customer == null)
        {
            customer = new User
            {
                FullName = string.IsNullOrWhiteSpace(request.CustomerName) ? "Khách Vãng Lai" : request.CustomerName.Trim(),
                PhoneNumber = request.CustomerPhone,
                Email = $"guest_{Random.Shared.Next(10000, 99999)}@sportchain.local",
                PasswordHash = "GUEST_NO_PASS",
                Role = UserRole.Customer,
                IsActive = true
            };
            await _context.Users.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        var priceRules = await _context.PriceRules.Where(p => p.CourtId == request.CourtId).ToListAsync();

        decimal total = 0;
        var details = new List<BookingDetail>();
        foreach (var slot in timeSlots)
        {
            bool isPeak = slot.StartTime >= new TimeSpan(17, 0, 0) && slot.StartTime < new TimeSpan(21, 0, 0);
            var rule = priceRules.FirstOrDefault(r => r.TimeSlotId == slot.Id);
            decimal price = rule?.PricePerHour ?? SportChain.Shared.Constants.SystemPolicies.GetStandardPrice(court.SportType, isPeak);
            total += price;
            details.Add(new BookingDetail
            {
                TimeSlotId = slot.Id,
                SlotPrice = price
            });
        }

        var booking = new Booking
        {
            CustomerId = customer.Id,
            BranchId = request.BranchId,
            CourtId = request.CourtId,
            BookingDate = request.BookingDate,
            TotalAmount = total,
            DepositAmount = total, // Khách vãng lai thu trước 100%
            Status = BookingStatus.InUse, // Vào sân ngay
            PaymentMethod = request.PaymentMethod,
            BookingCode = $"WALK-{request.BookingDate:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            CheckInCode = Guid.NewGuid().ToString("N").ToUpper(),
            CreatedAt = DateTime.UtcNow,
            CheckedInAt = DateTime.UtcNow,
            ExpireHoldingAt = DateTime.UtcNow.AddHours(2),
            Details = details
        };

        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync(); // Sinh Booking.Id trước khi ghi AuditLog

        var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? staffUserId = int.TryParse(staffIdStr, out var sid) ? sid : null;

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = staffUserId,
            Action = "CreateWalkInBooking",
            EntityName = nameof(Booking),
            EntityId = booking.Id, // Đã có ID chính xác
            Details = $"Tạo đơn vãng lai tại quầy {booking.BookingCode} cho khách {customer.FullName} ({customer.PhoneNumber}) - Tổng tiền: {booking.TotalAmount:N0} đ.",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "POS",
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        // Broadcast Realtime sang InUse kèm ngày
        var dateStr = request.BookingDate.ToString("yyyy-MM-dd");
        foreach (var slotId in request.TimeSlotIds)
        {
            await _hubContext.Clients.Group($"Branch_{request.BranchId}")
                .SendAsync("OnSlotStatusChanged", request.CourtId, slotId, SlotStatus.InUse, dateStr);
        }

        var invoice = await GetInvoiceInternal(booking.Id);
        return Ok(ApiResponse<PosInvoiceDto>.Ok(invoice!, "Đặt sân tại quầy thành công, khách đã vào sân."));
    }

    [HttpPost("complete-payment")]
    public async Task<ActionResult<ApiResponse<bool>>> CompletePayment([FromBody] CompletePaymentRequest request)
    {
        var booking = await _context.Bookings
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId);

        if (booking == null) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy đơn đặt sân."));

        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var branchIdClaim = User.FindFirstValue("BranchId");
        int? userBranchId = int.TryParse(branchIdClaim, out var bid) ? bid : null;

        if (!isSuperAdmin && userBranchId.HasValue && booking.BranchId != userBranchId.Value)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<bool>.Fail("Bạn không có quyền thao tác trên đơn của cơ sở khác!"));
        }

        if (booking.Status == BookingStatus.Completed)
        {
            return BadRequest(ApiResponse<bool>.Fail("Đơn đặt sân này đã được thanh toán và hoàn tất trước đó."));
        }

        if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.NoShow)
        {
            return BadRequest(ApiResponse<bool>.Fail($"Không thể thanh toán cho đơn đang ở trạng thái '{booking.Status}'."));
        }

        if (booking.Status == BookingStatus.PendingPayment)
        {
            return BadRequest(ApiResponse<bool>.Fail("Đơn đặt sân chưa hoàn tất đặt cọc, không thể quyết toán."));
        }

        booking.Status = BookingStatus.Completed;
        booking.PaymentMethod = request.PaymentMethod;

        var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? staffUserId = int.TryParse(staffIdStr, out var sid) ? sid : null;

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = staffUserId,
            Action = "CompletePayment",
            EntityName = nameof(Booking),
            EntityId = booking.Id,
            Details = $"Hoàn tất thanh toán và kết thúc buổi chơi cho đơn {booking.BookingCode} (Phương thức: {booking.PaymentMethod}).",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "POS",
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        // Broadcast Realtime sang Booked / Completed kèm ngày (Không được đổi sang Available vì ca này đã kết thúc)
        var dateStr = booking.BookingDate.ToString("yyyy-MM-dd");
        foreach (var detail in booking.Details)
        {
            await _hubContext.Clients.Group($"Branch_{booking.BranchId}")
                .SendAsync("OnSlotStatusChanged", booking.CourtId, detail.TimeSlotId, SlotStatus.Booked, dateStr);
        }

        return Ok(ApiResponse<bool>.Ok(true, "Hoàn tất thanh toán và kết thúc buổi chơi thành công!"));
    }

    [HttpGet("invoice/{bookingId}")]
    public async Task<ActionResult<ApiResponse<PosInvoiceDto>>> GetInvoice(int bookingId)
    {
        var invoice = await GetInvoiceInternal(bookingId);
        if (invoice == null) return NotFound(ApiResponse<PosInvoiceDto>.Fail("Không tìm thấy hóa đơn."));

        return Ok(ApiResponse<PosInvoiceDto>.Ok(invoice));
    }

    private async Task<PosInvoiceDto?> GetInvoiceInternal(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Branch)
            .Include(b => b.Court)
            .Include(b => b.Details)
                .ThenInclude(d => d.TimeSlot)
            .Include(b => b.ServiceItems)
                .ThenInclude(s => s.ServiceItem)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null) return null;

        var services = booking.ServiceItems.Select(s => new BookingServiceItemSummaryDto
        {
            ServiceItemId = s.ServiceItemId,
            ServiceName = s.ServiceItem.Name,
            Quantity = s.Quantity,
            UnitPrice = s.UnitPrice
        }).ToList();

        decimal totalServices = services.Sum(s => s.SubTotal);
        decimal remainingRent = Math.Max(0, booking.TotalAmount - booking.DepositAmount);
        decimal totalToPay = remainingRent + totalServices;

        return new PosInvoiceDto
        {
            BookingId = booking.Id,
            BookingCode = booking.BookingCode,
            CustomerName = booking.Customer?.FullName ?? string.Empty,
            CustomerPhone = booking.Customer?.PhoneNumber ?? string.Empty,
            BranchName = booking.Branch?.Name ?? string.Empty,
            CourtName = booking.Court?.Name ?? string.Empty,
            BookingDate = booking.BookingDate,
            SlotLabels = booking.Details.Select(d => d.TimeSlot.DisplayLabel).ToList(),
            CourtRentAmount = booking.TotalAmount,
            DepositPaid = booking.DepositAmount,
            Services = services,
            TotalServicesAmount = totalServices,
            RemainingAmountToPay = totalToPay,
            Status = booking.Status
        };
    }
}
