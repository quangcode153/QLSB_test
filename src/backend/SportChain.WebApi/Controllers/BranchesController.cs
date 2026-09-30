using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SportChain.Shared.DTOs.Common;
using SportChain.Shared.DTOs.Courts;
using SportChain.WebApi.Data;
using SportChain.WebApi.Entities;

namespace SportChain.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly AppDbContext _context;

    public BranchesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BranchDto>>>> GetAllBranches([FromQuery] string? city = null, [FromQuery] bool onlyApproved = true)
    {
        var query = _context.Branches.Include(b => b.Courts).AsQueryable();

        // Khách hàng thông thường hoặc người dùng vãng lai chỉ được xem chi nhánh đã duyệt và đang hoạt động
        if (!User.IsInRole("SuperAdmin"))
        {
            query = query.Where(b => b.IsApproved && b.IsActive);
        }
        else if (onlyApproved)
        {
            query = query.Where(b => b.IsApproved && b.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(b => b.City.ToLower().Contains(city.ToLower()));
        }

        var branches = await query.Select(b => new BranchDto
        {
            Id = b.Id,
            Name = b.Name,
            Address = b.Address,
            City = b.City,
            Latitude = b.Latitude,
            Longitude = b.Longitude,
            PhoneNumber = b.PhoneNumber,
            OpenTimeLabel = $"{b.OpenTime:hh\\:mm}",
            CloseTimeLabel = $"{b.CloseTime:hh\\:mm}",
            IsApproved = b.IsApproved,
            IsActive = b.IsActive,
            TotalCourts = b.Courts.Count(c => c.IsActive)
        }).ToListAsync();

        return Ok(ApiResponse<List<BranchDto>>.Ok(branches, $"Lấy thành công {branches.Count} chi nhánh"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BranchDto>>> GetBranchById(int id)
    {
        var b = await _context.Branches.Include(x => x.Courts).FirstOrDefaultAsync(x => x.Id == id);
        if (b == null)
        {
            return NotFound(ApiResponse<BranchDto>.Fail("Không tìm thấy chi nhánh"));
        }

        var dto = new BranchDto
        {
            Id = b.Id,
            Name = b.Name,
            Address = b.Address,
            City = b.City,
            Latitude = b.Latitude,
            Longitude = b.Longitude,
            PhoneNumber = b.PhoneNumber,
            OpenTimeLabel = $"{b.OpenTime:hh\\:mm}",
            CloseTimeLabel = $"{b.CloseTime:hh\\:mm}",
            IsApproved = b.IsApproved,
            IsActive = b.IsActive,
            TotalCourts = b.Courts.Count(c => c.IsActive)
        };

        return Ok(ApiResponse<BranchDto>.Ok(dto));
    }

    [Authorize(Roles = "SuperAdmin,BranchManager")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BranchDto>>> CreateBranch([FromBody] BranchDto dto)
    {
        var branch = new Branch
        {
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            PhoneNumber = dto.PhoneNumber,
            IsApproved = false, // Chờ duyệt
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Branches.AddAsync(branch);
        await _context.SaveChangesAsync();

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? creatorId = int.TryParse(userIdStr, out var uid) ? uid : null;

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = creatorId,
            Action = "CreateBranch",
            EntityName = nameof(Branch),
            EntityId = branch.Id,
            Details = $"Tạo cơ sở mới '{branch.Name}' ({branch.Address}, {branch.City}), trạng thái chờ duyệt.",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "API",
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        dto.Id = branch.Id;
        dto.IsApproved = branch.IsApproved;
        dto.IsActive = branch.IsActive;

        return CreatedAtAction(nameof(GetBranchById), new { id = branch.Id }, ApiResponse<BranchDto>.Ok(dto, "Tạo chi nhánh thành công, đang chờ Admin duyệt"));
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPatch("{id}/approve")]
    public async Task<ActionResult<ApiResponse<bool>>> ApproveBranch(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch == null) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy chi nhánh"));

        branch.IsApproved = true;

        var userIdStr = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        int? adminUserId = int.TryParse(userIdStr, out var uid) ? uid : null;

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = adminUserId,
            Action = "ApproveBranch",
            EntityName = nameof(Branch),
            EntityId = branch.Id,
            Details = $"SuperAdmin duyệt chi nhánh '{branch.Name}' ({branch.Address}, {branch.City}) đi vào hoạt động chính thức.",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Đã duyệt hoạt động chi nhánh thành công"));
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPatch("{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleBranchStatus(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch == null) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy chi nhánh"));

        branch.IsActive = !branch.IsActive;

        var userIdStr = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        int? adminUserId = int.TryParse(userIdStr, out var uid) ? uid : null;

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = adminUserId,
            Action = branch.IsActive ? "ResumeBranch" : "SuspendBranch",
            EntityName = nameof(Branch),
            EntityId = branch.Id,
            Details = $"SuperAdmin {(branch.IsActive ? "mở lại hoạt động" : "tạm dừng hoạt động")} chi nhánh '{branch.Name}'.",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(branch.IsActive, branch.IsActive ? "Đã mở lại hoạt động chi nhánh" : "Đã tạm khóa chi nhánh"));
    }
}
