using SportChain.WebApi.Services;

namespace SportChain.WebApi.Workers;

public class BookingWatcherWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingWatcherWorker> _logger;

    public BookingWatcherWorker(IServiceProvider serviceProvider, ILogger<BookingWatcherWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 [Worker] BookingWatcherWorker đã khởi động (Quét định kỳ mỗi 60 giây).");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var bookingService = scope.ServiceProvider.GetService<IBookingService>();
                if (bookingService != null)
                {
                    // 1. Quét tự động hủy đơn quá 15 phút chưa đặt cọc
                    await bookingService.AutoReleaseExpiredHoldingsAsync();

                    // 2. Quét tự động hủy đơn No-show quá 15 phút sau giờ bắt đầu ca
                    await bookingService.AutoMarkNoShowsAsync();
                }
            }
            catch (OperationCanceledException)
            {
                // Máy chủ đang dừng hoạt động, thoát vòng lặp êm dịu
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [Worker] Có lỗi xảy ra trong tiến trình kiểm tra đơn đặt sân.");
            }

            try
            {
                // Chờ 60 giây cho chu kỳ quét tiếp theo
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("🛑 [Worker] BookingWatcherWorker đã dừng an toàn.");
    }
}
