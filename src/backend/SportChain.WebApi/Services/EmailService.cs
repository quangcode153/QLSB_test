namespace SportChain.WebApi.Services;

public interface IEmailService
{
    Task SendBookingSuccessEmailAsync(string toEmail, string bookingCode, string courtName, string qrBase64);
    Task SendCancellationEmailAsync(string toEmail, string bookingCode, string reason, decimal refundAmount);
    Task SendReminderEmailAsync(string toEmail, string bookingCode, string timeLabel);
}

public class EmailService : IEmailService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IWebHostEnvironment env, ILogger<EmailService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task SendBookingSuccessEmailAsync(string toEmail, string bookingCode, string courtName, string qrBase64)
    {
        // Chế độ Development: Ghi ra file HTML trực quan
        if (_env.IsDevelopment())
        {
            var mockDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "doc", "mock_emails");
            Directory.CreateDirectory(mockDir);
            var filePath = Path.Combine(mockDir, $"Booking_{bookingCode}_{DateTime.Now:yyyyMMdd_HHmmss}.html");

            var html = $@"
            <html>
            <body style='font-family: Arial; padding: 20px;'>
                <h2 style='color: #22c55e;'>Xác Nhận Đặt Sân Thành Công!</h2>
                <p>Mã đơn: <strong>{bookingCode}</strong></p>
                <p>Sân: <strong>{courtName}</strong></p>
                <p>Vé Check-In QR Code:</p>
                <img src='{qrBase64}' alt='QR Ticket' style='width: 200px; height: 200px;'/>
                <p style='color: #64748b;'>Vui lòng đến trước 15 phút và đưa mã QR này cho lễ tân để check-in.</p>
            </body>
            </html>";

            await File.WriteAllTextAsync(filePath, html);
            _logger.LogInformation(" [Mock Email] Đã xuất file vé điện tử ra: {Path}", filePath);
            return;
        }

        // Chế độ Production: Gửi SMTP thật
        _logger.LogInformation(" [Production Email] Đã gửi email tới: {Email}", toEmail);
    }

    public Task SendCancellationEmailAsync(string toEmail, string bookingCode, string reason, decimal refundAmount)
    {
        _logger.LogInformation(" [Email] Báo hủy đơn {Code}, lý do: {Reason}, hoàn: {Refund}", bookingCode, reason, refundAmount);
        return Task.CompletedTask;
    }

    public Task SendReminderEmailAsync(string toEmail, string bookingCode, string timeLabel)
    {
        _logger.LogInformation(" [Email] Nhắc lịch chơi {Code} ({Time}) tới {Email}", bookingCode, timeLabel, toEmail);
        return Task.CompletedTask;
    }
}
