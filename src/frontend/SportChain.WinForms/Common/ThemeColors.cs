using System.Drawing;

namespace SportChain.WinForms.Common;

public static class ThemeColors
{
    // Màu chủ đạo (Brand Accent - Teal)
    public static readonly Color Primary = Color.FromArgb(13, 148, 136);       // #0d9488
    public static readonly Color PrimaryDark = Color.FromArgb(15, 118, 110);    // #0f766e
    public static readonly Color PrimaryLight = Color.FromArgb(204, 251, 241);  // #ccfbf1

    // Sidebar & Dark Theme
    public static readonly Color SidebarBg = Color.FromArgb(15, 23, 42);       // #0f172a
    public static readonly Color SidebarDarker = Color.FromArgb(2, 6, 23);     // #020617
    public static readonly Color SidebarHover = Color.FromArgb(30, 41, 59);     // #1e293b
    public static readonly Color SidebarActive = Color.FromArgb(13, 148, 136);  // #0d9488

    // Nền & Khối nội dung
    public static readonly Color ContentBg = Color.FromArgb(248, 250, 252);     // #f8fafc
    public static readonly Color CardBg = Color.White;
    public static readonly Color CardBorder = Color.FromArgb(226, 232, 240);    // #e2e8f0

    // Typography
    public static readonly Color TextDark = Color.FromArgb(15, 23, 42);         // #0f172a
    public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);     // #64748b
    public static readonly Color TextLight = Color.FromArgb(241, 245, 249);     // #f1f5f9

    // Trạng thái các ô ca giờ (Slot Matrix States)
    // 1. Trống (Available - Xanh lá nhạt)
    public static readonly Color SlotAvailableBg = Color.FromArgb(236, 253, 245);       // #ecfdf5
    public static readonly Color SlotAvailableBorder = Color.FromArgb(167, 243, 208);   // #a7f3d0
    public static readonly Color SlotAvailableText = Color.FromArgb(6, 95, 70);         // #065f46

    // 2. Giữ chỗ 15p (Holding - Vàng nổi bật)
    public static readonly Color SlotHoldingBg = Color.FromArgb(254, 243, 199);         // #fef3c7
    public static readonly Color SlotHoldingBorder = Color.FromArgb(245, 158, 11);      // #f59e0b
    public static readonly Color SlotHoldingText = Color.FromArgb(146, 64, 14);         // #92400e

    // 3. Chính khách hàng đang giữ (My Holding - Vàng cam viền đậm)
    public static readonly Color SlotMyHoldingBg = Color.FromArgb(255, 237, 213);       // #ffedd5
    public static readonly Color SlotMyHoldingBorder = Color.FromArgb(249, 115, 22);    // #f97316
    public static readonly Color SlotMyHoldingText = Color.FromArgb(154, 52, 18);       // #9a3412

    // 4. Đang chọn (Selected - Xanh ngọc nổi bật)
    public static readonly Color SlotSelectedBg = Color.FromArgb(16, 185, 129);         // #10b981
    public static readonly Color SlotSelectedBorder = Color.FromArgb(5, 150, 105);      // #059669
    public static readonly Color SlotSelectedText = Color.White;

    // 5. Đã cọc/Đã đặt (Confirmed - Xanh dương)
    public static readonly Color SlotBookedBg = Color.FromArgb(224, 242, 254);          // #e0f2fe
    public static readonly Color SlotBookedBorder = Color.FromArgb(2, 132, 199);        // #0284c7
    public static readonly Color SlotBookedText = Color.FromArgb(3, 105, 161);          // #0369a1

    // 6. Đang chơi (InUse - Tím)
    public static readonly Color SlotInUseBg = Color.FromArgb(237, 233, 254);           // #ede9fe
    public static readonly Color SlotInUseBorder = Color.FromArgb(147, 51, 234);        // #9333ea
    public static readonly Color SlotInUseText = Color.FromArgb(109, 40, 217);          // #6d28d9

    // 7. Bảo trì (Maintenance - Xám)
    public static readonly Color SlotMaintenanceBg = Color.FromArgb(241, 245, 249);     // #f1f5f9
    public static readonly Color SlotMaintenanceBorder = Color.FromArgb(203, 213, 225); // #cbd5e1
    public static readonly Color SlotMaintenanceText = Color.FromArgb(148, 163, 184);   // #94a3b8

    // Alert & Badges
    public static readonly Color Danger = Color.FromArgb(239, 68, 68);                  // #ef4444
    public static readonly Color Warning = Color.FromArgb(245, 158, 11);                // #f59e0b
    public static readonly Color Success = Color.FromArgb(34, 197, 94);                 // #22c55e
    public static readonly Color Info = Color.FromArgb(14, 165, 233);                   // #0ea5e9
}
