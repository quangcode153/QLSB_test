using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UI.Controls;

/// <summary>
/// Thẻ thống kê chỉ số KPI hiện đại: gồm Icon nổi bật, số liệu lớn, nhãn mô tả và tỷ lệ tăng trưởng so với kỳ trước.
/// </summary>
public class KpiCard : Control
{
    private string _title = "TIÊU ĐỀ CHỈ SỐ";
    private string _value = "0";
    private string _changeText = "+0.0%";
    private bool _isPositive = true;
    private IconChar _icon = IconChar.ChartLine;
    private Color _accentColor = AppTheme.Primary;

    [Category("Appearance")]
    public string Title
    {
        get => _title;
        set { _title = value; Invalidate(); }
    }

    [Category("Appearance")]
    public string Value
    {
        get => _value;
        set { _value = value; Invalidate(); }
    }

    [Category("Appearance")]
    public string ChangeText
    {
        get => _changeText;
        set { _changeText = value; Invalidate(); }
    }

    [Category("Appearance")]
    public bool IsPositive
    {
        get => _isPositive;
        set { _isPositive = value; Invalidate(); }
    }

    [Category("Appearance")]
    public IconChar Icon
    {
        get => _icon;
        set { _icon = value; Invalidate(); }
    }

    [Category("Appearance")]
    public Color AccentColor
    {
        get => _accentColor;
        set { _accentColor = value; Invalidate(); }
    }

    public KpiCard()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Size = new Size(240, 110);
        BackColor = AppTheme.CardBg;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        // Vẽ nền thẻ Card
        using (var brush = new SolidBrush(AppTheme.CardBg))
        {
            AppTheme.FillRoundedRectangle(g, brush, rect, AppTheme.RadiusCard);
        }

        // Vẽ viền thẻ
        using (var pen = new Pen(AppTheme.Border, 1))
        {
            AppTheme.DrawRoundedRectangle(g, pen, rect, AppTheme.RadiusCard);
        }

        // Vẽ hộp Icon nổi bật phía trên bên phải
        int iconBoxSize = 44;
        var iconBoxRect = new Rectangle(Width - iconBoxSize - 16, 16, iconBoxSize, iconBoxSize);
        var iconBg = Color.FromArgb(30, _accentColor.R, _accentColor.G, _accentColor.B);

        using (var brush = new SolidBrush(iconBg))
        {
            AppTheme.FillRoundedRectangle(g, brush, iconBoxRect, AppTheme.RadiusControl);
        }

        var iconBmp = AppIconHelper.Get(_icon, 20, _accentColor);
        int ix = iconBoxRect.X + (iconBoxSize - iconBmp.Width) / 2;
        int iy = iconBoxRect.Y + (iconBoxSize - iconBmp.Height) / 2;
        g.DrawImage(iconBmp, ix, iy);

        // Vẽ Tiêu đề (Subtitle)
        TextRenderer.DrawText(g, _title.ToUpper(), AppTheme.FontSmallBold, new Point(16, 16), AppTheme.TextMuted);

        // Vẽ Giá trị KPI lớn
        TextRenderer.DrawText(g, _value, AppTheme.FontKpiValue, new Point(14, 38), AppTheme.TextMain);

        // Vẽ Tỷ lệ tăng trưởng %
        if (!string.IsNullOrEmpty(_changeText))
        {
            Color trendColor = _isPositive ? AppTheme.Success : AppTheme.Danger;
            var trendIcon = _isPositive ? IconChar.ArrowTrendUp : IconChar.ArrowTrendDown;
            var trendBmp = AppIconHelper.Get(trendIcon, 12, trendColor);

            int by = Height - 24;
            g.DrawImage(trendBmp, 16, by + 1);

            TextRenderer.DrawText(g, _changeText, AppTheme.FontSmallBold, new Point(16 + trendBmp.Width + 4, by), trendColor);
        }
    }
}
