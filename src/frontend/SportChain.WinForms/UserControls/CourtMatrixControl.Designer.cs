using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UserControls;

partial class CourtMatrixControl
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlHeader;
    private Panel pnlHoldingBanner;
    private Panel pnlGridHost;
    private Panel pnlBottomBar;

    // Header controls
    private Label lblDateTitle;
    private DateTimePicker dtpDate;
    private Button btnToday;
    private Button btnTomorrow;
    private Button btnPlus2;
    private Button btnPlus3;
    private AppButton btnRefresh;

    private Label lblSportTitle;
    private Button btnSportAll;
    private Button btnSportPickleball;
    private Button btnSportBadminton;
    private Button btnSportFootball;
    private Button btnSportTennis;

    private FlowLayoutPanel flpLegend;

    // Holding alert controls
    private IconPictureBox picHoldingIcon;
    private Label lblHoldingText;
    private Label lblHoldingTimer;
    private AppButton btnPayDeposit;
    private AppButton btnCancelHolding;

    // Grid
    private DataGridView dgvMatrix;

    // Bottom summary bar
    private Label lblSelectedSummary;
    private Label lblPriceSummary;
    private AppButton btnClearSelection;
    private AppButton btnHoldAndDeposit;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            CleanupResources();
            if (components != null)
            {
                components.Dispose();
            }
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlHeader = new Panel();
        pnlHoldingBanner = new Panel();
        pnlGridHost = new Panel();
        pnlBottomBar = new Panel();

        lblDateTitle = new Label();
        dtpDate = new DateTimePicker();
        btnToday = new Button();
        btnTomorrow = new Button();
        btnPlus2 = new Button();
        btnPlus3 = new Button();
        btnRefresh = new AppButton();

        lblSportTitle = new Label();
        btnSportAll = new Button();
        btnSportPickleball = new Button();
        btnSportBadminton = new Button();
        btnSportFootball = new Button();
        btnSportTennis = new Button();

        flpLegend = new FlowLayoutPanel();

        picHoldingIcon = new IconPictureBox();
        lblHoldingText = new Label();
        lblHoldingTimer = new Label();
        btnPayDeposit = new AppButton();
        btnCancelHolding = new AppButton();

        dgvMatrix = new DataGridView();

        lblSelectedSummary = new Label();
        lblPriceSummary = new Label();
        btnClearSelection = new AppButton();
        btnHoldAndDeposit = new AppButton();

        SuspendLayout();

        // 
        // pnlHeader
        // 
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Height = 100;
        pnlHeader.BackColor = AppTheme.CardBg;
        pnlHeader.Padding = new Padding(16, 12, 16, 8);
        pnlHeader.Paint += (s, e) =>
        {
            using var pen = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
        };

        // Row 1: Date & Actions
        lblDateTitle.Text = "Ngày đặt:";
        lblDateTitle.Font = AppTheme.FontCaptionBold;
        lblDateTitle.ForeColor = AppTheme.TextMain;
        lblDateTitle.Location = new Point(16, 15);
        lblDateTitle.AutoSize = true;

        dtpDate.Location = new Point(88, 11);
        dtpDate.Size = new Size(130, 27);
        dtpDate.Font = AppTheme.FontCaption;
        dtpDate.Format = DateTimePickerFormat.Custom;
        dtpDate.CustomFormat = "dd/MM/yyyy";
        dtpDate.MinDate = DateTime.Today;

        ConfigureQuickDateBtn(btnToday, "Hôm nay", 228);
        ConfigureQuickDateBtn(btnTomorrow, "Ngày mai", 312);
        ConfigureQuickDateBtn(btnPlus2, "+2 Ngày", 396);
        ConfigureQuickDateBtn(btnPlus3, "+3 Ngày", 480);

        btnRefresh.Text = "Làm Mới";
        btnRefresh.Icon = IconChar.Rotate;
        btnRefresh.ButtonType = AppButtonType.Secondary;
        btnRefresh.Location = new Point(568, 9);
        btnRefresh.Size = new Size(100, 32);

        // Row 2: Sport filter pills
        lblSportTitle.Text = "Bộ môn:";
        lblSportTitle.Font = AppTheme.FontCaptionBold;
        lblSportTitle.ForeColor = AppTheme.TextMuted;
        lblSportTitle.Location = new Point(16, 58);
        lblSportTitle.AutoSize = true;

        ConfigureSportBtn(btnSportAll, "Tất cả", 88, true);
        ConfigureSportBtn(btnSportPickleball, "🏓 Pickleball", 164, false);
        ConfigureSportBtn(btnSportBadminton, "🏸 Cầu Lông", 276, false);
        ConfigureSportBtn(btnSportFootball, "⚽ Bóng Đá", 380, false);
        ConfigureSportBtn(btnSportTennis, "🎾 Tennis", 480, false);

        // Legend
        flpLegend.Location = new Point(585, 48);
        flpLegend.Size = new Size(680, 44);
        flpLegend.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;
        flpLegend.FlowDirection = FlowDirection.LeftToRight;
        flpLegend.WrapContents = false;
        flpLegend.BackColor = Color.Transparent;

        AddLegendItem(flpLegend, AppTheme.SuccessLight, Color.FromArgb(6, 95, 70), "Trống");
        AddLegendItem(flpLegend, AppTheme.Primary, Color.White, "Đang chọn");
        AddLegendItem(flpLegend, AppTheme.WarningLight, Color.FromArgb(146, 64, 14), "Giữ cọc (15p)");
        AddLegendItem(flpLegend, AppTheme.InfoLight, Color.FromArgb(3, 105, 161), "Đã cọc");
        AddLegendItem(flpLegend, AppTheme.InUseLight, Color.FromArgb(109, 40, 217), "Đang chơi");
        AddLegendItem(flpLegend, Color.FromArgb(254, 249, 195), Color.FromArgb(133, 77, 14), "⭐ Giờ vàng");

        pnlHeader.Controls.Add(lblDateTitle);
        pnlHeader.Controls.Add(dtpDate);
        pnlHeader.Controls.Add(btnToday);
        pnlHeader.Controls.Add(btnTomorrow);
        pnlHeader.Controls.Add(btnPlus2);
        pnlHeader.Controls.Add(btnPlus3);
        pnlHeader.Controls.Add(btnRefresh);
        pnlHeader.Controls.Add(lblSportTitle);
        pnlHeader.Controls.Add(btnSportAll);
        pnlHeader.Controls.Add(btnSportPickleball);
        pnlHeader.Controls.Add(btnSportBadminton);
        pnlHeader.Controls.Add(btnSportFootball);
        pnlHeader.Controls.Add(btnSportTennis);
        pnlHeader.Controls.Add(flpLegend);

        // 
        // pnlHoldingBanner (Visible when user has an active 15m hold)
        // 
        pnlHoldingBanner.Dock = DockStyle.Top;
        pnlHoldingBanner.Height = 56;
        pnlHoldingBanner.BackColor = AppTheme.WarningLight;
        pnlHoldingBanner.Padding = new Padding(16, 8, 16, 8);
        pnlHoldingBanner.Visible = false;
        pnlHoldingBanner.Paint += (s, e) =>
        {
            using var pen = new Pen(AppTheme.WarningBorder, 1);
            e.Graphics.DrawLine(pen, 0, pnlHoldingBanner.Height - 1, pnlHoldingBanner.Width, pnlHoldingBanner.Height - 1);
        };

        picHoldingIcon.IconChar = IconChar.HourglassHalf;
        picHoldingIcon.IconColor = Color.FromArgb(180, 83, 9);
        picHoldingIcon.IconSize = 22;
        picHoldingIcon.Size = new Size(24, 24);
        picHoldingIcon.Location = new Point(16, 16);
        picHoldingIcon.BackColor = Color.Transparent;

        lblHoldingText.Text = "Bạn đang giữ chỗ: Sân Pickleball 1 | Cọc 30%: 72.000 đ";
        lblHoldingText.Font = AppTheme.FontBodyBold;
        lblHoldingText.ForeColor = Color.FromArgb(146, 64, 14);
        lblHoldingText.Location = new Point(48, 17);
        lblHoldingText.AutoSize = true;

        lblHoldingTimer.Text = "Còn lại: 14:59";
        lblHoldingTimer.Font = AppTheme.FontCaptionBold;
        lblHoldingTimer.ForeColor = Color.FromArgb(146, 64, 14);
        lblHoldingTimer.BackColor = Color.FromArgb(253, 230, 138);
        lblHoldingTimer.Padding = new Padding(8, 5, 8, 5);
        lblHoldingTimer.Location = new Point(560, 13);
        lblHoldingTimer.AutoSize = true;

        btnPayDeposit.Text = "Đặt Cọc Ngay";
        btnPayDeposit.Icon = IconChar.CreditCard;
        btnPayDeposit.ButtonType = AppButtonType.Primary;
        btnPayDeposit.Location = new Point(710, 10);
        btnPayDeposit.Size = new Size(130, 36);

        btnCancelHolding.Text = "Hủy Giữ";
        btnCancelHolding.Icon = IconChar.Times;
        btnCancelHolding.ButtonType = AppButtonType.Danger;
        btnCancelHolding.Location = new Point(850, 10);
        btnCancelHolding.Size = new Size(95, 36);

        pnlHoldingBanner.Controls.Add(picHoldingIcon);
        pnlHoldingBanner.Controls.Add(lblHoldingText);
        pnlHoldingBanner.Controls.Add(lblHoldingTimer);
        pnlHoldingBanner.Controls.Add(btnPayDeposit);
        pnlHoldingBanner.Controls.Add(btnCancelHolding);

        // 
        // pnlGridHost
        // 
        pnlGridHost.Dock = DockStyle.Fill;
        pnlGridHost.BackColor = AppTheme.MainBg;
        pnlGridHost.Padding = new Padding(12);

        // dgvMatrix
        dgvMatrix.Dock = DockStyle.Fill;
        dgvMatrix.BackgroundColor = AppTheme.MainBg;
        dgvMatrix.BorderStyle = BorderStyle.None;
        dgvMatrix.AllowUserToAddRows = false;
        dgvMatrix.AllowUserToDeleteRows = false;
        dgvMatrix.AllowUserToResizeRows = false;
        dgvMatrix.RowHeadersVisible = false;
        dgvMatrix.SelectionMode = DataGridViewSelectionMode.CellSelect;
        dgvMatrix.MultiSelect = false;
        dgvMatrix.ReadOnly = true;
        dgvMatrix.EnableHeadersVisualStyles = false;
        dgvMatrix.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.SidebarDarker;
        dgvMatrix.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvMatrix.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontCaptionBold;
        dgvMatrix.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvMatrix.ColumnHeadersHeight = 44;
        dgvMatrix.RowTemplate.Height = 56;
        dgvMatrix.GridColor = AppTheme.Border;

        pnlGridHost.Controls.Add(dgvMatrix);

        // 
        // pnlBottomBar
        // 
        pnlBottomBar.Dock = DockStyle.Bottom;
        pnlBottomBar.Height = 72;
        pnlBottomBar.BackColor = AppTheme.CardBg;
        pnlBottomBar.Padding = new Padding(20, 12, 20, 12);
        pnlBottomBar.Paint += (s, e) =>
        {
            using var pen = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawLine(pen, 0, 0, pnlBottomBar.Width, 0);
        };

        lblSelectedSummary.Text = "Chưa chọn ca nào. Nhấp vào các ô màu xanh trên ma trận để chọn ca đặt sân.";
        lblSelectedSummary.Font = AppTheme.FontCaption;
        lblSelectedSummary.ForeColor = AppTheme.TextMuted;
        lblSelectedSummary.Location = new Point(20, 14);
        lblSelectedSummary.Size = new Size(580, 20);

        lblPriceSummary.Text = "Tổng tiền: 0 đ | Cọc 30%: 0 đ";
        lblPriceSummary.Font = AppTheme.FontBodyBold;
        lblPriceSummary.ForeColor = AppTheme.TextMain;
        lblPriceSummary.Location = new Point(20, 38);
        lblPriceSummary.Size = new Size(580, 22);

        btnClearSelection.Text = "Bỏ Chọn";
        btnClearSelection.Icon = IconChar.Times;
        btnClearSelection.ButtonType = AppButtonType.Secondary;
        btnClearSelection.Location = new Point(480, 15);
        btnClearSelection.Size = new Size(110, 42);

        btnHoldAndDeposit.Text = "THANH TOÁN CỌC 30%";
        btnHoldAndDeposit.Icon = IconChar.CreditCard;
        btnHoldAndDeposit.ButtonType = AppButtonType.Primary;
        btnHoldAndDeposit.Location = new Point(600, 15);
        btnHoldAndDeposit.Size = new Size(280, 42);
        btnHoldAndDeposit.Enabled = false;

        pnlBottomBar.Controls.Add(lblSelectedSummary);
        pnlBottomBar.Controls.Add(lblPriceSummary);
        pnlBottomBar.Controls.Add(btnClearSelection);
        pnlBottomBar.Controls.Add(btnHoldAndDeposit);

        pnlBottomBar.Resize += (s, e) => LayoutBottomBar();
        pnlHoldingBanner.Resize += (s, e) => LayoutHoldingBanner();

        // 
        // CourtMatrixControl
        // 
        this.AutoScaleDimensions = new SizeF(8F, 20F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.BackColor = AppTheme.MainBg;
        this.Controls.Add(pnlGridHost);
        this.Controls.Add(pnlHoldingBanner);
        this.Controls.Add(pnlHeader);
        this.Controls.Add(pnlBottomBar);
        this.Size = new Size(1040, 680);

        ResumeLayout(false);
    }

    private void ConfigureQuickDateBtn(Button btn, string text, int x)
    {
        btn.Text = text;
        btn.Font = AppTheme.FontSmall;
        btn.Location = new Point(x, 10);
        btn.Size = new Size(80, 28);
        btn.FlatStyle = FlatStyle.Flat;
        btn.BackColor = AppTheme.CardBgAlt;
        btn.ForeColor = AppTheme.TextMain;
        btn.FlatAppearance.BorderColor = AppTheme.Border;
        btn.Cursor = Cursors.Hand;
    }

    private void ConfigureSportBtn(Button btn, string text, int x, bool active)
    {
        btn.Text = text;
        btn.Font = AppTheme.FontSmallBold;
        btn.Location = new Point(x, 54);
        btn.Size = new Size(text.Length > 10 ? 104 : 70, 30);
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.BackColor = active ? AppTheme.Primary : AppTheme.CardBgAlt;
        btn.ForeColor = active ? Color.White : AppTheme.TextMuted;
        btn.Cursor = Cursors.Hand;
    }

    private void AddLegendItem(FlowLayoutPanel panel, Color bg, Color fg, string label)
    {
        var pnl = new Panel { Size = new Size(96, 30), Margin = new Padding(2, 2, 4, 2) };
        var badge = new Label
        {
            Size = new Size(14, 14),
            Location = new Point(2, 8),
            BackColor = bg,
            BorderStyle = BorderStyle.FixedSingle
        };
        var text = new Label
        {
            Text = label,
            Font = AppTheme.FontSmallBold,
            ForeColor = AppTheme.TextMuted,
            Location = new Point(20, 6),
            AutoSize = true
        };
        pnl.Controls.Add(badge);
        pnl.Controls.Add(text);
        panel.Controls.Add(pnl);
    }
}

