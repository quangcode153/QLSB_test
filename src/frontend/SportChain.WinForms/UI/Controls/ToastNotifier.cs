using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UI.Controls;

public enum ToastType
{
    Success,
    Warning,
    Danger,
    Info
}

/// <summary>
/// Khung thông báo Toast nổi ở góc phải màn hình, tự động biến mất sau 3.5 giây, thay thế MessageBox gây khó chịu.
/// </summary>
public class ToastNotifier : Control
{
    private readonly ToastType _type;
    private readonly string _title;
    private readonly string _message;
    private readonly System.Windows.Forms.Timer _dismissTimer;

    public ToastNotifier(string message, ToastType type = ToastType.Success, string? title = null)
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        _type = type;
        _message = message;
        _title = title ?? type switch
        {
            ToastType.Success => "Thành công",
            ToastType.Warning => "Cảnh báo",
            ToastType.Danger => "Lỗi",
            _ => "Thông báo"
        };

        Size = new Size(320, 68);
        Font = AppTheme.FontCaption;
        Cursor = Cursors.Hand;

        _dismissTimer = new System.Windows.Forms.Timer { Interval = 3500 };
        _dismissTimer.Tick += (s, e) =>
        {
            _dismissTimer.Stop();
            Parent?.Controls.Remove(this);
            Dispose();
        };
        _dismissTimer.Start();

        Click += (s, e) =>
        {
            _dismissTimer.Stop();
            Parent?.Controls.Remove(this);
            Dispose();
        };
    }

    public static void Show(Control parent, string message, ToastType type = ToastType.Success, string? title = null)
    {
        if (parent == null) return;
        var form = parent.FindForm() ?? parent;

        var toast = new ToastNotifier(message, type, title);
        int margin = 20;
        toast.Location = new Point(form.ClientSize.Width - toast.Width - margin, form.ClientSize.Height - toast.Height - margin);
        toast.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

        form.Controls.Add(toast);
        toast.BringToFront();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        Color border = _type switch
        {
            ToastType.Success => AppTheme.SuccessBorder,
            ToastType.Warning => AppTheme.WarningBorder,
            ToastType.Danger => AppTheme.DangerBorder,
            _ => AppTheme.InfoBorder
        };

        Color accent = _type switch
        {
            ToastType.Success => AppTheme.Success,
            ToastType.Warning => AppTheme.Warning,
            ToastType.Danger => AppTheme.Danger,
            _ => AppTheme.Info
        };

        IconChar icon = _type switch
        {
            ToastType.Success => IconChar.CheckCircle,
            ToastType.Warning => IconChar.ExclamationTriangle,
            ToastType.Danger => IconChar.TimesCircle,
            _ => IconChar.InfoCircle
        };

        // Vẽ nền trắng
        using (var brush = new SolidBrush(AppTheme.CardBg))
        {
            AppTheme.FillRoundedRectangle(g, brush, rect, AppTheme.RadiusCard);
        }

        // Vẽ viền
        using (var pen = new Pen(border, 1.2f))
        {
            AppTheme.DrawRoundedRectangle(g, pen, rect, AppTheme.RadiusCard);
        }

        // Vẽ dải màu bên trái
        using (var brush = new SolidBrush(accent))
        {
            var barRect = new Rectangle(0, 0, 5, Height);
            g.FillRectangle(brush, barRect);
        }

        // Vẽ Icon Vector
        var iconBmp = AppIconHelper.Get(icon, 20, accent);
        g.DrawImage(iconBmp, 16, (Height - iconBmp.Height) / 2);

        // Vẽ Title & Message
        int textLeft = 16 + iconBmp.Width + 12;
        TextRenderer.DrawText(g, _title, AppTheme.FontCaptionBold, new Point(textLeft, 14), AppTheme.TextMain);
        TextRenderer.DrawText(g, _message, AppTheme.FontSmall, new Rectangle(textLeft, 34, Width - textLeft - 16, 26), AppTheme.TextMuted, TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dismissTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
