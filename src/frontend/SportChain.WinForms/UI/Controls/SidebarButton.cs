using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UI.Controls;

/// <summary>
/// Nút điều hướng Sidebar hiện đại: Hỗ trợ Icon Vector, chế độ thu gọn (Collapsed 72px) và hiệu ứng Active Teal.
/// </summary>
public class SidebarButton : Control
{
    private IconChar _icon = IconChar.Home;
    private bool _isActive = false;
    private bool _isHovered = false;
    private bool _isCollapsed = false;

    [Category("Appearance")]
    public IconChar Icon
    {
        get => _icon;
        set { _icon = value; Invalidate(); }
    }

    [Category("Behavior")]
    public bool IsActive
    {
        get => _isActive;
        set { _isActive = value; Invalidate(); }
    }

    [Category("Behavior")]
    public bool IsCollapsed
    {
        get => _isCollapsed;
        set { _isCollapsed = value; Invalidate(); }
    }

    public SidebarButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Height = 44;
        Width = 240;
        Font = AppTheme.FontBodyBold;
        Cursor = Cursors.Hand;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _isHovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _isHovered = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        Color bg = _isActive ? AppTheme.SidebarActive : (_isHovered ? AppTheme.SidebarHover : Color.Transparent);
        Color fg = _isActive ? Color.White : (_isHovered ? Color.White : AppTheme.SidebarText);

        // Vẽ nền bo góc nhẹ
        if (bg != Color.Transparent)
        {
            using var brush = new SolidBrush(bg);
            AppTheme.FillRoundedRectangle(g, brush, rect, AppTheme.RadiusControl);
        }

        // Vẽ Icon Vector
        var iconBmp = AppIconHelper.Get(_icon, 18, fg);

        if (_isCollapsed || Width < 100)
        {
            // Chế độ thu gọn 72px: Căn giữa Icon, ẩn Text
            int ix = (Width - iconBmp.Width) / 2;
            int iy = (Height - iconBmp.Height) / 2;
            g.DrawImage(iconBmp, ix, iy);
        }
        else
        {
            // Chế độ mở rộng: Icon bên trái, Text bên phải
            int ix = 16;
            int iy = (Height - iconBmp.Height) / 2;
            g.DrawImage(iconBmp, ix, iy);

            int textX = ix + iconBmp.Width + 12;
            var textRect = new Rectangle(textX, 0, Math.Max(0, Width - textX - 8), Height);
            TextRenderer.DrawText(g, Text, Font, textRect, fg, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.WordEllipsis);
        }
    }
}
