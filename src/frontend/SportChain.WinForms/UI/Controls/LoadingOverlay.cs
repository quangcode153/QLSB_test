using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UI.Controls;

/// <summary>
/// Lớp phủ chờ tải dữ liệu (Loading Overlay) với hiệu ứng spinner xoay tròn và thông báo thân thiện.
/// </summary>
public class LoadingOverlay : Control
{
    private readonly System.Windows.Forms.Timer _timer;
    private float _angle = 0;
    private string _message = "Đang tải dữ liệu...";

    public string Message
    {
        get => _message;
        set { _message = value; Invalidate(); }
    }

    public LoadingOverlay()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Dock = DockStyle.Fill;
        Visible = false;

        _timer = new System.Windows.Forms.Timer { Interval = 35 };
        _timer.Tick += (s, e) =>
        {
            _angle = (_angle + 20) % 360;
            Invalidate();
        };
    }

    public void ShowLoading(string message = "Đang tải dữ liệu...")
    {
        _message = message;
        Visible = true;
        BringToFront();
        _timer.Start();
    }

    public void HideLoading()
    {
        _timer.Stop();
        Visible = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Vẽ lớp phủ làm mờ bán trong suốt (Semi-transparent background)
        using (var brush = new SolidBrush(Color.FromArgb(190, 248, 250, 252)))
        {
            g.FillRectangle(brush, ClientRectangle);
        }

        int spinnerSize = 40;
        int cx = (Width - spinnerSize) / 2;
        int cy = (Height - spinnerSize) / 2 - 15;

        // Vẽ vòng xoay Spinner
        using (var trackPen = new Pen(AppTheme.Border, 3.5f))
        {
            g.DrawEllipse(trackPen, cx, cy, spinnerSize, spinnerSize);
        }

        using (var spinPen = new Pen(AppTheme.Primary, 3.5f))
        {
            spinPen.StartCap = LineCap.Round;
            spinPen.EndCap = LineCap.Round;
            g.DrawArc(spinPen, cx, cy, spinnerSize, spinnerSize, _angle, 100);
        }

        // Vẽ Text thông báo
        var textRect = new Rectangle(0, cy + spinnerSize + 16, Width, 30);
        TextRenderer.DrawText(g, _message, AppTheme.FontCaptionBold, textRect, AppTheme.TextMain, TextFormatFlags.HorizontalCenter);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
