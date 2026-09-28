using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SportChain.WebApi.Data;
using SportChain.WebApi.Hubs;
using SportChain.WebApi.Security;
using SportChain.WebApi.Services;
using SportChain.WebApi.Workers;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database EF Core (SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=localhost\\SQLEXPRESS;Database=SportChainDb;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Cấu hình CORS CHÍNH XÁC NGUỒN (Strict Whitelisting)
// Chỉ chấp nhận request từ cổng của Frontend Blazor (5001 local / 8080 docker)
builder.Services.AddCors(options =>
{
    options.AddPolicy("SportChainCorsPolicy", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5001",   // Frontend Web Blazor Local (HTTP)
            "https://localhost:7001",  // Frontend Web Blazor Local (HTTPS)
            "http://localhost:8080"    // Frontend Web khi chạy Docker trên WSL
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // Bắt buộc cho kết nối WebSocket Realtime SignalR
    });
});

// 3. Đăng ký các dịch vụ Dependency Injection (DI)
builder.Services.AddControllers();
builder.Services.AddSignalR(); // Động cơ WebSocket Realtime
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Core Services & Security
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// 4. Đăng ký Background Worker chạy ngầm (Quét 15p cọc & 15p no-show)
builder.Services.AddHostedService<BookingWatcherWorker>();

// 5. Cấu hình Authentication & JWT Token
var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "SportChain_Ultra_Secret_Key_For_Jwt_Auth_2026_Secure_Token!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Áp dụng CORS trước Authentication & Authorization
app.UseCors("SportChainCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Định tuyến API Controllers
app.MapControllers();

// Mở cổng kết nối WebSocket Realtime liên tục từ khi khách truy cập
app.MapHub<CourtHub>("/hubs/court");

app.Run();
