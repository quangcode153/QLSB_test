using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UI.Controls;

/// <summary>
/// Nút gạt bật/tắt (Toggle Switch) phong cách Fluent hiện đại cho việc bật tắt chế độ bảo trì sân.
/// </summary>
public class ToggleSwitch : Control
{
    private bool _checked = false;
    private Color _onColor = AppTheme.Primary;
    private Color _offColor = Color.FromArgb(203, 213, 225);

    public event EventHandler? CheckedChanged;

    [Category("Behavior")]
    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked != value)
            {
                _checked = value;
                OnCheckedChanged();
                Invalidate();
            }
        }
    }

    [Category("Appearance")]
    public Color OnColor
    {
        get => _onColor;
        set { _onColor = value; Invalidate(); }
    }

    public ToggleSwitch()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Size = new Size(46, 24);
        Cursor = Cursors.Hand;
    }

    protected virtual void OnCheckedChanged()
    {
        CheckedChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        Checked = !Checked;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        int radius = rect.Height / 2;

        // Vẽ nền pill
        Color trackColor = _checked ? _onColor : _offColor;
        using (var brush = new SolidBrush(trackColor))
        {
            AppTheme.FillRoundedRectangle(g, brush, rect, radius);
        }

        // Vẽ nút tròn (Thumb)
        int thumbSize = rect.Height - 4;
        int thumbX = _checked ? rect.Right - thumbSize - 2 : rect.X + 2;
        int thumbY = rect.Y + 2;

        using (var brush = new SolidBrush(Color.White))
        {
            g.FillEllipse(brush, thumbX, thumbY, thumbSize, thumbSize);
        }
    }
}
