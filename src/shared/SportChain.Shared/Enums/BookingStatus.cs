namespace SportChain.Shared.Enums;

public enum BookingStatus
{
    PendingPayment = 0, // Chờ thanh toán cọc trong 15 phút
    Confirmed = 1,      // Đã cọc thành công, có mã QR vé
    InUse = 2,          // Lễ tân đã quét QR check-in
    Completed = 3,      // Đã chơi xong & thanh toán hoàn tất
    Cancelled = 4,      // Đã hủy (quá 15p cọc / khách hủy / quản lý hủy)
    NoShow = 5,         // Vắng mặt quá 15p sau giờ bắt đầu ca
    Refunded = 6        // Đã hoàn cọc theo chính sách
}
