using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UI.Controls;

/// <summary>
/// Ô nhập liệu hiện đại chuẩn Design System: có Icon trái, đổi màu viền khi focus, nút toggle mật khẩu.
/// Kiến trúc đơn tầng (Single-level Flat Canvas) bảo đảm hiển thị pixel-perfect, không giật lag, không lệch icon hay trôi viền.
/// </summary>
public class AppTextBox : UserControl
{
    private readonly TextBox _innerBox = new();
    private readonly IconButton _btnToggleEye = new();

    private IconChar _leftIcon = IconChar.None;
    private bool _isFocused = false;
    private bool _isAdjustingLayout = false;
    private string _errorMessage = string.Empty;

    [Category("Appearance")]
    public string Title { get; set; } = string.Empty;

    [Category("Appearance")]
    public IconChar LeftIcon
    {
        get => _leftIcon;
        set
        {
            _leftIcon = value;
            AdjustLayout();
            Invalidate();
        }
    }

    [Category("Appearance")]
    public string PlaceholderText
    {
        get => _innerBox.PlaceholderText;
        set => _innerBox.PlaceholderText = value;
    }

    [Category("Behavior")]
    public bool UseSystemPasswordChar
    {
        get => _innerBox.UseSystemPasswordChar;
        set
        {
            _innerBox.UseSystemPasswordChar = value;
            _btnToggleEye.Visible = value;
            AdjustLayout();
        }
    }

    [Category("Appearance")]
    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string Text
    {
        get => _innerBox.Text;
        set => _innerBox.Text = value ?? string.Empty;
    }

    [Category("Behavior")]
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            Invalidate();
        }
    }

    public TextBox InnerTextBox => _innerBox;

    public AppTextBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent;

        // 1. TextBox bên trong
        _innerBox.BorderStyle = BorderStyle.None;
        _innerBox.Font = AppTheme.FontBody;
        _innerBox.ForeColor = AppTheme.TextMain;
        _innerBox.BackColor = AppTheme.CardBg;
        _innerBox.Enter += (s, e) => { _isFocused = true; Invalidate(); };
        _innerBox.Leave += (s, e) => { _isFocused = false; Invalidate(); };
        _innerBox.TextChanged += (s, e) => OnTextChanged(e);

        // 2. Nút bật/tắt hiển thị mật khẩu
        _btnToggleEye.IconChar = IconChar.EyeSlash;
        _btnToggleEye.IconSize = 16;
        _btnToggleEye.IconColor = AppTheme.TextSubtle;
        _btnToggleEye.FlatStyle = FlatStyle.Flat;
        _btnToggleEye.Size = new Size(28, 26);
        _btnToggleEye.Cursor = Cursors.Hand;
        _btnToggleEye.Visible = false;
        _btnToggleEye.FlatAppearance.BorderSize = 0;
        _btnToggleEye.BackColor = Color.Transparent;
        _btnToggleEye.Click += (s, e) =>
        {
            _innerBox.UseSystemPasswordChar = !_innerBox.UseSystemPasswordChar;
            _btnToggleEye.IconChar = _innerBox.UseSystemPasswordChar ? IconChar.EyeSlash : IconChar.Eye;
            _btnToggleEye.IconColor = _innerBox.UseSystemPasswordChar ? AppTheme.TextSubtle : AppTheme.Primary;
        };

        // Chuyển focus vào TextBox khi nhấn bất kỳ đâu trên ô nhập
        Cursor = Cursors.IBeam;
        Click += (s, e) => _innerBox.Focus();

        Controls.Add(_innerBox);
        Controls.Add(_btnToggleEye);

        Width = 260;
        Height = 40;

        AdjustLayout();
    }

    private void AdjustLayout()
    {
        if (_isAdjustingLayout || _innerBox == null || _btnToggleEye == null)
        {
            return;
        }

        _isAdjustingLayout = true;
        try
        {
            int boxLeft = _leftIcon != IconChar.None ? 36 : 14;
            int boxRightPad = _btnToggleEye.Visible ? 36 : 14;

            int boxY = Math.Max(0, (Height - _innerBox.Height) / 2);

            _innerBox.Location = new Point(boxLeft, boxY);
            _innerBox.Width = Math.Max(20, Width - boxLeft - boxRightPad);

            _btnToggleEye.Location = new Point(Width - 32, (Height - _btnToggleEye.Height) / 2);
        }
        finally
        {
            _isAdjustingLayout = false;
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        AdjustLayout();
        Invalidate();
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        AdjustLayout();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var boxRect = new Rectangle(0, 0, Width - 1, Height - 1);

        // Nền ô nhập
        using (var brush = new SolidBrush(AppTheme.CardBg))
        {
            AppTheme.FillRoundedRectangle(g, brush, boxRect, AppTheme.RadiusControl);
        }

        // Viền đổi màu theo trạng thái (Lỗi -> Focus -> Bình thường)
        Color borderColor = !string.IsNullOrEmpty(_errorMessage)
            ? AppTheme.Danger
            : (_isFocused ? AppTheme.BorderFocus : AppTheme.Border);

        using (var pen = new Pen(borderColor, _isFocused ? 1.5f : 1f))
        {
            AppTheme.DrawRoundedRectangle(g, pen, boxRect, AppTheme.RadiusControl);
        }

        // Icon trái vẽ căn giữa theo chiều dọc
        if (_leftIcon != IconChar.None)
        {
            Color iconColor = _isFocused ? AppTheme.Primary : AppTheme.TextSubtle;
            var iconBmp = AppIconHelper.Get(_leftIcon, 16, iconColor);
            int iconY = (Height - iconBmp.Height) / 2;
            g.DrawImage(iconBmp, 12, iconY);
        }
    }

    public new bool Focus()
    {
        return _innerBox.Focus();
    }
}
