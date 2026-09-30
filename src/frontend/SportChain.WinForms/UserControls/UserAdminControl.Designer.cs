using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UserControls;

partial class UserAdminControl
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlHeader;
    private Label lblTitle;
    private Label lblSubtitle;
    private AppButton btnRefreshAll;

    private TabControl tabAdmin;
    private TabPage tabUsers;
    private TabPage tabBranches;
    private TabPage tabAuditLogs;

    // Tab 1: Users
    private Panel pnlUsersList;
    private Splitter splitterUsers;
    private DataGridView dgvUsers;
    private Panel pnlUserActions;
    private AppButton btnToggleUserStatus;
    private Label lblUserListTitle;

    private Panel pnlCreateStaff;
    private Label lblCreateStaffTitle;
    private Label lblStaffName;
    private TextBox txtStaffName;
    private Label lblStaffEmail;
    private TextBox txtStaffEmail;
    private Label lblStaffPass;
    private TextBox txtStaffPass;
    private Label lblStaffPhone;
    private TextBox txtStaffPhone;
    private Label lblStaffRole;
    private ComboBox cboStaffRole;
    private Label lblStaffBranch;
    private ComboBox cboStaffBranch;
    private AppButton btnCreateStaff;

    // Tab 2: Branches
    private DataGridView dgvBranches;
    private Panel pnlBranchActions;
    private AppButton btnApproveBranch;
    private AppButton btnToggleBranchStatus;

    // Tab 3: Audit Logs
    private DataGridView dgvAuditLogs;
    private Panel pnlAuditActions;
    private Label lblAuditHint;
    private NumericUpDown nudLogLimit;
    private AppButton btnLoadLogs;

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
        btnRefreshAll = new AppButton();

        tabAdmin = new TabControl();
        tabUsers = new TabPage();
        pnlUsersList = new Panel();
        splitterUsers = new Splitter();
        dgvUsers = new DataGridView();
        pnlUserActions = new Panel();
        lblUserListTitle = new Label();
        btnToggleUserStatus = new AppButton();

        pnlCreateStaff = new Panel();
        lblCreateStaffTitle = new Label();
        lblStaffName = new Label();
        txtStaffName = new TextBox();
        lblStaffEmail = new Label();
        txtStaffEmail = new TextBox();
        lblStaffPass = new Label();
        txtStaffPass = new TextBox();
        lblStaffPhone = new Label();
        txtStaffPhone = new TextBox();
        lblStaffRole = new Label();
        cboStaffRole = new ComboBox();
        lblStaffBranch = new Label();
        cboStaffBranch = new ComboBox();
        btnCreateStaff = new AppButton();

        tabBranches = new TabPage();
        dgvBranches = new DataGridView();
        pnlBranchActions = new Panel();
        btnApproveBranch = new AppButton();
        btnToggleBranchStatus = new AppButton();

        tabAuditLogs = new TabPage();
        dgvAuditLogs = new DataGridView();
        pnlAuditActions = new Panel();
        lblAuditHint = new Label();
        nudLogLimit = new NumericUpDown();
        btnLoadLogs = new AppButton();

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

        lblTitle.Text = "QUẢN TRỊ HỆ THỐNG & PHÂN QUYỀN TOÀN CHUỖI (SUPERADMIN)";
        lblTitle.Font = AppTheme.FontCardTitle;
        lblTitle.ForeColor = AppTheme.TextMain;
        lblTitle.Location = new Point(24, 12);
        lblTitle.AutoSize = true;

        lblSubtitle.Text = "Cấp quyền nhân sự (Quản lý/Lễ tân), phê duyệt hoạt động cơ sở mới & truy vết nhật ký kiểm toán";
        lblSubtitle.Font = AppTheme.FontSmall;
        lblSubtitle.ForeColor = AppTheme.TextMuted;
        lblSubtitle.Location = new Point(24, 38);
        lblSubtitle.AutoSize = true;

        btnRefreshAll.Text = "Làm Mới Dữ Liệu";
        btnRefreshAll.Icon = IconChar.Rotate;
        btnRefreshAll.ButtonType = AppButtonType.Secondary;
        btnRefreshAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnRefreshAll.Location = new Point(860, 16);
        btnRefreshAll.Size = new Size(156, 36);

        pnlHeader.Controls.Add(btnRefreshAll);
        pnlHeader.Controls.Add(lblSubtitle);
        pnlHeader.Controls.Add(lblTitle);

        // 
        // tabAdmin
        // 
        tabAdmin.Dock = DockStyle.Fill;
        tabAdmin.Font = AppTheme.FontBody;
        tabAdmin.Controls.Add(tabUsers);
        tabAdmin.Controls.Add(tabBranches);
        tabAdmin.Controls.Add(tabAuditLogs);
        tabAdmin.Padding = new Point(16, 8);

        // 
        // tabUsers
        // 
        tabUsers.Text = "👥 Nhân Sự & Người Dùng";
        tabUsers.BackColor = AppTheme.MainBg;
        tabUsers.Controls.Add(pnlUsersList);
        tabUsers.Controls.Add(splitterUsers);
        tabUsers.Controls.Add(pnlCreateStaff);

        // pnlUsersList
        pnlUsersList.Dock = DockStyle.Fill;
        pnlUsersList.Padding = new Padding(12);
        pnlUsersList.Controls.Add(dgvUsers);
        pnlUsersList.Controls.Add(pnlUserActions);

        pnlUserActions.Dock = DockStyle.Top;
        pnlUserActions.Height = 44;
        pnlUserActions.Controls.Add(lblUserListTitle);
        pnlUserActions.Controls.Add(btnToggleUserStatus);

        lblUserListTitle.Text = "DANH SÁCH TÀI KHOẢN HỆ THỐNG:";
        lblUserListTitle.Font = AppTheme.FontCaptionBold;
        lblUserListTitle.ForeColor = AppTheme.TextMain;
        lblUserListTitle.Location = new Point(4, 12);
        lblUserListTitle.AutoSize = true;

        btnToggleUserStatus.Text = "Khóa / Mở Khóa";
        btnToggleUserStatus.Icon = IconChar.Lock;
        btnToggleUserStatus.ButtonType = AppButtonType.Secondary;
        btnToggleUserStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnToggleUserStatus.Location = new Point(470, 6);
        btnToggleUserStatus.Size = new Size(150, 32);

        dgvUsers.Dock = DockStyle.Fill;
        dgvUsers.BackgroundColor = AppTheme.CardBg;
        dgvUsers.BorderStyle = BorderStyle.None;
        dgvUsers.RowHeadersVisible = false;
        dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvUsers.MultiSelect = false;
        dgvUsers.AllowUserToAddRows = false;
        dgvUsers.ReadOnly = true;
        dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.SidebarDarker;
        dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvUsers.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontCaptionBold;
        dgvUsers.ColumnHeadersHeight = 40;
        dgvUsers.RowTemplate.Height = 38;
        dgvUsers.GridColor = AppTheme.Border;

        // splitterUsers
        splitterUsers.Dock = DockStyle.Right;
        splitterUsers.Width = 6;
        splitterUsers.BackColor = AppTheme.MainBg;
        splitterUsers.MinSize = 360;
        splitterUsers.MinExtra = 400;

        // pnlCreateStaff
        pnlCreateStaff.Dock = DockStyle.Right;
        pnlCreateStaff.Width = 380;
        pnlCreateStaff.BackColor = AppTheme.CardBg;
        pnlCreateStaff.Padding = new Padding(16);
        pnlCreateStaff.AutoScroll = true;
        pnlCreateStaff.Paint += (s, e) =>
        {
            using var pen = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, pnlCreateStaff.Width - 1, pnlCreateStaff.Height - 1);
        };

        lblCreateStaffTitle.Text = "CẤP TÀI KHOẢN NHÂN VIÊN";
        lblCreateStaffTitle.Font = AppTheme.FontCardTitle;
        lblCreateStaffTitle.ForeColor = AppTheme.Primary;
        lblCreateStaffTitle.Location = new Point(16, 12);
        lblCreateStaffTitle.AutoSize = true;

        lblStaffName.Text = "Họ và Tên Nhân Viên:";
        lblStaffName.Font = AppTheme.FontCaptionBold;
        lblStaffName.Location = new Point(16, 44);
        lblStaffName.AutoSize = true;

        txtStaffName.Location = new Point(16, 66);
        txtStaffName.Size = new Size(330, 27);
        txtStaffName.Font = AppTheme.FontCaption;
        txtStaffName.PlaceholderText = "VD: Nguyễn Văn A";
        txtStaffName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        lblStaffEmail.Text = "Email Đăng Nhập:";
        lblStaffEmail.Font = AppTheme.FontCaptionBold;
        lblStaffEmail.Location = new Point(16, 100);
        lblStaffEmail.AutoSize = true;

        txtStaffEmail.Location = new Point(16, 122);
        txtStaffEmail.Size = new Size(330, 27);
        txtStaffEmail.Font = AppTheme.FontCaption;
        txtStaffEmail.PlaceholderText = "staff@sportchain.vn";
        txtStaffEmail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        lblStaffPass.Text = "Mật Khẩu Khởi Tạo:";
        lblStaffPass.Font = AppTheme.FontCaptionBold;
        lblStaffPass.Location = new Point(16, 156);
        lblStaffPass.AutoSize = true;

        txtStaffPass.Location = new Point(16, 178);
        txtStaffPass.Size = new Size(330, 27);
        txtStaffPass.Font = AppTheme.FontCaption;
        txtStaffPass.UseSystemPasswordChar = true;
        txtStaffPass.PlaceholderText = "Tối thiểu 6 ký tự";
        txtStaffPass.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        lblStaffPhone.Text = "Số Điện Thoại:";
        lblStaffPhone.Font = AppTheme.FontCaptionBold;
        lblStaffPhone.Location = new Point(16, 212);
        lblStaffPhone.AutoSize = true;

        txtStaffPhone.Location = new Point(16, 234);
        txtStaffPhone.Size = new Size(330, 27);
        txtStaffPhone.Font = AppTheme.FontCaption;
        txtStaffPhone.PlaceholderText = "0988xxxxxx";
        txtStaffPhone.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        lblStaffRole.Text = "Vai Trò Bổ Nhiệm:";
        lblStaffRole.Font = AppTheme.FontCaptionBold;
        lblStaffRole.Location = new Point(16, 268);
        lblStaffRole.AutoSize = true;

        cboStaffRole.Location = new Point(16, 290);
        cboStaffRole.Size = new Size(330, 28);
        cboStaffRole.Font = AppTheme.FontCaption;
        cboStaffRole.DropDownStyle = ComboBoxStyle.DropDownList;
        cboStaffRole.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        lblStaffBranch.Text = "Chi Nhánh Làm Việc (*):";
        lblStaffBranch.Font = AppTheme.FontCaptionBold;
        lblStaffBranch.Location = new Point(16, 324);
        lblStaffBranch.AutoSize = true;

        cboStaffBranch.Location = new Point(16, 346);
        cboStaffBranch.Size = new Size(330, 28);
        cboStaffBranch.Font = AppTheme.FontCaption;
        cboStaffBranch.DropDownStyle = ComboBoxStyle.DropDownList;
        cboStaffBranch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        btnCreateStaff.Text = "Cấp Tài Khoản Mới";
        btnCreateStaff.Icon = IconChar.UserPlus;
        btnCreateStaff.ButtonType = AppButtonType.Primary;
        btnCreateStaff.Location = new Point(16, 390);
        btnCreateStaff.Size = new Size(330, 42);
        btnCreateStaff.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        pnlCreateStaff.Controls.Add(lblCreateStaffTitle);
        pnlCreateStaff.Controls.Add(lblStaffName);
        pnlCreateStaff.Controls.Add(txtStaffName);
        pnlCreateStaff.Controls.Add(lblStaffEmail);
        pnlCreateStaff.Controls.Add(txtStaffEmail);
        pnlCreateStaff.Controls.Add(lblStaffPass);
        pnlCreateStaff.Controls.Add(txtStaffPass);
        pnlCreateStaff.Controls.Add(lblStaffPhone);
        pnlCreateStaff.Controls.Add(txtStaffPhone);
        pnlCreateStaff.Controls.Add(lblStaffRole);
        pnlCreateStaff.Controls.Add(cboStaffRole);
        pnlCreateStaff.Controls.Add(lblStaffBranch);
        pnlCreateStaff.Controls.Add(cboStaffBranch);
        pnlCreateStaff.Controls.Add(btnCreateStaff);

        // 
        // tabBranches
        // 
        tabBranches.Text = "🏢 Duyệt & Quản Lý Chi Nhánh";
        tabBranches.BackColor = AppTheme.MainBg;
        tabBranches.Controls.Add(dgvBranches);
        tabBranches.Controls.Add(pnlBranchActions);

        dgvBranches.Dock = DockStyle.Fill;
        dgvBranches.BackgroundColor = AppTheme.CardBg;
        dgvBranches.BorderStyle = BorderStyle.None;
        dgvBranches.RowHeadersVisible = false;
        dgvBranches.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvBranches.MultiSelect = false;
        dgvBranches.AllowUserToAddRows = false;
        dgvBranches.ReadOnly = true;
        dgvBranches.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvBranches.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.SidebarDarker;
        dgvBranches.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvBranches.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontCaptionBold;
        dgvBranches.ColumnHeadersHeight = 42;
        dgvBranches.RowTemplate.Height = 40;
        dgvBranches.GridColor = AppTheme.Border;

        pnlBranchActions.Dock = DockStyle.Bottom;
        pnlBranchActions.Height = 60;
        pnlBranchActions.Padding = new Padding(24, 10, 24, 10);
        pnlBranchActions.BackColor = AppTheme.CardBg;
        pnlBranchActions.Paint += (s, e) =>
        {
            using var pen = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawLine(pen, 0, 0, pnlBranchActions.Width, 0);
        };

        btnApproveBranch.Text = "Phê Duyệt Hoạt Động";
        btnApproveBranch.Icon = IconChar.CheckCircle;
        btnApproveBranch.ButtonType = AppButtonType.Primary;
        btnApproveBranch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnApproveBranch.Location = new Point(600, 10);
        btnApproveBranch.Size = new Size(200, 40);

        btnToggleBranchStatus.Text = "Khóa / Mở Cơ Sở";
        btnToggleBranchStatus.Icon = IconChar.PowerOff;
        btnToggleBranchStatus.ButtonType = AppButtonType.Secondary;
        btnToggleBranchStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnToggleBranchStatus.Location = new Point(810, 10);
        btnToggleBranchStatus.Size = new Size(180, 40);

        pnlBranchActions.Controls.Add(btnApproveBranch);
        pnlBranchActions.Controls.Add(btnToggleBranchStatus);

        // 
        // tabAuditLogs
        // 
        tabAuditLogs.Text = "📜 Nhật Ký Kiểm Toán (Audit Logs)";
        tabAuditLogs.BackColor = AppTheme.MainBg;
        tabAuditLogs.Controls.Add(dgvAuditLogs);
        tabAuditLogs.Controls.Add(pnlAuditActions);

        dgvAuditLogs.Dock = DockStyle.Fill;
        dgvAuditLogs.BackgroundColor = AppTheme.CardBg;
        dgvAuditLogs.BorderStyle = BorderStyle.None;
        dgvAuditLogs.RowHeadersVisible = false;
        dgvAuditLogs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvAuditLogs.MultiSelect = false;
        dgvAuditLogs.AllowUserToAddRows = false;
        dgvAuditLogs.ReadOnly = true;
        dgvAuditLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvAuditLogs.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.SidebarDarker;
        dgvAuditLogs.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvAuditLogs.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontCaptionBold;
        dgvAuditLogs.ColumnHeadersHeight = 42;
        dgvAuditLogs.RowTemplate.Height = 38;
        dgvAuditLogs.GridColor = AppTheme.Border;

        pnlAuditActions.Dock = DockStyle.Top;
        pnlAuditActions.Height = 52;
        pnlAuditActions.Padding = new Padding(24, 10, 24, 10);
        pnlAuditActions.BackColor = AppTheme.CardBg;
        pnlAuditActions.Paint += (s, e) =>
        {
            using var pen = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawLine(pen, 0, pnlAuditActions.Height - 1, pnlAuditActions.Width, pnlAuditActions.Height - 1);
        };

        lblAuditHint.Text = "Hiển thị số lượng nhật ký gần nhất:";
        lblAuditHint.Font = AppTheme.FontCaption;
        lblAuditHint.ForeColor = AppTheme.TextMuted;
        lblAuditHint.Location = new Point(24, 15);
        lblAuditHint.AutoSize = true;

        nudLogLimit.Location = new Point(240, 12);
        nudLogLimit.Size = new Size(80, 27);
        nudLogLimit.Minimum = 10;
        nudLogLimit.Maximum = 500;
        nudLogLimit.Value = 50;
        nudLogLimit.Font = AppTheme.FontCaption;

        btnLoadLogs.Text = "Tải Nhật Ký";
        btnLoadLogs.Icon = IconChar.Rotate;
        btnLoadLogs.ButtonType = AppButtonType.Secondary;
        btnLoadLogs.Location = new Point(330, 9);
        btnLoadLogs.Size = new Size(130, 34);

        pnlAuditActions.Controls.Add(lblAuditHint);
        pnlAuditActions.Controls.Add(nudLogLimit);
        pnlAuditActions.Controls.Add(btnLoadLogs);

        // Root
        Controls.Add(tabAdmin);
        Controls.Add(pnlHeader);
        Size = new Size(1040, 680);
        Dock = DockStyle.Fill;
        BackColor = AppTheme.MainBg;

        ResumeLayout(false);
    }
}

