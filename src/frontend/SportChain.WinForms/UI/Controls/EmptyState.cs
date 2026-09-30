using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UI.Controls;

/// <summary>
/// Giao diện trạng thái rỗng (Empty State) hiển thị khi danh sách hoặc bảng không có dữ liệu.
/// </summary>
public class EmptyState : UserControl
{
    private readonly IconPictureBox _picIcon = new();
    private readonly Label _lblTitle = new();
    private readonly Label _lblDescription = new();
    private readonly AppButton _btnAction = new();
    private bool _isAdjustingLayout = false;

    [Category("Appearance")]
    public IconChar Icon
    {
        get => _picIcon.IconChar;
        set => _picIcon.IconChar = value;
    }

    [Category("Appearance")]
    public string Title
    {
        get => _lblTitle.Text;
        set => _lblTitle.Text = value;
    }

    [Category("Appearance")]
    public string Description
    {
        get => _lblDescription.Text;
        set => _lblDescription.Text = value;
    }

    [Category("Appearance")]
    public string ActionButtonText
    {
        get => _btnAction.Text;
        set
        {
            _btnAction.Text = value;
            _btnAction.Visible = !string.IsNullOrEmpty(value);
            AdjustLayout();
        }
    }

    public event EventHandler? ActionClick
    {
        add => _btnAction.Click += value;
        remove => _btnAction.Click -= value;
    }

    public EmptyState()
    {
        BackColor = Color.Transparent;

        _picIcon.IconChar = IconChar.Inbox;
        _picIcon.IconColor = AppTheme.TextSubtle;
        _picIcon.IconSize = 48;
        _picIcon.Size = new Size(48, 48);
        _picIcon.BackColor = Color.Transparent;

        _lblTitle.Text = "Không có dữ liệu";
        _lblTitle.Font = AppTheme.FontCardTitle;
        _lblTitle.ForeColor = AppTheme.TextMain;
        _lblTitle.AutoSize = true;
        _lblTitle.TextAlign = ContentAlignment.MiddleCenter;

        _lblDescription.Text = "Hiện chưa có bản ghi nào được tìm thấy.";
        _lblDescription.Font = AppTheme.FontCaption;
        _lblDescription.ForeColor = AppTheme.TextMuted;
        _lblDescription.Size = new Size(320, 36);
        _lblDescription.TextAlign = ContentAlignment.MiddleCenter;

        _btnAction.Text = "Hành Động";
        _btnAction.ButtonType = AppButtonType.Primary;
        _btnAction.Size = new Size(130, 36);
        _btnAction.Visible = false;

        Controls.Add(_picIcon);
        Controls.Add(_lblTitle);
        Controls.Add(_lblDescription);
        Controls.Add(_btnAction);

        Size = new Size(360, 200);

        AdjustLayout();
    }

    private void AdjustLayout()
    {
        if (_isAdjustingLayout || _picIcon == null || _lblTitle == null || _lblDescription == null || _btnAction == null)
        {
            return;
        }

        _isAdjustingLayout = true;
        try
        {
            _picIcon.Location = new Point((Width - _picIcon.Width) / 2, 20);
            _lblTitle.Location = new Point((Width - _lblTitle.Width) / 2, _picIcon.Bottom + 12);
            _lblDescription.Location = new Point((Width - _lblDescription.Width) / 2, _lblTitle.Bottom + 6);
            _btnAction.Location = new Point((Width - _btnAction.Width) / 2, _lblDescription.Bottom + 12);

            int totalH = _btnAction.Visible ? _btnAction.Bottom + 20 : _lblDescription.Bottom + 20;
            if (Height != totalH)
            {
                Height = totalH;
            }
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
    }
}
