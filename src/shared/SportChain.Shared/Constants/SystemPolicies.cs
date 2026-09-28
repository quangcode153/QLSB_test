namespace SportChain.Shared.Constants;

public static class SystemPolicies
{
    public const int DefaultDepositHoldingMinutes = 15;     // 15 phút giữ chỗ cọc
    public const int DefaultCheckInGraceMinutes = 15;       // 15 phút chờ check-in no-show
    public const int DefaultSlotDurationMinutes = 60;       // Mỗi ca chuẩn đúng 60 phút
    public const decimal DefaultDepositPercentage = 0.30m;   // Mặc định cọc 30%
}
