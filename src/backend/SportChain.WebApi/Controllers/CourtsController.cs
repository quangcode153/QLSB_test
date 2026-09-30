using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SportChain.Shared.Constants;
using SportChain.Shared.DTOs.Common;
using SportChain.Shared.DTOs.Courts;
using SportChain.Shared.Enums;
using SportChain.WebApi.Data;
using SportChain.WebApi.Entities;
using SportChain.WebApi.Hubs;
using SportChain.WebApi.Repositories;
using SportChain.WebApi.Services;

namespace SportChain.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourtsController : ControllerBase
{
    private readonly ICourtRepository _courtRepo;
    private readonly ICourtMatrixService _matrixService;
    private readonly AppDbContext _context;
    private readonly IHubContext<CourtHub> _hubContext;

    public CourtsController(
        ICourtRepository courtRepo, 
        ICourtMatrixService matrixService,
        AppDbContext context,
        IHubContext<CourtHub> hubContext)
    {
        _courtRepo = courtRepo;
        _matrixService = matrixService;
        _context = context;
        _hubContext = hubContext;
    }

    [HttpGet("branch/{branchId}")]
    public async Task<ActionResult<ApiResponse<List<CourtDto>>>> GetCourtsByBranch(int branchId, [FromQuery] SportType? sportType = null)
    {
        var courts = await _courtRepo.GetCourtsByBranchAsync(branchId, sportType);
        var dtos = courts.Select(c => new CourtDto
        {
            Id = c.Id,
            BranchId = c.BranchId,
            Name = c.Name,
            SportType = c.SportType,
            SurfaceType = c.SurfaceType,
            Status = c.Status,
            DefaultPrice = c.PriceRules.FirstOrDefault()?.PricePerHour ?? SystemPolicies.GetStandardPrice(c.SportType, false)
        }).ToList();

        return Ok(ApiResponse<List<CourtDto>>.Ok(dtos));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CourtDto>>> GetCourtById(int id)
    {
        var c = await _courtRepo.GetByIdAsync(id);
        if (c == null) return NotFound(ApiResponse<CourtDto>.Fail("Không tìm thấy sân"));

        var dto = new CourtDto
        {
            Id = c.Id,
            BranchId = c.BranchId,
            Name = c.Name,
            SportType = c.SportType,
            SurfaceType = c.SurfaceType,
            Status = c.Status,
            DefaultPrice = c.PriceRules.FirstOrDefault()?.PricePerHour ?? SystemPolicies.GetStandardPrice(c.SportType, false)
        };

        return Ok(ApiResponse<CourtDto>.Ok(dto));
    }

    /// <summary>
    /// Lấy ma trận lịch sân chi nhánh theo ngày phục vụ giao diện Interactive Booking Matrix
    /// </summary>
    [HttpGet("branch/{branchId}/matrix")]
    public async Task<ActionResult<ApiResponse<BranchMatrixDto>>> GetBranchMatrix(int branchId, [FromQuery] DateOnly? date = null)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.Now);
        try
        {
            var matrix = await _matrixService.GetBranchMatrixAsync(branchId, targetDate);
            return Ok(ApiResponse<BranchMatrixDto>.Ok(matrix));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<BranchMatrixDto>.Fail(ex.Message));
        }
    }

    [Authorize(Roles = "SuperAdmin,BranchManager")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CourtDto>>> CreateCourt([FromBody] CourtDto dto)
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var branchIdClaim = User.FindFirstValue("BranchId");
        int? staffBranchId = int.TryParse(branchIdClaim, out var bid) ? bid : null;

        if (!isSuperAdmin && staffBranchId.HasValue && staffBranchId.Value != dto.BranchId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<CourtDto>.Fail("Bạn không có quyền tạo sân cho cơ sở khác!"));
        }

        var court = new Court
        {
            BranchId = dto.BranchId,
            Name = dto.Name,
            SportType = dto.SportType,
            SurfaceType = dto.SurfaceType,
            Status = SlotStatus.Available,
            IsActive = true
        };

        await _courtRepo.AddCourtAsync(court);
        await _courtRepo.SaveChangesAsync();

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? currentUserId = int.TryParse(userIdStr, out var uid) ? uid : null;

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = currentUserId,
            Action = "CreateCourt",
            EntityName = nameof(Court),
            EntityId = court.Id,
            Details = $"Tạo sân mới: {court.Name} ({court.SportType} - {court.SurfaceType}) tại chi nhánh ID {court.BranchId}.",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "API",
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        dto.Id = court.Id;
        return CreatedAtAction(nameof(GetCourtById), new { id = court.Id }, ApiResponse<CourtDto>.Ok(dto, "Thêm sân thành công"));
    }

    [Authorize(Roles = "SuperAdmin,BranchManager,Receptionist")]
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateCourtStatus(int id, [FromBody] SlotStatus status)
    {
        var court = await _courtRepo.GetByIdAsync(id);
        if (court == null) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy sân"));

        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var branchIdClaim = User.FindFirstValue("BranchId");
        int? staffBranchId = int.TryParse(branchIdClaim, out var bid) ? bid : null;

        if (!isSuperAdmin && staffBranchId.HasValue && staffBranchId.Value != court.BranchId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<bool>.Fail("Bạn không có quyền can thiệp vào sân của cơ sở khác!"));
        }

        var oldStatus = court.Status;
        court.Status = status;

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? currentUserId = int.TryParse(userIdStr, out var uid) ? uid : null;

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = currentUserId,
            Action = "UpdateCourtStatus",
            EntityName = nameof(Court),
            EntityId = court.Id,
            Details = $"Đổi trạng thái sân {court.Name} (Cơ sở ID {court.BranchId}) từ {oldStatus} sang {status}.",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "API",
            Timestamp = DateTime.UtcNow
        });

        await _courtRepo.UpdateCourtAsync(court);
        await _courtRepo.SaveChangesAsync();

        // Broadcast realtime cập nhật trạng thái sân tới chi nhánh (dateStr = null áp dụng cho tất cả ngày)
        var timeSlots = await _courtRepo.GetAllTimeSlotsAsync();
        foreach (var slot in timeSlots)
        {
            await _hubContext.Clients.Group($"Branch_{court.BranchId}")
                .SendAsync("OnSlotStatusChanged", court.Id, slot.Id, status, (string?)null);
        }

        return Ok(ApiResponse<bool>.Ok(true, $"Đã cập nhật trạng thái sân thành {status}"));
    }
}
