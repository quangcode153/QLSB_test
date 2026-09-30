using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UserControls;

partial class BranchReportControl
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlHeader;
    private Label lblTitle;
    private Label lblSubtitle;
    private AppButton btnRefresh;

    private Panel pnlCards;
    private KpiCard cardRevenue;
    private KpiCard cardCompleted;
    private KpiCard cardActive;
    private KpiCard cardNoShow;

    private TabControl tabReports;
    private TabPage tabCourts;
    private TabPage tabBookings;

    // Tab Courts
    private DataGridView dgvCourts;
    private Panel pnlCourtActions;
    private AppButton btnAddCourt;
    private AppButton btnToggleMaintenance;
    private Label lblCourtActionHint;

    // Tab Bookings
    private DataGridView dgvBranchBookings;

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

        pnlCards = new Panel();
        cardRevenue = new KpiCard();
        cardCompleted = new KpiCard();
        cardActive = new KpiCard();
        cardNoShow = new KpiCard();

        tabReports = new TabControl();
        tabCourts = new TabPage();
        dgvCourts = new DataGridView();
        pnlCourtActions = new Panel();
        btnAddCourt = new AppButton();
        btnToggleMaintenance = new AppButton();
        lblCourtActionHint = new Label();

        tabBookings = new TabPage();
        dgvBranchBookings = new DataGridView();

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

        lblTitle.Text = "BÁO CÁO DOANH THU & GIÁM SÁT CHI NHÁNH";
        lblTitle.Font = AppTheme.FontCardTitle;
        lblTitle.ForeColor = AppTheme.TextMain;
        lblTitle.Location = new Point(24, 12);
        lblTitle.AutoSize = true;

        lblSubtitle.Text = "Giám sát hiệu suất lấp đầy ca sân, doanh thu thực tế, và điều chuyển trạng thái bảo trì sân";
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
        // pnlCards
        // 
        pnlCards.BackColor = AppTheme.MainBg;
        pnlCards.Dock = DockStyle.Top;
        pnlCards.Height = 126;
        pnlCards.Padding = new Padding(24, 12, 24, 12);

        // cardRevenue
        cardRevenue.Title = "DOANH THU THỰC THU";
        cardRevenue.Value = "0 VNĐ";
        cardRevenue.Icon = IconChar.Coins;
        cardRevenue.AccentColor = AppTheme.Primary;
        cardRevenue.ChangeText = "Theo ca đã chơi";
        cardRevenue.Location = new Point(24, 10);
        cardRevenue.Size = new Size(240, 104);

        // cardCompleted
        cardCompleted.Title = "CA ĐÃ HOÀN TẤT";
        cardCompleted.Value = "0 ca";
        cardCompleted.Icon = IconChar.CheckDouble;
        cardCompleted.AccentColor = AppTheme.Success;
        cardCompleted.ChangeText = "Hoàn tất";
        cardCompleted.Location = new Point(280, 10);
        cardCompleted.Size = new Size(240, 104);

        // cardActive
        cardActive.Title = "ĐANG CHƠI / CHỜ CỌC";
        cardActive.Value = "0 ca";
        cardActive.Icon = IconChar.Running;
        cardActive.AccentColor = AppTheme.Info;
        cardActive.ChangeText = "Đang diễn ra";
        cardActive.Location = new Point(536, 10);
        cardActive.Size = new Size(240, 104);

        // cardNoShow
        cardNoShow.Title = "VẮNG MẶT / HỦY";
        cardNoShow.Value = "0 đơn";
        cardNoShow.Icon = IconChar.UserSlash;
        cardNoShow.AccentColor = AppTheme.Danger;
        cardNoShow.ChangeText = "No-show & huỷ";
        cardNoShow.IsPositive = false;
        cardNoShow.Location = new Point(792, 10);
        cardNoShow.Size = new Size(240, 104);

        pnlCards.Controls.Add(cardNoShow);
        pnlCards.Controls.Add(cardActive);
        pnlCards.Controls.Add(cardCompleted);
        pnlCards.Controls.Add(cardRevenue);

        // 
        // tabReports
        // 
        tabReports.Dock = DockStyle.Fill;
        tabReports.Font = AppTheme.FontBody;
        tabReports.Controls.Add(tabCourts);
        tabReports.Controls.Add(tabBookings);
        tabReports.Padding = new Point(16, 8);

        // tabCourts
        tabCourts.Text = "🏟️ Quản Lý Sân & Điều Phối Bảo Trì";
        tabCourts.BackColor = AppTheme.MainBg;
        tabCourts.Controls.Add(dgvCourts);
        tabCourts.Controls.Add(pnlCourtActions);

        // dgvCourts
        dgvCourts.Dock = DockStyle.Fill;
        dgvCourts.BackgroundColor = AppTheme.CardBg;
        dgvCourts.BorderStyle = BorderStyle.None;
        dgvCourts.RowHeadersVisible = false;
        dgvCourts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvCourts.MultiSelect = false;
        dgvCourts.AllowUserToAddRows = false;
        dgvCourts.ReadOnly = true;
        dgvCourts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvCourts.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.SidebarDarker;
        dgvCourts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvCourts.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontCaptionBold;
        dgvCourts.ColumnHeadersHeight = 42;
        dgvCourts.RowTemplate.Height = 40;
        dgvCourts.GridColor = AppTheme.Border;

        // pnlCourtActions
        pnlCourtActions.Dock = DockStyle.Bottom;
        pnlCourtActions.Height = 60;
        pnlCourtActions.Padding = new Padding(24, 10, 24, 10);
        pnlCourtActions.BackColor = AppTheme.CardBg;
        pnlCourtActions.Paint += (s, e) =>
        {
            using var pen = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawLine(pen, 0, 0, pnlCourtActions.Width, 0);
        };

        btnAddCourt.Text = "Thêm Sân Mới";
        btnAddCourt.Icon = IconChar.Plus;
        btnAddCourt.ButtonType = AppButtonType.Primary;
        btnAddCourt.Location = new Point(24, 10);
        btnAddCourt.Size = new Size(160, 40);

        btnToggleMaintenance.Text = "Bật / Tắt Bảo Trì Sân";
        btnToggleMaintenance.Icon = IconChar.Wrench;
        btnToggleMaintenance.ButtonType = AppButtonType.Secondary;
        btnToggleMaintenance.Location = new Point(194, 10);
        btnToggleMaintenance.Size = new Size(190, 40);

        lblCourtActionHint.Text = "Chọn sân trên bảng để đổi trạng thái bảo trì hoặc thêm sân mới cho cơ sở.";
        lblCourtActionHint.Font = AppTheme.FontCaption;
        lblCourtActionHint.ForeColor = AppTheme.TextMuted;
        lblCourtActionHint.Location = new Point(396, 20);
        lblCourtActionHint.AutoSize = true;

        pnlCourtActions.Controls.Add(btnAddCourt);
        pnlCourtActions.Controls.Add(btnToggleMaintenance);
        pnlCourtActions.Controls.Add(lblCourtActionHint);

        // tabBookings
        tabBookings.Text = "📋 Danh Sách Đơn Đặt Trong Ngày";
        tabBookings.BackColor = AppTheme.MainBg;
        tabBookings.Controls.Add(dgvBranchBookings);

        // dgvBranchBookings
        dgvBranchBookings.Dock = DockStyle.Fill;
        dgvBranchBookings.BackgroundColor = AppTheme.CardBg;
        dgvBranchBookings.BorderStyle = BorderStyle.None;
        dgvBranchBookings.RowHeadersVisible = false;
        dgvBranchBookings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvBranchBookings.MultiSelect = false;
        dgvBranchBookings.AllowUserToAddRows = false;
        dgvBranchBookings.ReadOnly = true;
        dgvBranchBookings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvBranchBookings.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.SidebarDarker;
        dgvBranchBookings.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvBranchBookings.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontCaptionBold;
        dgvBranchBookings.ColumnHeadersHeight = 42;
        dgvBranchBookings.RowTemplate.Height = 40;
        dgvBranchBookings.GridColor = AppTheme.Border;

        // Root
        Controls.Add(tabReports);
        Controls.Add(pnlCards);
        Controls.Add(pnlHeader);
        Size = new Size(1040, 680);
        Dock = DockStyle.Fill;
        BackColor = AppTheme.MainBg;

        ResumeLayout(false);
    }
}

