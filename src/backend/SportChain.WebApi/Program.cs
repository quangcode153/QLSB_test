using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using SportChain.WebApi.Data;
using SportChain.WebApi.Hubs;
using SportChain.WebApi.Repositories;
using SportChain.WebApi.Security;
using SportChain.WebApi.Services;
using SportChain.WebApi.Workers;

// Tá»± Ä‘á»™ng náº¡p file .env tá»« thÆ° má»¥c gá»‘c
var currentDir = Directory.GetCurrentDirectory();
var envPath = File.Exists(Path.Combine(currentDir, ".env"))
    ? Path.Combine(currentDir, ".env")
    : Path.Combine(Directory.GetParent(currentDir)?.Parent?.FullName ?? currentDir, ".env");

if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#")) continue;
        var parts = trimmed.Split('=', 2);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
}

var builder = WebApplication.CreateBuilder(args);

// 1. Cáº¥u hÃ¬nh Database EF Core (SQL Server)
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost\\SQLEXPRESS";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "SportChainDb";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "sa";
var dbPass = Environment.GetEnvironmentVariable("DB_PASSWORD");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? (string.IsNullOrEmpty(dbPass)
        ? $"Server={dbServer};Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;"
        : $"Server={dbServer};Database={dbName};User Id={dbUser};Password={dbPass};TrustServerCertificate=True;");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));

// 2. Cáº¥u hÃ¬nh CORS CHÃNH XÃC NGUá»’N (Strict Whitelisting)
builder.Services.AddCors(options =>
{
    options.AddPolicy("SportChainCorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // Báº¯t buá»™c cho káº¿t ná»‘i WebSocket Realtime SignalR
    });
});

// 3. ÄÄƒng kÃ½ cÃ¡c dá»‹ch vá»¥ Dependency Injection (DI)
builder.Services.AddControllers();
builder.Services.AddSignalR(); // Äá»™ng cÆ¡ WebSocket Realtime
builder.Services.AddEndpointsApiExplorer();

// Cáº¥u hÃ¬nh Swagger kÃ¨m nÃºt xÃ¡c thá»±c Bearer Token
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SportChain Web API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nháº­p JWT token theo Ä‘á»‹nh dáº¡ng: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Core Security & Auth
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Repositories
builder.Services.AddScoped<ICourtRepository, CourtRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// Business Services
builder.Services.AddSingleton<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ICourtMatrixService, CourtMatrixService>();

// 4. ÄÄƒng kÃ½ Background Worker cháº¡y ngáº§m (QuÃ©t 15p cá»c & 15p no-show)
builder.Services.AddHostedService<BookingWatcherWorker>();

// 5. Cấu hình Authentication & JWT Token
var jwtSecret = builder.Configuration["Jwt:SecretKey"] 
    ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
    ?? "SportChain_Ultra_Secret_Key_For_Jwt_Auth_2026_Secure_Token!";

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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SportChain API v1");
    });
}
else
{
    app.UseHttpsRedirection();
}

// Ãp dá»¥ng CORS trÆ°á»›c Authentication & Authorization
app.UseCors("SportChainCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Äá»‹nh tuyáº¿n API Controllers
app.MapControllers();

// Má»Ÿ cá»•ng káº¿t ná»‘i WebSocket Realtime liÃªn tá»¥c tá»« khi khÃ¡ch truy cáº­p
app.MapHub<CourtHub>("/hubs/court");

// Tá»± Ä‘á»™ng kiá»ƒm tra vÃ  Seed dá»¯ liá»‡u máº«u ban Ä‘áº§u (Chi nhÃ¡nh, SÃ¢n, Ca giá», TÃ i khoáº£n, Báº£ng giÃ¡)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    var hasher = services.GetRequiredService<IPasswordHasher>();
    await DbSeeder.SeedAsync(context, hasher);
}

app.Run();

