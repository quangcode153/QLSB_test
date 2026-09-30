using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UI.Controls;

public enum AppButtonType
{
    Primary,
    Secondary,
    Danger,
    Ghost,
    Accent
}

/// <summary>
/// Nút bấm hiện đại GDI+ hỗ trợ Icon Vector FontAwesome, trạng thái Hover/Pressed/Loading và bo góc 6px.
/// </summary>
public class AppButton : Button
{
    private AppButtonType _buttonType = AppButtonType.Primary;
    private IconChar _icon = IconChar.None;
    private int _iconSize = 16;
    private bool _isLoading = false;
    private bool _isHovered = false;
    private bool _isPressed = false;
    private float _spinnerAngle = 0;
    private readonly System.Windows.Forms.Timer _spinnerTimer;

    [Category("Appearance")]
    public AppButtonType ButtonType
    {
        get => _buttonType;
        set { _buttonType = value; Invalidate(); }
    }

    [Category("Appearance")]
    public IconChar Icon
    {
        get => _icon;
        set { _icon = value; Invalidate(); }
    }

    [Category("Appearance")]
    public int IconSize
    {
        get => _iconSize;
        set { _iconSize = value; Invalidate(); }
    }

    [Category("Behavior")]
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            Enabled = !_isLoading;
            if (_isLoading)
            {
                _spinnerTimer.Start();
            }
            else
            {
                _spinnerTimer.Stop();
            }
            Invalidate();
        }
    }

    public AppButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Size = new Size(120, 38);
        Font = AppTheme.FontCaptionBold;
        Cursor = Cursors.Hand;

        _spinnerTimer = new System.Windows.Forms.Timer { Interval = 40 };
        _spinnerTimer.Tick += (s, e) =>
        {
            _spinnerAngle = (_spinnerAngle + 30) % 360;
            Invalidate();
        };
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
        _isPressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        base.OnMouseDown(mevent);
        _isPressed = true;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        base.OnMouseUp(mevent);
        _isPressed = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        var g = pevent.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        Color bg;
        Color fg;
        Color border = Color.Transparent;

        if (!Enabled)
        {
            bg = AppTheme.CardBgAlt;
            fg = AppTheme.TextDisabled;
            border = AppTheme.Border;
        }
        else
        {
            switch (_buttonType)
            {
                case AppButtonType.Primary:
                    bg = _isPressed ? AppTheme.PrimaryPressed : (_isHovered ? AppTheme.PrimaryHover : AppTheme.Primary);
                    fg = Color.White;
                    break;
                case AppButtonType.Accent:
                    bg = _isPressed ? AppTheme.AccentHover : (_isHovered ? AppTheme.AccentHover : AppTheme.Accent);
                    fg = Color.White;
                    break;
                case AppButtonType.Danger:
                    bg = _isPressed ? Color.FromArgb(185, 28, 28) : (_isHovered ? Color.FromArgb(220, 38, 38) : AppTheme.Danger);
                    fg = Color.White;
                    break;
                case AppButtonType.Secondary:
                    bg = _isPressed ? Color.FromArgb(226, 232, 240) : (_isHovered ? Color.FromArgb(241, 245, 249) : Color.White);
                    fg = AppTheme.TextMain;
                    border = AppTheme.Border;
                    break;
                case AppButtonType.Ghost:
                default:
                    bg = _isPressed ? Color.FromArgb(226, 232, 240) : (_isHovered ? Color.FromArgb(241, 245, 249) : Color.Transparent);
                    fg = AppTheme.TextMain;
                    break;
            }
        }

        // Vẽ nền bo góc
        using (var brush = new SolidBrush(bg))
        {
            AppTheme.FillRoundedRectangle(g, brush, rect, AppTheme.RadiusControl);
        }

        // Vẽ viền
        if (border != Color.Transparent)
        {
            using var pen = new Pen(border, 1);
            AppTheme.DrawRoundedRectangle(g, pen, rect, AppTheme.RadiusControl);
        }

        // Vẽ Spinner Loading nếu đang bận
        if (_isLoading)
        {
            DrawSpinner(g, fg, rect);
            return;
        }

        // Đo đạc vẽ Icon và Text căn giữa hài hòa
        Bitmap? iconBmp = null;
        if (_icon != IconChar.None)
        {
            iconBmp = AppIconHelper.Get(_icon, _iconSize, fg);
        }

        var textSize = TextRenderer.MeasureText(Text, Font);
        int iconSpacing = (!string.IsNullOrEmpty(Text) && iconBmp != null) ? 6 : 0;
        int totalContentWidth = (iconBmp != null ? iconBmp.Width : 0) + iconSpacing + textSize.Width;
        int startX = Math.Max(4, (Width - totalContentWidth) / 2);

        if (iconBmp != null)
        {
            int iconY = (Height - iconBmp.Height) / 2;
            g.DrawImage(iconBmp, startX, iconY);
            startX += iconBmp.Width + iconSpacing;
        }

        if (!string.IsNullOrEmpty(Text))
        {
            var textRect = new Rectangle(startX, 0, textSize.Width + 6, Height);
            TextRenderer.DrawText(g, Text, Font, textRect, fg, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }

    private void DrawSpinner(Graphics g, Color color, Rectangle rect)
    {
        int spinnerSize = Math.Min(20, rect.Height - 12);
        int cx = rect.X + (rect.Width - spinnerSize) / 2;
        int cy = rect.Y + (rect.Height - spinnerSize) / 2;

        using var pen = new Pen(color, 2.5f);
        pen.StartCap = LineCap.Round;
        pen.EndCap = LineCap.Round;

        var arcRect = new Rectangle(cx, cy, spinnerSize, spinnerSize);
        g.DrawArc(pen, arcRect, _spinnerAngle, 270);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _spinnerTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
