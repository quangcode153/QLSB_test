namespace SportChain.Shared.DTOs.Admin;

public class ChainStatsDto
{
    public int TotalBranches { get; set; }
    public int TotalCourts { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ConfirmedCount { get; set; }
    public int CompletedCount { get; set; }
    public int NoShowCount { get; set; }
    public int CancelledCount { get; set; }
}

public class ResolveComplaintRequest
{
    public string Status { get; set; } = "Đã giải quyết";
    public string Resolution { get; set; } = string.Empty;
}

public class AuditLogDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string Details { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ComplaintDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int? BookingId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = "Chờ xử lý";
    public string? Resolution { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public SportChain.Shared.Enums.UserRole Role { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateStaffRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public SportChain.Shared.Enums.UserRole Role { get; set; }
    public int? BranchId { get; set; }
}
