using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UI.Dialogs;

/// <summary>
/// Hộp thoại xác nhận thao tác nhạy cảm (Confirm Dialog) hiện đại, thay thế MessageBox cổ điển.
/// </summary>
public class ConfirmDialog : Form
{
    private readonly AppButton _btnConfirm;
    private readonly AppButton _btnCancel;

    public ConfirmDialog(string title, string message, string confirmText = "Xác Nhận", bool isDestructive = false)
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(420, 220);
        BackColor = AppTheme.CardBg;
        ShowInTaskbar = false;

        var card = new CardPanel
        {
            Dock = DockStyle.Fill,
            BorderColor = AppTheme.Border,
            CornerRadius = AppTheme.RadiusCard,
            Padding = new Padding(24)
        };

        var picIcon = new IconPictureBox
        {
            IconChar = isDestructive ? IconChar.ExclamationTriangle : IconChar.QuestionCircle,
            IconColor = isDestructive ? AppTheme.Danger : AppTheme.Primary,
            IconSize = 36,
            Size = new Size(36, 36),
            Location = new Point(24, 24),
            BackColor = Color.Transparent
        };

        var lblTitle = new Label
        {
            Text = title,
            Font = AppTheme.FontCardTitle,
            ForeColor = AppTheme.TextMain,
            Location = new Point(72, 24),
            AutoSize = true
        };

        var lblMessage = new Label
        {
            Text = message,
            Font = AppTheme.FontCaption,
            ForeColor = AppTheme.TextMuted,
            Location = new Point(72, 54),
            Size = new Size(320, 80)
        };

        _btnCancel = new AppButton
        {
            Text = "Hủy Bỏ",
            ButtonType = AppButtonType.Secondary,
            Size = new Size(100, 36),
            Location = new Point(190, 160)
        };
        _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

        _btnConfirm = new AppButton
        {
            Text = confirmText,
            ButtonType = isDestructive ? AppButtonType.Danger : AppButtonType.Primary,
            Size = new Size(100, 36),
            Location = new Point(296, 160)
        };
        _btnConfirm.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };

        card.Controls.Add(picIcon);
        card.Controls.Add(lblTitle);
        card.Controls.Add(lblMessage);
        card.Controls.Add(_btnCancel);
        card.Controls.Add(_btnConfirm);

        Controls.Add(card);

        AcceptButton = _btnConfirm;
        CancelButton = _btnCancel;
    }

    public static bool Show(IWin32Window owner, string title, string message, string confirmText = "Xác Nhận", bool isDestructive = false)
    {
        using var dlg = new ConfirmDialog(title, message, confirmText, isDestructive);
        return dlg.ShowDialog(owner) == DialogResult.OK;
    }
}
