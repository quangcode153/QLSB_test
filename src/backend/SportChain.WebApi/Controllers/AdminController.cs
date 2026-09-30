using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportChain.Shared.DTOs.Admin;
using SportChain.Shared.DTOs.Common;
using SportChain.Shared.Enums;
using SportChain.WebApi.Data;

using SportChain.WebApi.Entities;
using SportChain.WebApi.Security;

namespace SportChain.WebApi.Controllers;

[Authorize(Roles = "SuperAdmin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public AdminController(AppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetUsers([FromQuery] string? search = null, [FromQuery] UserRole? role = null)
    {
        var query = _context.Users.Include(u => u.Branch).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower().Trim();
            query = query.Where(u => u.FullName.ToLower().Contains(s) || u.Email.ToLower().Contains(s) || u.PhoneNumber.Contains(s));
        }

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
                BranchId = u.BranchId,
                BranchName = u.Branch != null ? u.Branch.Name : "Toàn hệ thống",
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<UserDto>>.Ok(users, $"Lấy thành công {users.Count} tài khoản."));
    }

    [HttpPost("users")]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateStaffUser([FromBody] CreateStaffRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(ApiResponse<UserDto>.Fail("Họ tên, email và mật khẩu không được để trống."));
        }

        var emailLower = request.Email.Trim().ToLower();
        var exists = await _context.Users.AnyAsync(u => u.Email.ToLower() == emailLower);
        if (exists)
        {
            return Conflict(ApiResponse<UserDto>.Fail("Email này đã được sử dụng trong hệ thống."));
        }

        if (request.Role == UserRole.BranchManager || request.Role == UserRole.Receptionist)
        {
            if (!request.BranchId.HasValue)
            {
                return BadRequest(ApiResponse<UserDto>.Fail("Vui lòng chọn cơ sở/chi nhánh làm việc cho Quản lý hoặc Lễ tân."));
            }
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = emailLower,
            PhoneNumber = request.PhoneNumber?.Trim() ?? string.Empty,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = request.Role,
            BranchId = request.Role == UserRole.SuperAdmin ? null : request.BranchId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var adminIdStr = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        int? adminUserId = int.TryParse(adminIdStr, out var aid) ? aid : null;

        // Thêm Audit log với đầy đủ UserId, EntityId và IpAddress
        var audit = new AuditLog
        {
            UserId = adminUserId,
            Action = "CreateStaffAccount",
            EntityName = nameof(User),
            EntityId = user.Id,
            Details = $"SuperAdmin tạo tài khoản nhân sự mới: {user.FullName} ({user.Role}) - Email: {user.Email}",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "API",
            Timestamp = DateTime.UtcNow
        };
        await _context.AuditLogs.AddAsync(audit);
        await _context.SaveChangesAsync();

        var branch = user.BranchId.HasValue ? await _context.Branches.FindAsync(user.BranchId.Value) : null;
        var dto = new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            BranchId = user.BranchId,
            BranchName = branch?.Name ?? "Toàn hệ thống",
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Ok(ApiResponse<UserDto>.Ok(dto, "Tạo tài khoản nhân sự thành công!"));
    }

    [HttpPatch("users/{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleUserStatus(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy tài khoản."));

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(user.IsActive, user.IsActive ? "Đã mở khóa tài khoản thành công." : "Đã tạm dừng hoạt động tài khoản."));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<ChainStatsDto>>> GetChainStats()
    {
        var totalBranches = await _context.Branches.CountAsync();
        var totalCourts = await _context.Courts.CountAsync(c => c.IsActive);
        var bookings = await _context.Bookings
            .Include(b => b.ServiceItems)
            .ToListAsync();

        // Tính doanh thu thực tế chính xác:
        // - Completed & InUse: 100% tiền sân + 100% tiền dịch vụ (nước, thuê vợt)
        // - Confirmed: 30% tiền cọc thực tế đã thu qua cổng thanh toán
        // - NoShow: 100% tiền cọc bị tịch thu (do khách bỏ sân)
        // - Cancelled: Tiền phạt hủy cọc giữ lại (DepositAmount - RefundAmount)
        decimal totalRevenue = 0;
        foreach (var b in bookings)
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
                    var retainedDeposit = b.DepositAmount - b.RefundAmount;
                    if (retainedDeposit > 0)
                    {
                        totalRevenue += retainedDeposit;
                    }
                    break;
            }
        }

        var stats = new ChainStatsDto
        {
            TotalBranches = totalBranches,
            TotalCourts = totalCourts,
            TotalBookings = bookings.Count,
            TotalRevenue = totalRevenue,
            ConfirmedCount = bookings.Count(b => b.Status == BookingStatus.Confirmed),
            CompletedCount = bookings.Count(b => b.Status == BookingStatus.Completed),
            NoShowCount = bookings.Count(b => b.Status == BookingStatus.NoShow),
            CancelledCount = bookings.Count(b => b.Status == BookingStatus.Cancelled)
        };

        return Ok(ApiResponse<ChainStatsDto>.Ok(stats, "Lấy thống kê chuỗi thành công."));
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<ApiResponse<List<AuditLogDto>>>> GetAuditLogs([FromQuery] int limit = 50)
    {
        var logs = await _context.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.User != null ? a.User.FullName : "Hệ thống Tự động",
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Details = a.Details,
                IpAddress = a.IpAddress,
                Timestamp = a.Timestamp
            })
            .ToListAsync();

        return Ok(ApiResponse<List<AuditLogDto>>.Ok(logs, $"Lấy thành công {logs.Count} nhật ký kiểm toán."));
    }

    [HttpGet("complaints")]
    public async Task<ActionResult<ApiResponse<List<ComplaintDto>>>> GetComplaints()
    {
        var complaints = await _context.Complaints
            .Include(c => c.Customer)
            .Include(c => c.Branch)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ComplaintDto
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer.FullName,
                CustomerPhone = c.Customer.PhoneNumber,
                BranchId = c.BranchId,
                BranchName = c.Branch.Name,
                BookingId = c.BookingId,
                Content = c.Content,
                Status = c.Status,
                Resolution = c.Resolution,
                CreatedAt = c.CreatedAt,
                ResolvedAt = c.ResolvedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<ComplaintDto>>.Ok(complaints, $"Lấy thành công {complaints.Count} khiếu nại."));
    }

    [HttpPatch("complaints/{id}/resolve")]
    public async Task<ActionResult<ApiResponse<bool>>> ResolveComplaint(int id, [FromBody] ResolveComplaintRequest request)
    {
        var complaint = await _context.Complaints.FindAsync(id);
        if (complaint == null) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy khiếu nại."));

        complaint.Status = request.Status;
        complaint.Resolution = request.Resolution;
        complaint.ResolvedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Đã cập nhật trạng thái khiếu nại thành công."));
    }
}
