using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UserControls;

partial class MyBookingsControl
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlHeader;
    private Label lblTitle;
    private Label lblSubtitle;
    private AppButton btnRefresh;

    private DataGridView dgvBookings;
    private Panel pnlActions;
    private Label lblSelectedInfo;
    private FlowLayoutPanel flpActionButtons;
    private AppButton btnViewTicket;
    private AppButton btnCopyCode;
    private AppButton btnPayDeposit;
    private AppButton btnCancel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlHeader = new Panel();
        lblTitle = new Label();
        lblSubtitle = new Label();
        btnRefresh = new AppButton();

        dgvBookings = new DataGridView();

        pnlActions = new Panel();
        lblSelectedInfo = new Label();
        flpActionButtons = new FlowLayoutPanel();
        btnViewTicket = new AppButton();
        btnCopyCode = new AppButton();
        btnPayDeposit = new AppButton();
        btnCancel = new AppButton();

        SuspendLayout();

        // 
        // pnlHeader
        // 
        pnlHeader.BackColor = AppTheme.CardBg;
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Height = 72;
        pnlHeader.Padding = new Padding(24, 14, 24, 12);
        pnlHeader.Paint += (s, e) =>
        {
            using var pen = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
        };

        lblTitle.Text = "LỊCH SỬ ĐẶT SÂN & VÉ ĐIỆN TỬ CỦA TÔI";
        lblTitle.Font = AppTheme.FontCardTitle;
        lblTitle.ForeColor = AppTheme.TextMain;
        lblTitle.Location = new Point(24, 12);
        lblTitle.AutoSize = true;
        lblTitle.UseMnemonic = false;

        lblSubtitle.Text = "Theo dõi trạng thái các ca đã đặt, mã QR check-in đã cấp, thanh toán cọc VietQR hoặc hủy đơn hoàn tiền";
        lblSubtitle.Font = AppTheme.FontSmall;
        lblSubtitle.ForeColor = AppTheme.TextMuted;
        lblSubtitle.Location = new Point(24, 38);
        lblSubtitle.AutoSize = true;

        btnRefresh.Text = "Làm Mới";
        btnRefresh.Icon = IconChar.Rotate;
        btnRefresh.ButtonType = AppButtonType.Secondary;
        btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnRefresh.Location = new Point(900, 16);
        btnRefresh.Size = new Size(116, 36);

        pnlHeader.Controls.Add(btnRefresh);
        pnlHeader.Controls.Add(lblSubtitle);
        pnlHeader.Controls.Add(lblTitle);

        // 
        // dgvBookings
        // 
        dgvBookings.Dock = DockStyle.Fill;
        dgvBookings.BackgroundColor = AppTheme.MainBg;
        dgvBookings.BorderStyle = BorderStyle.None;
        dgvBookings.RowHeadersVisible = false;
        dgvBookings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvBookings.MultiSelect = false;
        dgvBookings.AllowUserToAddRows = false;
        dgvBookings.ReadOnly = true;
        dgvBookings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvBookings.GridColor = AppTheme.Border;

        // 
        // pnlActions
        // 
        pnlActions.BackColor = AppTheme.CardBg;
        pnlActions.Dock = DockStyle.Bottom;
        pnlActions.Height = 72;
        pnlActions.Padding = new Padding(24, 12, 24, 12);
        pnlActions.Paint += (s, e) =>
        {
            using var pen = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawLine(pen, 0, 0, pnlActions.Width, 0);
        };

        lblSelectedInfo.Text = "Vui lòng chọn một đơn đặt trong bảng để thao tác.";
        lblSelectedInfo.Font = AppTheme.FontCaptionBold;
        lblSelectedInfo.ForeColor = AppTheme.TextMuted;
        lblSelectedInfo.Location = new Point(24, 14);
        lblSelectedInfo.Size = new Size(380, 44);
        lblSelectedInfo.TextAlign = ContentAlignment.MiddleLeft;
        lblSelectedInfo.AutoEllipsis = true;

        flpActionButtons.Dock = DockStyle.Right;
        flpActionButtons.Width = 600;
        flpActionButtons.Height = 48;
        flpActionButtons.FlowDirection = FlowDirection.RightToLeft;
        flpActionButtons.WrapContents = false;
        flpActionButtons.BackColor = Color.Transparent;
        flpActionButtons.Padding = new Padding(0, 4, 0, 0);

        btnCancel.Text = "Hủy Đơn";
        btnCancel.Icon = IconChar.TimesCircle;
        btnCancel.ButtonType = AppButtonType.Danger;
        btnCancel.Size = new Size(116, 40);
        btnCancel.Margin = new Padding(6, 0, 0, 0);

        btnPayDeposit.Text = "Nộp Cọc";
        btnPayDeposit.Icon = IconChar.CreditCard;
        btnPayDeposit.ButtonType = AppButtonType.Primary;
        btnPayDeposit.Size = new Size(116, 40);
        btnPayDeposit.Margin = new Padding(6, 0, 0, 0);

        btnCopyCode.Text = "Sao Chép";
        btnCopyCode.Icon = IconChar.Copy;
        btnCopyCode.ButtonType = AppButtonType.Secondary;
        btnCopyCode.Size = new Size(116, 40);
        btnCopyCode.Margin = new Padding(6, 0, 0, 0);

        btnViewTicket.Text = "Xem Vé / QR";
        btnViewTicket.Icon = IconChar.TicketAlt;
        btnViewTicket.ButtonType = AppButtonType.Accent;
        btnViewTicket.Size = new Size(140, 40);
        btnViewTicket.Margin = new Padding(6, 0, 0, 0);

        flpActionButtons.Controls.Add(btnCancel);
        flpActionButtons.Controls.Add(btnPayDeposit);
        flpActionButtons.Controls.Add(btnCopyCode);
        flpActionButtons.Controls.Add(btnViewTicket);

        pnlActions.Controls.Add(lblSelectedInfo);
        pnlActions.Controls.Add(flpActionButtons);

        // Root
        Controls.Add(dgvBookings);
        Controls.Add(pnlActions);
        Controls.Add(pnlHeader);
        Size = new Size(1040, 640);
        Dock = DockStyle.Fill;
        BackColor = AppTheme.MainBg;

        ResumeLayout(false);
    }
}

