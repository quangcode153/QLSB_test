using SportChain.Shared.Enums;

namespace SportChain.Shared.Constants;

public static class SystemPolicies
{
    public const int DefaultDepositHoldingMinutes = 15;     // 15 phút giữ chỗ cọc
    public const int DefaultCheckInGraceMinutes = 15;       // 15 phút chờ check-in no-show
    public const int DefaultSlotDurationMinutes = 60;       // Mỗi ca chuẩn đúng 60 phút
    public const decimal DefaultDepositPercentage = 0.30m;   // Mặc định cọc 30%

    /// <summary>
    /// Bảng giá chuẩn theo Rule BR-PRICE-01 khi không có cấu hình PriceRule riêng
    /// </summary>
    public static decimal GetStandardPrice(SportType sport, bool isPeak) => sport switch
    {
        SportType.Badminton => isPeak ? 160_000m : 100_000m,
        SportType.Pickleball => isPeak ? 180_000m : 120_000m,
        SportType.Tennis => isPeak ? 300_000m : 200_000m,
        SportType.Football => isPeak ? 450_000m : 300_000m,
        _ => isPeak ? 200_000m : 150_000m
    };
}

