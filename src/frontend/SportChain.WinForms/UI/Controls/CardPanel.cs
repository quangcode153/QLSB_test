using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UI.Controls;

/// <summary>
/// Panel thẻ phẳng bo góc (Card Panel) chuẩn Design System, hỗ trợ viền và bóng đổ nhẹ.
/// </summary>
public class CardPanel : Panel
{
    private int _cornerRadius = AppTheme.RadiusCard;
    private Color _borderColor = AppTheme.Border;
    private int _borderWidth = 1;

    [Category("Appearance")]
    public int CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = value; Invalidate(); }
    }

    [Category("Appearance")]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    [Category("Appearance")]
    public int BorderWidth
    {
        get => _borderWidth;
        set { _borderWidth = value; Invalidate(); }
    }

    public CardPanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = AppTheme.CardBg;
        Padding = new Padding(16);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        // Vẽ nền thẻ
        using (var brush = new SolidBrush(BackColor))
        {
            AppTheme.FillRoundedRectangle(g, brush, rect, _cornerRadius);
        }

        // Vẽ viền thẻ
        if (_borderWidth > 0 && _borderColor != Color.Transparent)
        {
            using var pen = new Pen(_borderColor, _borderWidth);
            AppTheme.DrawRoundedRectangle(g, pen, rect, _cornerRadius);
        }
    }
}
