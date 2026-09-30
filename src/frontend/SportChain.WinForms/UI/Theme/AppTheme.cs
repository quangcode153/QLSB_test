using System.Drawing;
using System.Drawing.Drawing2D;

namespace SportChain.WinForms.UI.Theme;

/// <summary>
/// Hệ thống Design Tokens trung tâm chuẩn hóa cho toàn bộ giao diện SportChain WinForms.
/// </summary>
public static class AppTheme
{
    // ==========================================
    // 1. MÀU SẮC CHỦ ĐẠO & THƯƠNG HIỆU (BRAND COLORS)
    // ==========================================
    public static readonly Color Primary = Color.FromArgb(13, 148, 136);          // Teal 600 (#0D9488)
    public static readonly Color PrimaryHover = Color.FromArgb(15, 118, 110);     // Teal 700 (#0F766E)
    public static readonly Color PrimaryPressed = Color.FromArgb(17, 94, 89);      // Teal 800 (#115E59)
    public static readonly Color PrimaryLight = Color.FromArgb(204, 251, 241);     // Teal 100 (#CCFBF1)
    public static readonly Color PrimarySubtle = Color.FromArgb(240, 253, 250);    // Teal 50 (#F0FDFA)

    public static readonly Color Accent = Color.FromArgb(16, 185, 129);           // Emerald 500 (#10B981)
    public static readonly Color AccentHover = Color.FromArgb(5, 150, 105);        // Emerald 600 (#059669)
    public static readonly Color AccentLight = Color.FromArgb(209, 250, 229);      // Emerald 100 (#D1FAE5)

    // ==========================================
    // 2. MÀU NỀN & KHỐI GIAO DIỆN (SURFACES & BACKGROUNDS)
    // ==========================================
    public static readonly Color MainBg = Color.FromArgb(248, 250, 252);          // Slate 50 (#F8FAFC)
    public static readonly Color CardBg = Color.White;                             // Pure White (#FFFFFF)
    public static readonly Color CardBgAlt = Color.FromArgb(241, 245, 249);       // Slate 100 (#F1F5F9)

    public static readonly Color Border = Color.FromArgb(226, 232, 240);          // Slate 200 (#E2E8F0)
    public static readonly Color BorderDark = Color.FromArgb(203, 213, 225);      // Slate 300 (#CBD5E1)
    public static readonly Color BorderFocus = Color.FromArgb(13, 148, 136);      // Teal 600

    // ==========================================
    // 3. SIDEBAR & TOPBAR DESIGN TOKENS
    // ==========================================
    public static readonly Color SidebarBg = Color.FromArgb(15, 23, 42);          // Slate 900 (#0F172A)
    public static readonly Color SidebarDarker = Color.FromArgb(2, 6, 23);        // Slate 950 (#020617)
    public static readonly Color SidebarHover = Color.FromArgb(30, 41, 59);        // Slate 800 (#1E293B)
    public static readonly Color SidebarActive = Color.FromArgb(13, 148, 136);     // Teal 600 (#0D9488)
    public static readonly Color SidebarText = Color.FromArgb(203, 213, 225);      // Slate 300
    public static readonly Color SidebarTextMuted = Color.FromArgb(100, 116, 139); // Slate 500

    public static readonly Color TopbarBg = Color.White;                           // Nền trắng chuẩn mực
    public static readonly Color TopbarBorder = Color.FromArgb(226, 232, 240);     // 1px viền dưới #E2E8F0

    // ==========================================
    // 4. MÀU NGỮ NGHĨA TRẠNG THÁI (SEMANTIC STATUS COLORS)
    // ==========================================
    public static readonly Color Success = Color.FromArgb(16, 185, 129);          // Emerald 500 (#10B981)
    public static readonly Color SuccessLight = Color.FromArgb(236, 253, 245);     // Emerald 50 (#ECFDF5)
    public static readonly Color SuccessBorder = Color.FromArgb(167, 243, 208);    // Emerald 200

    public static readonly Color Warning = Color.FromArgb(245, 158, 11);           // Amber 500 (#F59E0B)
    public static readonly Color WarningLight = Color.FromArgb(254, 243, 199);     // Amber 100 (#FEF3C7)
    public static readonly Color WarningBorder = Color.FromArgb(252, 211, 77);     // Amber 300

    public static readonly Color Danger = Color.FromArgb(239, 68, 68);             // Red 500 (#EF4444)
    public static readonly Color DangerLight = Color.FromArgb(254, 242, 242);      // Red 50 (#FEF2F2)
    public static readonly Color DangerBorder = Color.FromArgb(254, 202, 202);     // Red 200

    public static readonly Color Info = Color.FromArgb(2, 132, 199);               // Sky 600 (#0284C7)
    public static readonly Color InfoLight = Color.FromArgb(224, 242, 254);        // Sky 100 (#E0F2FE)
    public static readonly Color InfoBorder = Color.FromArgb(186, 230, 253);       // Sky 200

    public static readonly Color InUse = Color.FromArgb(147, 51, 234);            // Purple 600 (#9333EA)
    public static readonly Color InUseLight = Color.FromArgb(243, 232, 255);       // Purple 100 (#F3E8FF)
    public static readonly Color InUseBorder = Color.FromArgb(216, 180, 254);      // Purple 300

    // ==========================================
    // 5. MÀU VĂN BẢN (TYPOGRAPHY COLORS)
    // ==========================================
    public static readonly Color TextMain = Color.FromArgb(15, 23, 42);           // Slate 900 (#0F172A)
    public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);       // Slate 500 (#64748B)
    public static readonly Color TextSubtle = Color.FromArgb(148, 163, 184);      // Slate 400 (#94A3B8)
    public static readonly Color TextWhite = Color.White;
    public static readonly Color TextDisabled = Color.FromArgb(148, 163, 184);

    // ==========================================
    // 6. THANG FONT CHỮ CHUẨN (TYPOGRAPHIC SCALE - SEGOE UI)
    // ==========================================
    public static readonly Font FontScreenTitle = new("Segoe UI", 16F, FontStyle.Bold);
    public static readonly Font FontCardTitle = new("Segoe UI", 12F, FontStyle.Bold);
    public static readonly Font FontSection = new("Segoe UI", 11F, FontStyle.Bold);
    public static readonly Font FontBody = new("Segoe UI", 10F, FontStyle.Regular);
    public static readonly Font FontBodyBold = new("Segoe UI", 10F, FontStyle.Bold);
    public static readonly Font FontCaption = new("Segoe UI", 9F, FontStyle.Regular);
    public static readonly Font FontCaptionBold = new("Segoe UI", 9F, FontStyle.Bold);
    public static readonly Font FontSmall = new("Segoe UI", 8F, FontStyle.Regular);
    public static readonly Font FontSmallBold = new("Segoe UI", 8F, FontStyle.Bold);
    public static readonly Font FontKpiValue = new("Segoe UI", 20F, FontStyle.Bold);

    // ==========================================
    // 7. QUY CHUẨN KHOẢNG CÁCH (SPACING TOKENS - 4/8px)
    // ==========================================
    public const int Space2 = 2;
    public const int Space4 = 4;
    public const int Space8 = 8;
    public const int Space12 = 12;
    public const int Space16 = 16;
    public const int Space20 = 20;
    public const int Space24 = 24;
    public const int Space32 = 32;

    // ==========================================
    // 8. QUY CHUẨN BO GÓC (CORNER RADIUS)
    // ==========================================
    public const int RadiusCard = 8;
    public const int RadiusControl = 6;
    public const int RadiusPill = 999;

    // ==========================================
    // 9. CÁC HÀM TIỆN ÍCH ĐỒ HỌA GDI+ (GDI+ DRAWING HELPERS)
    // ==========================================
    public static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        if (radius <= 0)
        {
            path.AddRectangle(rect);
            return path;
        }

        int d = radius * 2;
        if (d > rect.Width) d = rect.Width;
        if (d > rect.Height) d = rect.Height;

        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void FillRoundedRectangle(Graphics g, Brush brush, Rectangle rect, int radius)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = CreateRoundedPath(rect, radius);
        g.FillPath(brush, path);
    }

    public static void DrawRoundedRectangle(Graphics g, Pen pen, Rectangle rect, int radius)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = CreateRoundedPath(rect, radius);
        g.DrawPath(pen, path);
    }
}
