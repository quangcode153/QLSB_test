using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UserControls;

partial class ReceptionistPosControl
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlHeader;
    private Label lblTitle;
    private Label lblSubtitle;
    private Panel pnlCheckInCard;
    private TextBox txtCheckInCode;
    private AppButton btnCheckIn;
    private Label lblCheckInStatus;

    private TabControl tabPos;
    private TabPage tabActiveBookings;
    private TabPage tabWalkIn;

    // Tab 1: Active Bookings & Services
    private Panel pnlActiveBookings;
    private Splitter splitterActive;
    private Panel pnlDetailTop;
    private Panel pnlDetailBottom;
    private DataGridView dgvActiveBookings;
    private Panel pnlBookingDetail;
    private Label lblDetailTitle;
    private Label lblDetailCustomer;
    private Label lblDetailCourt;
    private Label lblDetailFinancial;
    private DataGridView dgvServices;
    private ComboBox cboServiceCatalog;
    private NumericUpDown nudQuantity;
    private Label lblQuantity;
    private AppButton btnAddService;
    private AppButton btnCheckout;
    private AppButton btnPrintInvoice;
    private AppButton btnRefreshActive;
    private TableLayoutPanel tlpDetailActions;

    // Tab 2: Walk-In Booking
    private Panel pnlWalkIn;
    private Label lblWalkInTitle;
    private Label lblWalkInCourt;
    private ComboBox cboWalkInCourt;
    private Label lblWalkInSlots;
    private CheckedListBox chkWalkInSlots;
    private Label lblCustomerPhone;
    private TextBox txtWalkInPhone;
    private Label lblCustomerName;
    private TextBox txtWalkInName;
    private Label lblWalkInPayment;
    private ComboBox cboWalkInPayment;
    private Label lblWalkInTotal;
    private AppButton btnCreateWalkIn;

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
        pnlCheckInCard = new Panel();
        txtCheckInCode = new TextBox();
        btnCheckIn = new AppButton();
        lblCheckInStatus = new Label();

        tabPos = new TabControl();
        tabActiveBookings = new TabPage();
        pnlActiveBookings = new Panel();
        splitterActive = new Splitter();
        pnlDetailTop = new Panel();
        pnlDetailBottom = new Panel();
        tlpDetailActions = new TableLayoutPanel();
        dgvActiveBookings = new DataGridView();
        pnlBookingDetail = new Panel();
        lblDetailTitle = new Label();
        lblDetailCustomer = new Label();
        lblDetailCourt = new Label();
        lblDetailFinancial = new Label();
        dgvServices = new DataGridView();
        cboServiceCatalog = new ComboBox();
        nudQuantity = new NumericUpDown();
        lblQuantity = new Label();
        btnAddService = new AppButton();
        btnCheckout = new AppButton();
        btnPrintInvoice = new AppButton();
        btnRefreshActive = new AppButton();

        tabWalkIn = new TabPage();
        pnlWalkIn = new Panel();
        lblWalkInTitle = new Label();
        lblWalkInCourt = new Label();
        cboWalkInCourt = new ComboBox();
        lblWalkInSlots = new Label();
        chkWalkInSlots = new CheckedListBox();
        lblCustomerPhone = new Label();
        txtWalkInPhone = new TextBox();
        lblCustomerName = new Label();
        txtWalkInName = new TextBox();
        lblWalkInPayment = new Label();
        cboWalkInPayment = new ComboBox();
        lblWalkInTotal = new Label();
        btnCreateWalkIn = new AppButton();

        SuspendLayout();

        // 
        // pnlHeader
        // 
        pnlHeader.BackColor = AppTheme.CardBg;
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Height = 136;
        pnlHeader.Padding = new Padding(24, 14, 24, 12);
        pnlHeader.Paint += (s, e) =>
        {
            using var pen = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
        };

        lblTitle.Text = "QUẦY THU NGÂN POS & CHECK-IN NHẬN SÂN";
        lblTitle.Font = AppTheme.FontCardTitle;
        lblTitle.ForeColor = AppTheme.TextMain;
        lblTitle.Location = new Point(24, 12);
        lblTitle.AutoSize = true;

        lblSubtitle.Text = "Quét mã QR check-in vào sân [-15p, +15p], gọi dịch vụ giải khát & bán sân khách vãng lai";
        lblSubtitle.Font = AppTheme.FontSmall;
        lblSubtitle.ForeColor = AppTheme.TextMuted;
        lblSubtitle.Location = new Point(24, 38);
        lblSubtitle.AutoSize = true;

        // pnlCheckInCard
        pnlCheckInCard.Location = new Point(24, 66);
        pnlCheckInCard.Size = new Size(720, 52);
        pnlCheckInCard.BackColor = AppTheme.CardBgAlt;
        pnlCheckInCard.Padding = new Padding(8);

        txtCheckInCode.Font = new Font("Consolas", 10.5F, FontStyle.Bold);
        txtCheckInCode.Location = new Point(12, 12);
        txtCheckInCode.Size = new Size(510, 28);
        txtCheckInCode.PlaceholderText = "Nhập mã vé hoặc đưa đầu đọc mã vạch quét mã QR...";

        btnCheckIn.Text = "Check-in Nhận Sân";
        btnCheckIn.Icon = IconChar.Qrcode;
        btnCheckIn.ButtonType = AppButtonType.Primary;
        btnCheckIn.Location = new Point(532, 9);
        btnCheckIn.Size = new Size(175, 34);

        pnlCheckInCard.Controls.Add(txtCheckInCode);
        pnlCheckInCard.Controls.Add(btnCheckIn);

        lblCheckInStatus.Font = AppTheme.FontCaptionBold;
        lblCheckInStatus.Location = new Point(760, 72);
        lblCheckInStatus.Size = new Size(420, 42);
        lblCheckInStatus.TextAlign = ContentAlignment.MiddleLeft;

        pnlHeader.Controls.Add(lblCheckInStatus);
        pnlHeader.Controls.Add(pnlCheckInCard);
        pnlHeader.Controls.Add(lblSubtitle);
        pnlHeader.Controls.Add(lblTitle);

        // 
        // tabPos
        // 
        tabPos.Dock = DockStyle.Fill;
        tabPos.Font = AppTheme.FontBody;
        tabPos.Controls.Add(tabActiveBookings);
        tabPos.Controls.Add(tabWalkIn);
        tabPos.Padding = new Point(16, 8);

        // tabActiveBookings
        tabActiveBookings.Text = "🏟️ Sân Đang Chơi & Gọi Thêm Dịch Vụ";
        tabActiveBookings.BackColor = AppTheme.MainBg;
        tabActiveBookings.Controls.Add(pnlActiveBookings);
        tabActiveBookings.Controls.Add(splitterActive);
        tabActiveBookings.Controls.Add(pnlBookingDetail);

        // pnlActiveBookings
        pnlActiveBookings.Dock = DockStyle.Fill;
        pnlActiveBookings.Padding = new Padding(12);
        pnlActiveBookings.Controls.Add(dgvActiveBookings);
        pnlActiveBookings.Controls.Add(btnRefreshActive);

        // splitterActive
        splitterActive.Dock = DockStyle.Right;
        splitterActive.Width = 6;
        splitterActive.BackColor = AppTheme.MainBg;
        splitterActive.MinSize = 460;
        splitterActive.MinExtra = 350;

        // btnRefreshActive
        btnRefreshActive.Text = "Làm Mới Danh Sách";
        btnRefreshActive.Icon = IconChar.Rotate;
        btnRefreshActive.ButtonType = AppButtonType.Secondary;
        btnRefreshActive.Dock = DockStyle.Top;
        btnRefreshActive.Height = 36;

        // dgvActiveBookings
        dgvActiveBookings.Dock = DockStyle.Fill;
        dgvActiveBookings.BackgroundColor = AppTheme.CardBg;
        dgvActiveBookings.BorderStyle = BorderStyle.None;
        dgvActiveBookings.RowHeadersVisible = false;
        dgvActiveBookings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvActiveBookings.MultiSelect = false;
        dgvActiveBookings.AllowUserToAddRows = false;
        dgvActiveBookings.AllowUserToDeleteRows = false;
        dgvActiveBookings.ReadOnly = true;
        dgvActiveBookings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvActiveBookings.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.SidebarDarker;
        dgvActiveBookings.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvActiveBookings.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontCaptionBold;
        dgvActiveBookings.ColumnHeadersHeight = 40;
        dgvActiveBookings.RowTemplate.Height = 38;
        dgvActiveBookings.GridColor = AppTheme.Border;

        // pnlBookingDetail
        pnlBookingDetail.Dock = DockStyle.Right;
        pnlBookingDetail.Width = 430;
        pnlBookingDetail.BackColor = AppTheme.CardBg;
        pnlBookingDetail.Padding = new Padding(12);
        pnlBookingDetail.Paint += (s, e) =>
        {
            using var pen = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, pnlBookingDetail.Width - 1, pnlBookingDetail.Height - 1);
        };

        // pnlDetailTop
        pnlDetailTop.Dock = DockStyle.Top;
        pnlDetailTop.Height = 192;
        pnlDetailTop.BackColor = AppTheme.CardBg;

        lblDetailTitle.Text = "CHI TIẾT ĐƠN & PHỤ PHÍ";
        lblDetailTitle.Font = AppTheme.FontCardTitle;
        lblDetailTitle.ForeColor = AppTheme.TextMain;
        lblDetailTitle.Location = new Point(4, 4);
        lblDetailTitle.AutoSize = true;

        lblDetailCustomer.Text = "Khách hàng: (Chưa chọn đơn)";
        lblDetailCustomer.Font = AppTheme.FontCaption;
        lblDetailCustomer.ForeColor = AppTheme.TextMuted;
        lblDetailCustomer.Location = new Point(4, 28);
        lblDetailCustomer.AutoSize = true;

        lblDetailCourt.Text = "Sân thi đấu: - | Ca: -";
        lblDetailCourt.Font = AppTheme.FontCaption;
        lblDetailCourt.ForeColor = AppTheme.TextMuted;
        lblDetailCourt.Location = new Point(4, 48);
        lblDetailCourt.AutoSize = true;

        lblDetailFinancial.Text = "Tiền sân: 0 đ | Đã cọc: 0 đ\nCần thu còn lại: 0 đ";
        lblDetailFinancial.Font = AppTheme.FontCaptionBold;
        lblDetailFinancial.ForeColor = AppTheme.Primary;
        lblDetailFinancial.Location = new Point(4, 70);
        lblDetailFinancial.AutoSize = true;

        // Dòng 1: Dropdown chọn món / dịch vụ (kéo dài toàn bộ chiều rộng)
        cboServiceCatalog.Location = new Point(4, 114);
        cboServiceCatalog.Size = new Size(400, 28);
        cboServiceCatalog.DropDownStyle = ComboBoxStyle.DropDownList;
        cboServiceCatalog.Font = AppTheme.FontCaption;
        cboServiceCatalog.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // Dòng 2: Nhập số lượng + Nút Thêm dịch vụ
        lblQuantity.Text = "SL:";
        lblQuantity.Font = AppTheme.FontCaptionBold;
        lblQuantity.Location = new Point(4, 153);
        lblQuantity.Size = new Size(26, 24);
        lblQuantity.TextAlign = ContentAlignment.MiddleLeft;

        nudQuantity.Location = new Point(32, 150);
        nudQuantity.Size = new Size(58, 28);
        nudQuantity.Minimum = 1;
        nudQuantity.Maximum = 50;
        nudQuantity.Value = 1;
        nudQuantity.Font = AppTheme.FontCaption;

        btnAddService.Text = "Thêm Dịch Vụ";
        btnAddService.Icon = IconChar.Plus;
        btnAddService.ButtonType = AppButtonType.Primary;
        btnAddService.Location = new Point(98, 148);
        btnAddService.Size = new Size(150, 32);
        btnAddService.Anchor = AnchorStyles.Top | AnchorStyles.Left;

        pnlDetailTop.Controls.Add(lblDetailTitle);
        pnlDetailTop.Controls.Add(lblDetailCustomer);
        pnlDetailTop.Controls.Add(lblDetailCourt);
        pnlDetailTop.Controls.Add(lblDetailFinancial);
        pnlDetailTop.Controls.Add(cboServiceCatalog);
        pnlDetailTop.Controls.Add(lblQuantity);
        pnlDetailTop.Controls.Add(nudQuantity);
        pnlDetailTop.Controls.Add(btnAddService);

        // pnlDetailBottom
        pnlDetailBottom.Dock = DockStyle.Bottom;
        pnlDetailBottom.Height = 52;
        pnlDetailBottom.Padding = new Padding(0, 6, 0, 0);
        pnlDetailBottom.BackColor = AppTheme.CardBg;

        btnCheckout.Text = "Tất Toán & Trả Sân";
        btnCheckout.Icon = IconChar.CheckCircle;
        btnCheckout.ButtonType = AppButtonType.Primary;
        btnCheckout.Dock = DockStyle.Fill;
        btnCheckout.Margin = new Padding(0, 0, 4, 0);

        btnPrintInvoice.Text = "In Hóa Đơn K80";
        btnPrintInvoice.Icon = IconChar.Print;
        btnPrintInvoice.ButtonType = AppButtonType.Secondary;
        btnPrintInvoice.Dock = DockStyle.Fill;
        btnPrintInvoice.Margin = new Padding(4, 0, 0, 0);

        tlpDetailActions.Dock = DockStyle.Fill;
        tlpDetailActions.ColumnCount = 2;
        tlpDetailActions.RowCount = 1;
        tlpDetailActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
        tlpDetailActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
        tlpDetailActions.Controls.Add(btnCheckout, 0, 0);
        tlpDetailActions.Controls.Add(btnPrintInvoice, 1, 0);
        pnlDetailBottom.Controls.Add(tlpDetailActions);

        // dgvServices
        dgvServices.Dock = DockStyle.Fill;
        dgvServices.BackgroundColor = AppTheme.CardBgAlt;
        dgvServices.BorderStyle = BorderStyle.None;
        dgvServices.RowHeadersVisible = false;
        dgvServices.AllowUserToAddRows = false;
        dgvServices.ReadOnly = true;
        dgvServices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvServices.GridColor = AppTheme.Border;
        dgvServices.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.SidebarBg;
        dgvServices.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvServices.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontCaptionBold;
        dgvServices.ColumnHeadersHeight = 36;
        dgvServices.RowTemplate.Height = 34;

        pnlBookingDetail.Controls.Add(dgvServices);
        pnlBookingDetail.Controls.Add(pnlDetailTop);
        pnlBookingDetail.Controls.Add(pnlDetailBottom);
        pnlDetailTop.BringToFront();
        pnlDetailBottom.BringToFront();
        dgvServices.BringToFront();

        // 
        // tabWalkIn (Bán sân tại quầy)
        // 
        tabWalkIn.Text = "⚡ Bán Sân Vãng Lai (Walk-in)";
        tabWalkIn.BackColor = AppTheme.MainBg;
        tabWalkIn.Controls.Add(pnlWalkIn);

        pnlWalkIn.BackColor = AppTheme.CardBg;
        pnlWalkIn.Location = new Point(24, 20);
        pnlWalkIn.Size = new Size(600, 490);
        pnlWalkIn.Padding = new Padding(24);
        pnlWalkIn.Paint += (s, e) =>
        {
            using var pen = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, pnlWalkIn.Width - 1, pnlWalkIn.Height - 1);
        };

        lblWalkInTitle.Text = "TẠO ĐƠN ĐẶT SÂN VÃNG LAI TẠI QUẦY (THU 100%)";
        lblWalkInTitle.Font = AppTheme.FontCardTitle;
        lblWalkInTitle.ForeColor = AppTheme.TextMain;
        lblWalkInTitle.Location = new Point(20, 16);
        lblWalkInTitle.AutoSize = true;

        lblWalkInCourt.Text = "Chọn Sân:";
        lblWalkInCourt.Font = AppTheme.FontCaptionBold;
        lblWalkInCourt.Location = new Point(20, 52);
        lblWalkInCourt.AutoSize = true;

        cboWalkInCourt.Location = new Point(20, 74);
        cboWalkInCourt.Size = new Size(250, 28);
        cboWalkInCourt.DropDownStyle = ComboBoxStyle.DropDownList;
        cboWalkInCourt.Font = AppTheme.FontCaption;

        lblCustomerPhone.Text = "Số Điện Thoại Khách:";
        lblCustomerPhone.Font = AppTheme.FontCaptionBold;
        lblCustomerPhone.Location = new Point(290, 52);
        lblCustomerPhone.AutoSize = true;

        txtWalkInPhone.Location = new Point(290, 74);
        txtWalkInPhone.Size = new Size(250, 27);
        txtWalkInPhone.PlaceholderText = "VD: 0988776655";
        txtWalkInPhone.Font = AppTheme.FontCaption;

        lblCustomerName.Text = "Họ Tên Khách Hàng:";
        lblCustomerName.Font = AppTheme.FontCaptionBold;
        lblCustomerName.Location = new Point(290, 112);
        lblCustomerName.AutoSize = true;

        txtWalkInName.Location = new Point(290, 134);
        txtWalkInName.Size = new Size(250, 27);
        txtWalkInName.PlaceholderText = "Khách Vãng Lai";
        txtWalkInName.Font = AppTheme.FontCaption;

        lblWalkInPayment.Text = "Phương Thức Thanh Toán:";
        lblWalkInPayment.Font = AppTheme.FontCaptionBold;
        lblWalkInPayment.Location = new Point(290, 174);
        lblWalkInPayment.AutoSize = true;

        cboWalkInPayment.Location = new Point(290, 196);
        cboWalkInPayment.Size = new Size(250, 28);
        cboWalkInPayment.DropDownStyle = ComboBoxStyle.DropDownList;
        cboWalkInPayment.Font = AppTheme.FontCaption;

        lblWalkInSlots.Text = "Chọn Ca Giờ (Đang Trống):";
        lblWalkInSlots.Font = AppTheme.FontCaptionBold;
        lblWalkInSlots.Location = new Point(20, 112);
        lblWalkInSlots.AutoSize = true;

        chkWalkInSlots.Location = new Point(20, 134);
        chkWalkInSlots.Size = new Size(250, 190);
        chkWalkInSlots.CheckOnClick = true;
        chkWalkInSlots.Font = AppTheme.FontCaption;

        lblWalkInTotal.Text = "TỔNG TIỀN THU NGAY: 0 VNĐ";
        lblWalkInTotal.Font = AppTheme.FontCardTitle;
        lblWalkInTotal.ForeColor = AppTheme.Danger;
        lblWalkInTotal.Location = new Point(20, 345);
        lblWalkInTotal.AutoSize = true;

        btnCreateWalkIn.Text = "Tạo Đơn & Vào Sân Ngay (Thu 100%)";
        btnCreateWalkIn.Icon = IconChar.Bolt;
        btnCreateWalkIn.ButtonType = AppButtonType.Primary;
        btnCreateWalkIn.Location = new Point(20, 390);
        btnCreateWalkIn.Size = new Size(520, 44);

        pnlWalkIn.Controls.Add(lblWalkInTitle);
        pnlWalkIn.Controls.Add(lblWalkInCourt);
        pnlWalkIn.Controls.Add(cboWalkInCourt);
        pnlWalkIn.Controls.Add(lblCustomerPhone);
        pnlWalkIn.Controls.Add(txtWalkInPhone);
        pnlWalkIn.Controls.Add(lblCustomerName);
        pnlWalkIn.Controls.Add(txtWalkInName);
        pnlWalkIn.Controls.Add(lblWalkInPayment);
        pnlWalkIn.Controls.Add(cboWalkInPayment);
        pnlWalkIn.Controls.Add(lblWalkInSlots);
        pnlWalkIn.Controls.Add(chkWalkInSlots);
        pnlWalkIn.Controls.Add(lblWalkInTotal);
        pnlWalkIn.Controls.Add(btnCreateWalkIn);

        // Form root
        Controls.Add(tabPos);
        Controls.Add(pnlHeader);
        Size = new Size(1100, 680);
        Dock = DockStyle.Fill;
        BackColor = AppTheme.MainBg;

        ResumeLayout(false);
    }
}

