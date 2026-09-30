using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UI.Controls;

public enum BadgeType
{
    Success,
    Warning,
    Danger,
    Info,
    InUse,
    Neutral
}

/// <summary>
/// Thẻ trạng thái hình viên thuốc (Pill Badge) bo tròn, kết hợp Icon Vector và chữ đảm bảo người mù màu vẫn nhận diện tốt.
/// </summary>
public class StatusBadge : Control
{
    private BadgeType _badgeType = BadgeType.Neutral;
    private IconChar _icon = IconChar.InfoCircle;
    private int _iconSize = 13;

    [Category("Appearance")]
    public BadgeType Type
    {
        get => _badgeType;
        set
        {
            _badgeType = value;
            ApplyDefaultIcon();
            Invalidate();
        }
    }

    [Category("Appearance")]
    public IconChar Icon
    {
        get => _icon;
        set { _icon = value; Invalidate(); }
    }

    public StatusBadge()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Font = AppTheme.FontSmallBold;
        Size = new Size(110, 26);
        Text = "Trạng thái";
        ApplyDefaultIcon();
    }

    private void ApplyDefaultIcon()
    {
        _icon = _badgeType switch
        {
            BadgeType.Success => IconChar.CheckCircle,
            BadgeType.Warning => IconChar.Clock,
            BadgeType.Danger => IconChar.TimesCircle,
            BadgeType.InUse => IconChar.PlayCircle,
            BadgeType.Info => IconChar.InfoCircle,
            _ => IconChar.Circle
        };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Color bg;
        Color fg;
        Color border;

        switch (_badgeType)
        {
            case BadgeType.Success:
                bg = AppTheme.SuccessLight;
                fg = Color.FromArgb(6, 95, 70);
                border = AppTheme.SuccessBorder;
                break;
            case BadgeType.Warning:
                bg = AppTheme.WarningLight;
                fg = Color.FromArgb(146, 64, 14);
                border = AppTheme.WarningBorder;
                break;
            case BadgeType.Danger:
                bg = AppTheme.DangerLight;
                fg = Color.FromArgb(185, 28, 28);
                border = AppTheme.DangerBorder;
                break;
            case BadgeType.InUse:
                bg = AppTheme.InUseLight;
                fg = Color.FromArgb(107, 33, 168);
                border = AppTheme.InUseBorder;
                break;
            case BadgeType.Info:
                bg = AppTheme.InfoLight;
                fg = Color.FromArgb(3, 105, 161);
                border = AppTheme.InfoBorder;
                break;
            case BadgeType.Neutral:
            default:
                bg = AppTheme.CardBgAlt;
                fg = AppTheme.TextMuted;
                border = AppTheme.Border;
                break;
        }

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        int pillRadius = rect.Height / 2;

        // Vẽ nền pill
        using (var brush = new SolidBrush(bg))
        {
            AppTheme.FillRoundedRectangle(g, brush, rect, pillRadius);
        }

        // Vẽ viền pill
        using (var pen = new Pen(border, 1))
        {
            AppTheme.DrawRoundedRectangle(g, pen, rect, pillRadius);
        }

        // Vẽ Icon
        var iconBmp = AppIconHelper.Get(_icon, _iconSize, fg);
        int iconX = 8;
        int iconY = (Height - iconBmp.Height) / 2;
        g.DrawImage(iconBmp, iconX, iconY);

        // Vẽ Text
        var textRect = new Rectangle(iconX + iconBmp.Width + 4, 0, Width - (iconX + iconBmp.Width + 10), Height);
        TextRenderer.DrawText(g, Text, Font, textRect, fg, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}
