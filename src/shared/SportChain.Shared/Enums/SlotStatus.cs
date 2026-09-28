namespace SportChain.Shared.Enums;

public enum SlotStatus
{
    Available = 0,    // Sân trống, khách có thể đặt ngay (Màu xanh #22c55e)
    Holding = 1,      // Đang giữ chỗ cọc 15 phút (Màu vàng #eab308)
    Booked = 2,       // Đã đặt cọc và xác nhận (Màu đỏ #ef4444)
    InUse = 3,        // Đang sử dụng, khách đã check-in QR (Màu xanh dương #3b82f6)
    Maintenance = 4   // Sân bảo trì / tạm dừng (Màu xám #64748b)
}
