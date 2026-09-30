using System.Drawing;
using FontAwesome.Sharp;

namespace SportChain.WinForms.UI.Theme;

/// <summary>
/// Trình trợ giúp sinh và quản lý Icon Vector FontAwesome độ nét cao, loại bỏ hoàn toàn Emoji.
/// </summary>
public static class AppIconHelper
{
    private static readonly Dictionary<string, Bitmap> _iconCache = new();

    /// <summary>
    /// Lấy ảnh Bitmap của Icon Vector theo kích thước và màu sắc chỉ định.
    /// </summary>
    public static Bitmap Get(IconChar icon, int size = 18, Color? color = null)
    {
        var targetColor = color ?? AppTheme.TextMain;
        var cacheKey = $"{icon}_{size}_{targetColor.ToArgb()}";

        if (_iconCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var bmp = icon.ToBitmap(targetColor, size);
        _iconCache[cacheKey] = bmp;
        return bmp;
    }

    /// <summary>
    /// Vẽ trực tiếp Icon Vector lên Graphics canvas bằng GDI+.
    /// </summary>
    public static void Draw(Graphics g, IconChar icon, Rectangle rect, Color color, int size = 18)
    {
        var bmp = Get(icon, size, color);
        int x = rect.X + (rect.Width - bmp.Width) / 2;
        int y = rect.Y + (rect.Height - bmp.Height) / 2;
        g.DrawImage(bmp, x, y);
    }
}
