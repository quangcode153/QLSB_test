using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    // Sidebar Controls
    private Panel pnlSidebar;
    private Panel pnlBrandHeader;
    private IconPictureBox picBrandLogo;
    private Label lblBrandName;
    private Label lblBrandSub;
    private Panel pnlUserInfo;
    private Panel pnlUserAvatarCircle;
    private Label lblUserAvatarInitials;
    private Label lblUserName;
    private StatusBadge badgeUserRole;
    private Label lblUserBranch;
    private Panel pnlNavMenu;

    // Sidebar Navigation Buttons
    private SidebarButton btnNavMatrix;
    private SidebarButton btnNavMyBookings;
    private SidebarButton btnNavPos;
    private SidebarButton btnNavBranch;
    private SidebarButton btnNavUsers;

    private Panel pnlSidebarBottom;
    private SidebarButton btnNavLogout;
    private Label lblVersion;

    // TopBar Controls
    private Panel pnlTopBar;
    private IconButton btnToggleSidebar;
    private Label lblBreadcrumb;
    private Label lblViewTitle;

    private Label lblBranchSelectLabel;
    private ComboBox cboBranchSelect;
    private StatusBadge badgeSignalR;
    private Label lblClock;
    private IconButton btnNotifications;
    private System.Windows.Forms.Timer timerClock;

    // Content Host
    private Panel pnlContentHost;
    private LoadingOverlay _loadingOverlay;

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
        components = new System.ComponentModel.Container();

        pnlSidebar = new Panel();
        pnlBrandHeader = new Panel();
        picBrandLogo = new IconPictureBox();
        lblBrandName = new Label();
        lblBrandSub = new Label();
        pnlUserInfo = new Panel();
        pnlUserAvatarCircle = new Panel();
        lblUserAvatarInitials = new Label();
        lblUserName = new Label();
        badgeUserRole = new StatusBadge();
        lblUserBranch = new Label();
        pnlNavMenu = new Panel();

        btnNavMatrix = new SidebarButton();
        btnNavMyBookings = new SidebarButton();
        btnNavPos = new SidebarButton();
        btnNavBranch = new SidebarButton();
        btnNavUsers = new SidebarButton();

        pnlSidebarBottom = new Panel();
        btnNavLogout = new SidebarButton();
        lblVersion = new Label();

        pnlTopBar = new Panel();
        btnToggleSidebar = new IconButton();
        lblBreadcrumb = new Label();
        lblViewTitle = new Label();

        lblBranchSelectLabel = new Label();
        cboBranchSelect = new ComboBox();
        badgeSignalR = new StatusBadge();
        lblClock = new Label();
        btnNotifications = new IconButton();
        timerClock = new System.Windows.Forms.Timer(components);

        pnlContentHost = new Panel();

        SuspendLayout();

        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1366, 768);
        MinimumSize = new Size(1200, 700);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "SportChain VN - Hệ Thống Quản Lý Chuỗi Sân Thể Thao";
        BackColor = AppTheme.MainBg;
        Font = AppTheme.FontBody;

        // 
        // pnlSidebar
        // 
        pnlSidebar.Dock = DockStyle.Left;
        pnlSidebar.Width = 260;
        pnlSidebar.BackColor = AppTheme.SidebarBg;
        pnlSidebar.Padding = new Padding(0);

        // pnlBrandHeader
        pnlBrandHeader.Dock = DockStyle.Top;
        pnlBrandHeader.Height = 72;
        pnlBrandHeader.BackColor = AppTheme.SidebarDarker;
        pnlBrandHeader.Padding = new Padding(16, 16, 16, 10);

        picBrandLogo.IconChar = IconChar.Bolt;
        picBrandLogo.IconColor = AppTheme.Primary;
        picBrandLogo.IconSize = 28;
        picBrandLogo.Size = new Size(28, 28);
        picBrandLogo.Location = new Point(16, 20);
        picBrandLogo.BackColor = Color.Transparent;

        lblBrandName.Text = "SPORTCHAIN";
        lblBrandName.ForeColor = Color.White;
        lblBrandName.Font = new Font("Segoe UI", 13.5F, FontStyle.Bold);
        lblBrandName.Location = new Point(48, 16);
        lblBrandName.AutoSize = true;

        lblBrandSub.Text = "HỆ THỐNG QUẢN LÝ SÂN THỂ THAO";
        lblBrandSub.ForeColor = AppTheme.Primary;
        lblBrandSub.Font = new Font("Segoe UI", 7F, FontStyle.Bold);
        lblBrandSub.Location = new Point(50, 42);
        lblBrandSub.AutoSize = true;

        pnlBrandHeader.Controls.Add(picBrandLogo);
        pnlBrandHeader.Controls.Add(lblBrandName);
        pnlBrandHeader.Controls.Add(lblBrandSub);

        // pnlUserInfo
        pnlUserInfo.Dock = DockStyle.Top;
        pnlUserInfo.Height = 88;
        pnlUserInfo.BackColor = AppTheme.SidebarBg;
        pnlUserInfo.Padding = new Padding(16, 12, 16, 12);

        pnlUserAvatarCircle.Size = new Size(42, 42);
        pnlUserAvatarCircle.Location = new Point(16, 16);
        pnlUserAvatarCircle.BackColor = AppTheme.Primary;
        pnlUserAvatarCircle.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(AppTheme.Primary);
            e.Graphics.FillEllipse(brush, 0, 0, 41, 41);
        };

        lblUserAvatarInitials.Text = "US";
        lblUserAvatarInitials.Font = AppTheme.FontCaptionBold;
        lblUserAvatarInitials.ForeColor = Color.White;
        lblUserAvatarInitials.Dock = DockStyle.Fill;
        lblUserAvatarInitials.TextAlign = ContentAlignment.MiddleCenter;
        pnlUserAvatarCircle.Controls.Add(lblUserAvatarInitials);

        lblUserName.Text = "Người Dùng";
        lblUserName.ForeColor = Color.White;
        lblUserName.Font = AppTheme.FontCaptionBold;
        lblUserName.Location = new Point(68, 14);
        lblUserName.AutoSize = true;

        badgeUserRole.Type = BadgeType.Success;
        badgeUserRole.Text = "KHÁCH HÀNG";
        badgeUserRole.Size = new Size(110, 22);
        badgeUserRole.Location = new Point(68, 36);

        lblUserBranch.Text = "Cơ sở: SportChain Cầu Giấy";
        lblUserBranch.ForeColor = AppTheme.SidebarTextMuted;
        lblUserBranch.Font = AppTheme.FontSmall;
        lblUserBranch.Location = new Point(68, 62);
        lblUserBranch.AutoSize = true;

        pnlUserInfo.Controls.Add(pnlUserAvatarCircle);
        pnlUserInfo.Controls.Add(lblUserName);
        pnlUserInfo.Controls.Add(badgeUserRole);
        pnlUserInfo.Controls.Add(lblUserBranch);

        // pnlNavMenu
        pnlNavMenu.Dock = DockStyle.Fill;
        pnlNavMenu.BackColor = AppTheme.SidebarBg;
        pnlNavMenu.Padding = new Padding(10, 8, 10, 8);

        ConfigureNavButton(btnNavMatrix, "Ma Trận Đặt Sân", IconChar.CalendarAlt, 0);
        ConfigureNavButton(btnNavMyBookings, "Đơn Đặt Của Tôi", IconChar.TicketAlt, 1);
        ConfigureNavButton(btnNavPos, "Quầy Check-in & POS", IconChar.CashRegister, 2);
        ConfigureNavButton(btnNavBranch, "Báo Cáo Chi Nhánh", IconChar.ChartBar, 3);
        ConfigureNavButton(btnNavUsers, "Quản Trị Người Dùng", IconChar.UsersCog, 4);

        pnlNavMenu.Controls.Add(btnNavMatrix);
        pnlNavMenu.Controls.Add(btnNavMyBookings);
        pnlNavMenu.Controls.Add(btnNavPos);
        pnlNavMenu.Controls.Add(btnNavBranch);
        pnlNavMenu.Controls.Add(btnNavUsers);

        // pnlSidebarBottom
        pnlSidebarBottom.Dock = DockStyle.Bottom;
        pnlSidebarBottom.Height = 84;
        pnlSidebarBottom.BackColor = AppTheme.SidebarBg;
        pnlSidebarBottom.Padding = new Padding(10, 4, 10, 8);

        btnNavLogout.Text = "Đăng Xuất";
        btnNavLogout.Icon = IconChar.SignOutAlt;
        btnNavLogout.Location = new Point(10, 6);
        btnNavLogout.Size = new Size(240, 40);

        lblVersion.Text = "SportChain VN v2.0 (.NET 8)";
        lblVersion.Dock = DockStyle.Bottom;
        lblVersion.Height = 24;
        lblVersion.ForeColor = AppTheme.SidebarTextMuted;
        lblVersion.Font = AppTheme.FontSmall;
        lblVersion.TextAlign = ContentAlignment.MiddleCenter;

        pnlSidebarBottom.Controls.Add(btnNavLogout);
        pnlSidebarBottom.Controls.Add(lblVersion);

        pnlSidebar.Controls.Add(pnlNavMenu);
        pnlSidebar.Controls.Add(pnlUserInfo);
        pnlSidebar.Controls.Add(pnlBrandHeader);
        pnlSidebar.Controls.Add(pnlSidebarBottom);

        // 
        // pnlTopBar (Pure White, 64px, 1px Border)
        // 
        pnlTopBar.Dock = DockStyle.Top;
        pnlTopBar.Height = 64;
        pnlTopBar.BackColor = AppTheme.TopbarBg;
        pnlTopBar.Padding = new Padding(16, 12, 24, 12);
        pnlTopBar.Paint += (s, e) =>
        {
            // Vẽ 1px viền dưới #E2E8F0
            using var pen = new Pen(AppTheme.TopbarBorder, 1);
            e.Graphics.DrawLine(pen, 0, pnlTopBar.Height - 1, pnlTopBar.Width, pnlTopBar.Height - 1);
        };

        // Nút Hamburger toggle thu gọn sidebar
        btnToggleSidebar.IconChar = IconChar.Bars;
        btnToggleSidebar.IconSize = 18;
        btnToggleSidebar.IconColor = AppTheme.TextMain;
        btnToggleSidebar.FlatStyle = FlatStyle.Flat;
        btnToggleSidebar.FlatAppearance.BorderSize = 0;
        btnToggleSidebar.Size = new Size(36, 36);
        btnToggleSidebar.Location = new Point(12, 14);
        btnToggleSidebar.Cursor = Cursors.Hand;

        lblBreadcrumb.Text = "Hệ Thống /";
        lblBreadcrumb.Font = AppTheme.FontSmall;
        lblBreadcrumb.ForeColor = AppTheme.TextMuted;
        lblBreadcrumb.Location = new Point(56, 12);
        lblBreadcrumb.AutoSize = true;

        lblViewTitle.Text = "Ma Trận Đặt Sân & Khóa Ca Tự Động";
        lblViewTitle.ForeColor = AppTheme.TextMain;
        lblViewTitle.Font = AppTheme.FontCardTitle;
        lblViewTitle.Location = new Point(56, 30);
        lblViewTitle.AutoSize = true;
        lblViewTitle.UseMnemonic = false;

        lblBranchSelectLabel.Text = "Cơ sở:";
        lblBranchSelectLabel.ForeColor = AppTheme.TextMuted;
        lblBranchSelectLabel.Font = AppTheme.FontCaptionBold;
        lblBranchSelectLabel.Location = new Point(580, 22);
        lblBranchSelectLabel.AutoSize = true;
        lblBranchSelectLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        cboBranchSelect.DropDownStyle = ComboBoxStyle.DropDownList;
        cboBranchSelect.Location = new Point(630, 18);
        cboBranchSelect.Size = new Size(200, 28);
        cboBranchSelect.Font = AppTheme.FontCaption;
        cboBranchSelect.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        badgeSignalR.Type = BadgeType.Success;
        badgeSignalR.Text = "Realtime Sẵn Sàng";
        badgeSignalR.Size = new Size(150, 28);
        badgeSignalR.Location = new Point(845, 18);
        badgeSignalR.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        lblClock.Text = "12:00:00";
        lblClock.Font = AppTheme.FontCaptionBold;
        lblClock.ForeColor = AppTheme.TextMain;
        lblClock.Location = new Point(1005, 22);
        lblClock.AutoSize = true;
        lblClock.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        btnNotifications.IconChar = IconChar.Bell;
        btnNotifications.IconSize = 18;
        btnNotifications.IconColor = AppTheme.TextMuted;
        btnNotifications.FlatStyle = FlatStyle.Flat;
        btnNotifications.FlatAppearance.BorderSize = 0;
        btnNotifications.Size = new Size(36, 36);
        btnNotifications.Location = new Point(1085, 14);
        btnNotifications.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnNotifications.Cursor = Cursors.Hand;

        pnlTopBar.Controls.Add(btnToggleSidebar);
        pnlTopBar.Controls.Add(lblBreadcrumb);
        pnlTopBar.Controls.Add(lblViewTitle);
        pnlTopBar.Controls.Add(lblBranchSelectLabel);
        pnlTopBar.Controls.Add(cboBranchSelect);
        pnlTopBar.Controls.Add(badgeSignalR);
        pnlTopBar.Controls.Add(lblClock);
        pnlTopBar.Controls.Add(btnNotifications);

        // 
        // pnlContentHost
        // 
        pnlContentHost.Dock = DockStyle.Fill;
        pnlContentHost.BackColor = AppTheme.MainBg;

        // 
        // _loadingOverlay
        // 
        _loadingOverlay = new LoadingOverlay();
        _loadingOverlay.Dock = DockStyle.Fill;
        _loadingOverlay.Visible = false;

        Controls.Add(_loadingOverlay);
        Controls.Add(pnlContentHost);
        Controls.Add(pnlTopBar);
        Controls.Add(pnlSidebar);

        ResumeLayout(false);
    }

    private void ConfigureNavButton(SidebarButton btn, string text, IconChar icon, int index)
    {
        btn.Text = text;
        btn.Icon = icon;
        btn.Location = new Point(10, 8 + index * 48);
        btn.Size = new Size(240, 42);
        btn.Anchor = AnchorStyles.Top | AnchorStyles.Left;
    }
}
