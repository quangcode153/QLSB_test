using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportChain.Shared.DTOs.Auth;
using SportChain.Shared.DTOs.Common;
using SportChain.Shared.Enums;
using SportChain.WebApi.Data;
using SportChain.WebApi.Entities;
using SportChain.WebApi.Security;

namespace SportChain.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthController(AppDbContext context, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail("Email và mật khẩu không được để trống."));
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Email hoặc mật khẩu không chính xác."));
        }

        if (!user.IsActive)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Tài khoản của bạn đã bị tạm khóa. Vui lòng liên hệ quản trị viên."));
        }

        var token = _tokenService.GenerateJwtToken(user);
        var response = new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            BranchId = user.BranchId
        };

        return Ok(ApiResponse<AuthResponse>.Ok(response, "Đăng nhập thành công!"));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail("Họ tên, email và mật khẩu không được để trống."));
        }

        var exists = await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());
        if (exists)
        {
            return Conflict(ApiResponse<AuthResponse>.Fail("Email này đã được đăng ký tài khoản trong hệ thống."));
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            PhoneNumber = request.PhoneNumber?.Trim() ?? string.Empty,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = UserRole.Customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateJwtToken(user);
        var response = new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            BranchId = null
        };

        return Ok(ApiResponse<AuthResponse>.Ok(response, "Đăng ký tài khoản thành công!"));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> GetCurrentUser()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Token không hợp lệ."));
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(ApiResponse<AuthResponse>.Fail("Không tìm thấy thông tin tài khoản."));
        }

        var response = new AuthResponse
        {
            Token = string.Empty,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            BranchId = user.BranchId
        };

        return Ok(ApiResponse<AuthResponse>.Ok(response, "Lấy thông tin người dùng thành công."));
    }
}
