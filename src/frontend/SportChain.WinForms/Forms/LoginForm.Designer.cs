using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.Forms;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null;

    // Layout
    private Panel pnlLeft;
    private Panel pnlRight;

    // Left Banner Controls
    private IconPictureBox picBrandLogo;
    private Label lblBrandTitle;
    private Label lblBrandSubtitle;
    private Panel pnlFeatures;

#if DEBUG
    private CardPanel grpQuickLogin;
    private Label lblQuickLoginTitle;
    private AppButton btnQuickAdmin;
    private AppButton btnQuickManager;
    private AppButton btnQuickReceptionist;
    private AppButton btnQuickCustomer;
#endif

    // Right Form Controls
    private Panel pnlTabs;
    private Button btnTabLogin;
    private Button btnTabRegister;
    private Panel pnlTabIndicator;

    // Login Controls
    private Panel pnlLoginBody;
    private Label lblLoginHeader;
    private Label lblLoginSubHeader;
    private Label lblLoginEmail;
    private AppTextBox txtEmail;
    private Label lblLoginPassword;
    private AppTextBox txtPassword;
    private CheckBox chkRememberMe;
    private LinkLabel lnkForgotPassword;
    private AppButton btnLogin;
    private Label lblLoginStatus;

    // Register Controls
    private Panel pnlRegisterBody;
    private Label lblRegisterHeader;
    private Label lblRegisterSubHeader;
    private Label lblRegFullName;
    private AppTextBox txtRegFullName;
    private Label lblRegPhone;
    private AppTextBox txtRegPhone;
    private Label lblRegEmail;
    private AppTextBox txtRegEmail;
    private Label lblRegPassword;
    private AppTextBox txtRegPassword;
    private Label lblRegConfirmPassword;
    private AppTextBox txtRegConfirmPassword;
    private AppButton btnRegister;
    private Label lblRegStatus;

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
        pnlLeft = new Panel();
        picBrandLogo = new IconPictureBox();
        lblBrandTitle = new Label();
        lblBrandSubtitle = new Label();
        pnlFeatures = new Panel();

#if DEBUG
        grpQuickLogin = new CardPanel();
        lblQuickLoginTitle = new Label();
        btnQuickAdmin = new AppButton();
        btnQuickManager = new AppButton();
        btnQuickReceptionist = new AppButton();
        btnQuickCustomer = new AppButton();
#endif

        pnlRight = new Panel();
        pnlTabs = new Panel();
        btnTabLogin = new Button();
        btnTabRegister = new Button();
        pnlTabIndicator = new Panel();

        pnlLoginBody = new Panel();
        lblLoginHeader = new Label();
        lblLoginSubHeader = new Label();
        lblLoginEmail = new Label();
        txtEmail = new AppTextBox();
        lblLoginPassword = new Label();
        txtPassword = new AppTextBox();
        chkRememberMe = new CheckBox();
        lnkForgotPassword = new LinkLabel();
        btnLogin = new AppButton();
        lblLoginStatus = new Label();

        pnlRegisterBody = new Panel();
        lblRegisterHeader = new Label();
        lblRegisterSubHeader = new Label();
        lblRegFullName = new Label();
        txtRegFullName = new AppTextBox();
        lblRegPhone = new Label();
        txtRegPhone = new AppTextBox();
        lblRegEmail = new Label();
        txtRegEmail = new AppTextBox();
        lblRegPassword = new Label();
        txtRegPassword = new AppTextBox();
        lblRegConfirmPassword = new Label();
        txtRegConfirmPassword = new AppTextBox();
        btnRegister = new AppButton();
        lblRegStatus = new Label();

        SuspendLayout();

        // 
        // LoginForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1000, 640);
        MinimumSize = new Size(920, 580);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "SportChain VN - Đăng Nhập Hệ Thống";
        BackColor = AppTheme.MainBg;
        Font = AppTheme.FontBody;

        // 
        // pnlLeft (Brand Banner)
        // 
        pnlLeft.Dock = DockStyle.Left;
        pnlLeft.Width = 380;
        pnlLeft.BackColor = AppTheme.SidebarBg;
        pnlLeft.Padding = new Padding(32, 40, 32, 24);

        picBrandLogo.IconChar = IconChar.Bolt;
        picBrandLogo.IconColor = AppTheme.Primary;
        picBrandLogo.IconSize = 40;
        picBrandLogo.Size = new Size(40, 40);
        picBrandLogo.Location = new Point(32, 36);
        picBrandLogo.BackColor = Color.Transparent;

        lblBrandTitle.Text = "SPORTCHAIN";
        lblBrandTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblBrandTitle.ForeColor = Color.White;
        lblBrandTitle.Location = new Point(78, 38);
        lblBrandTitle.AutoSize = true;

        lblBrandSubtitle.Text = "HỆ THỐNG QUẢN LÝ CHUỖI SÂN THỂ THAO";
        lblBrandSubtitle.Font = AppTheme.FontSmallBold;
        lblBrandSubtitle.ForeColor = AppTheme.Primary;
        lblBrandSubtitle.Location = new Point(32, 82);
        lblBrandSubtitle.AutoSize = true;

        pnlFeatures.Location = new Point(32, 114);
        pnlFeatures.Size = new Size(316, 170);
        pnlFeatures.BackColor = Color.Transparent;

        AddFeatureItem(pnlFeatures, 0, IconChar.CalendarCheck, "Đặt sân trực tuyến 24/7 theo thời gian thực");
        AddFeatureItem(pnlFeatures, 34, IconChar.Qrcode, "Check-in thông minh qua mã QR tiện lợi");
        AddFeatureItem(pnlFeatures, 68, IconChar.Coins, "Thanh toán cọc bảo đảm & minh bạch");
        AddFeatureItem(pnlFeatures, 102, IconChar.ChartPie, "Báo cáo doanh thu & đối soát tự động");
        AddFeatureItem(pnlFeatures, 136, IconChar.Building, "Mô hình quản lý đa chi nhánh linh hoạt");

#if DEBUG
        // Quick Login Card
        grpQuickLogin.Location = new Point(24, 300);
        grpQuickLogin.Size = new Size(332, 215);
        grpQuickLogin.BackColor = AppTheme.SidebarDarker;
        grpQuickLogin.BorderColor = Color.FromArgb(40, 50, 70);

        lblQuickLoginTitle.Text = "⚡ ĐĂNG NHẬP NHANH (MÔI TRƯỜNG TEST)";
        lblQuickLoginTitle.Font = AppTheme.FontSmallBold;
        lblQuickLoginTitle.ForeColor = AppTheme.Accent;
        lblQuickLoginTitle.Location = new Point(14, 12);
        lblQuickLoginTitle.AutoSize = true;

        btnQuickAdmin.Text = "SuperAdmin (Toàn hệ thống)";
        btnQuickAdmin.ButtonType = AppButtonType.Primary;
        btnQuickAdmin.Icon = IconChar.UserShield;
        btnQuickAdmin.Location = new Point(14, 38);
        btnQuickAdmin.Size = new Size(304, 36);

        btnQuickManager.Text = "Quản Lý Chi Nhánh (Cầu Giấy)";
        btnQuickManager.ButtonType = AppButtonType.Secondary;
        btnQuickManager.Icon = IconChar.UserTie;
        btnQuickManager.Location = new Point(14, 80);
        btnQuickManager.Size = new Size(304, 36);

        btnQuickReceptionist.Text = "Lễ Tân POS (Cầu Giấy)";
        btnQuickReceptionist.ButtonType = AppButtonType.Secondary;
        btnQuickReceptionist.Icon = IconChar.Headset;
        btnQuickReceptionist.Location = new Point(14, 122);
        btnQuickReceptionist.Size = new Size(304, 36);

        btnQuickCustomer.Text = "Khách Hàng (Đặt sân cá nhân)";
        btnQuickCustomer.ButtonType = AppButtonType.Secondary;
        btnQuickCustomer.Icon = IconChar.User;
        btnQuickCustomer.Location = new Point(14, 164);
        btnQuickCustomer.Size = new Size(304, 36);

        grpQuickLogin.Controls.Add(lblQuickLoginTitle);
        grpQuickLogin.Controls.Add(btnQuickAdmin);
        grpQuickLogin.Controls.Add(btnQuickManager);
        grpQuickLogin.Controls.Add(btnQuickReceptionist);
        grpQuickLogin.Controls.Add(btnQuickCustomer);

        pnlLeft.Controls.Add(grpQuickLogin);
#endif

        pnlLeft.Controls.Add(picBrandLogo);
        pnlLeft.Controls.Add(lblBrandTitle);
        pnlLeft.Controls.Add(lblBrandSubtitle);
        pnlLeft.Controls.Add(pnlFeatures);

        // 
        // pnlRight (Form Card Container)
        // 
        pnlRight.Dock = DockStyle.Fill;
        pnlRight.BackColor = AppTheme.MainBg;
        pnlRight.Padding = new Padding(48, 32, 48, 24);

        // Tabs Header
        pnlTabs.Dock = DockStyle.Top;
        pnlTabs.Height = 48;
        pnlTabs.BackColor = Color.Transparent;

        btnTabLogin.Text = "ĐĂNG NHẬP";
        btnTabLogin.Font = AppTheme.FontBodyBold;
        btnTabLogin.ForeColor = AppTheme.Primary;
        btnTabLogin.FlatStyle = FlatStyle.Flat;
        btnTabLogin.FlatAppearance.BorderSize = 0;
        btnTabLogin.Size = new Size(130, 42);
        btnTabLogin.Location = new Point(0, 0);
        btnTabLogin.Cursor = Cursors.Hand;

        btnTabRegister.Text = "ĐĂNG KÝ";
        btnTabRegister.Font = AppTheme.FontBodyBold;
        btnTabRegister.ForeColor = AppTheme.TextMuted;
        btnTabRegister.FlatStyle = FlatStyle.Flat;
        btnTabRegister.FlatAppearance.BorderSize = 0;
        btnTabRegister.Size = new Size(130, 42);
        btnTabRegister.Location = new Point(135, 0);
        btnTabRegister.Cursor = Cursors.Hand;

        pnlTabIndicator.Size = new Size(130, 3);
        pnlTabIndicator.Location = new Point(0, 44);
        pnlTabIndicator.BackColor = AppTheme.Primary;

        pnlTabs.Controls.Add(btnTabLogin);
        pnlTabs.Controls.Add(btnTabRegister);
        pnlTabs.Controls.Add(pnlTabIndicator);

        // 
        // pnlLoginBody
        // 
        pnlLoginBody.Dock = DockStyle.Fill;
        pnlLoginBody.BackColor = Color.Transparent;
        pnlLoginBody.Padding = new Padding(0, 16, 0, 0);

        lblLoginHeader.Text = "Chào mừng trở lại!";
        lblLoginHeader.Font = AppTheme.FontScreenTitle;
        lblLoginHeader.ForeColor = AppTheme.TextMain;
        lblLoginHeader.Location = new Point(0, 12);
        lblLoginHeader.AutoSize = true;

        lblLoginSubHeader.Text = "Vui lòng nhập tài khoản để truy cập hệ thống SportChain VN.";
        lblLoginSubHeader.Font = AppTheme.FontCaption;
        lblLoginSubHeader.ForeColor = AppTheme.TextMuted;
        lblLoginSubHeader.Location = new Point(2, 42);
        lblLoginSubHeader.AutoSize = true;

        lblLoginEmail.Text = "Email Đăng Nhập:";
        lblLoginEmail.Font = AppTheme.FontCaptionBold;
        lblLoginEmail.ForeColor = AppTheme.TextMain;
        lblLoginEmail.Location = new Point(0, 78);
        lblLoginEmail.AutoSize = true;

        txtEmail.PlaceholderText = "name@sportchain.vn hoặc email của bạn";
        txtEmail.LeftIcon = IconChar.Envelope;
        txtEmail.Location = new Point(0, 100);
        txtEmail.Size = new Size(460, 40);

        lblLoginPassword.Text = "Mật Khẩu:";
        lblLoginPassword.Font = AppTheme.FontCaptionBold;
        lblLoginPassword.ForeColor = AppTheme.TextMain;
        lblLoginPassword.Location = new Point(0, 150);
        lblLoginPassword.AutoSize = true;

        txtPassword.PlaceholderText = "Nhập mật khẩu tài khoản";
        txtPassword.LeftIcon = IconChar.Lock;
        txtPassword.UseSystemPasswordChar = true;
        txtPassword.Location = new Point(0, 172);
        txtPassword.Size = new Size(460, 40);

        chkRememberMe.Text = "Ghi nhớ phiên đăng nhập";
        chkRememberMe.Font = AppTheme.FontCaption;
        chkRememberMe.ForeColor = AppTheme.TextMuted;
        chkRememberMe.Location = new Point(4, 224);
        chkRememberMe.AutoSize = true;
        chkRememberMe.Checked = true;

        lnkForgotPassword.Text = "Quên mật khẩu?";
        lnkForgotPassword.Font = AppTheme.FontCaptionBold;
        lnkForgotPassword.LinkColor = AppTheme.Primary;
        lnkForgotPassword.Location = new Point(340, 224);
        lnkForgotPassword.AutoSize = true;
        lnkForgotPassword.Cursor = Cursors.Hand;
        lnkForgotPassword.LinkClicked += (s, e) =>
        {
            MessageBox.Show("Vui lòng liên hệ Quản trị viên cơ sở hoặc hotline 1900-SPORTCHAIN để đặt lại mật khẩu.",
                "Hỗ Trợ Khôi Phục Mật Khẩu", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        btnLogin.Text = "ĐĂNG NHẬP NGAY";
        btnLogin.ButtonType = AppButtonType.Primary;
        btnLogin.Icon = IconChar.SignInAlt;
        btnLogin.Location = new Point(0, 260);
        btnLogin.Size = new Size(460, 44);

        lblLoginStatus.Location = new Point(0, 314);
        lblLoginStatus.Size = new Size(460, 30);
        lblLoginStatus.Font = AppTheme.FontCaptionBold;
        lblLoginStatus.ForeColor = AppTheme.Danger;

        pnlLoginBody.Controls.Add(lblLoginHeader);
        pnlLoginBody.Controls.Add(lblLoginSubHeader);
        pnlLoginBody.Controls.Add(lblLoginEmail);
        pnlLoginBody.Controls.Add(txtEmail);
        pnlLoginBody.Controls.Add(lblLoginPassword);
        pnlLoginBody.Controls.Add(txtPassword);
        pnlLoginBody.Controls.Add(chkRememberMe);
        pnlLoginBody.Controls.Add(lnkForgotPassword);
        pnlLoginBody.Controls.Add(btnLogin);
        pnlLoginBody.Controls.Add(lblLoginStatus);

        // 
        // pnlRegisterBody
        // 
        pnlRegisterBody.Dock = DockStyle.Fill;
        pnlRegisterBody.BackColor = Color.Transparent;
        pnlRegisterBody.Padding = new Padding(0, 16, 0, 0);
        pnlRegisterBody.Visible = false;
        pnlRegisterBody.AutoScroll = true;

        lblRegisterHeader.Text = "Tạo tài khoản khách hàng";
        lblRegisterHeader.Font = AppTheme.FontScreenTitle;
        lblRegisterHeader.ForeColor = AppTheme.TextMain;
        lblRegisterHeader.Location = new Point(0, 6);
        lblRegisterHeader.AutoSize = true;

        lblRegisterSubHeader.Text = "Đặt sân thể thao trực tuyến, nhận ưu đãi giờ vàng và mã vé QR tiện lợi.";
        lblRegisterSubHeader.Font = AppTheme.FontCaption;
        lblRegisterSubHeader.ForeColor = AppTheme.TextMuted;
        lblRegisterSubHeader.Location = new Point(2, 34);
        lblRegisterSubHeader.AutoSize = true;

        // Họ và Tên
        lblRegFullName.Text = "Họ và Tên:";
        lblRegFullName.Font = AppTheme.FontCaptionBold;
        lblRegFullName.ForeColor = AppTheme.TextMain;
        lblRegFullName.Location = new Point(0, 64);
        lblRegFullName.AutoSize = true;

        txtRegFullName.PlaceholderText = "VD: Nguyễn Văn A";
        txtRegFullName.LeftIcon = IconChar.User;
        txtRegFullName.Location = new Point(0, 86);
        txtRegFullName.Size = new Size(460, 40);

        // Số Điện Thoại
        lblRegPhone.Text = "Số Điện Thoại:";
        lblRegPhone.Font = AppTheme.FontCaptionBold;
        lblRegPhone.ForeColor = AppTheme.TextMain;
        lblRegPhone.Location = new Point(0, 134);
        lblRegPhone.AutoSize = true;

        txtRegPhone.PlaceholderText = "09xxxxxxxx";
        txtRegPhone.LeftIcon = IconChar.Phone;
        txtRegPhone.Location = new Point(0, 156);
        txtRegPhone.Size = new Size(460, 40);

        // Địa Chỉ Email
        lblRegEmail.Text = "Địa Chỉ Email:";
        lblRegEmail.Font = AppTheme.FontCaptionBold;
        lblRegEmail.ForeColor = AppTheme.TextMain;
        lblRegEmail.Location = new Point(0, 204);
        lblRegEmail.AutoSize = true;

        txtRegEmail.PlaceholderText = "name@example.com";
        txtRegEmail.LeftIcon = IconChar.Envelope;
        txtRegEmail.Location = new Point(0, 226);
        txtRegEmail.Size = new Size(460, 40);

        // Mật Khẩu
        lblRegPassword.Text = "Mật Khẩu:";
        lblRegPassword.Font = AppTheme.FontCaptionBold;
        lblRegPassword.ForeColor = AppTheme.TextMain;
        lblRegPassword.Location = new Point(0, 274);
        lblRegPassword.AutoSize = true;

        txtRegPassword.PlaceholderText = "Tối thiểu 6 ký tự";
        txtRegPassword.LeftIcon = IconChar.Lock;
        txtRegPassword.UseSystemPasswordChar = true;
        txtRegPassword.Location = new Point(0, 296);
        txtRegPassword.Size = new Size(225, 40);

        // Xác Nhận Mật Khẩu
        lblRegConfirmPassword.Text = "Xác Nhận Mật Khẩu:";
        lblRegConfirmPassword.Font = AppTheme.FontCaptionBold;
        lblRegConfirmPassword.ForeColor = AppTheme.TextMain;
        lblRegConfirmPassword.Location = new Point(235, 274);
        lblRegConfirmPassword.AutoSize = true;

        txtRegConfirmPassword.PlaceholderText = "Nhập lại mật khẩu";
        txtRegConfirmPassword.LeftIcon = IconChar.CheckDouble;
        txtRegConfirmPassword.UseSystemPasswordChar = true;
        txtRegConfirmPassword.Location = new Point(235, 296);
        txtRegConfirmPassword.Size = new Size(225, 40);

        // Nút Đăng Ký
        btnRegister.Text = "TẠO TÀI KHOẢN NGAY";
        btnRegister.ButtonType = AppButtonType.Accent;
        btnRegister.Icon = IconChar.UserPlus;
        btnRegister.Location = new Point(0, 352);
        btnRegister.Size = new Size(460, 44);

        lblRegStatus.Location = new Point(0, 404);
        lblRegStatus.Size = new Size(460, 26);
        lblRegStatus.Font = AppTheme.FontCaptionBold;
        lblRegStatus.ForeColor = AppTheme.Danger;

        pnlRegisterBody.Controls.Add(lblRegisterHeader);
        pnlRegisterBody.Controls.Add(lblRegisterSubHeader);
        pnlRegisterBody.Controls.Add(lblRegFullName);
        pnlRegisterBody.Controls.Add(txtRegFullName);
        pnlRegisterBody.Controls.Add(lblRegPhone);
        pnlRegisterBody.Controls.Add(txtRegPhone);
        pnlRegisterBody.Controls.Add(lblRegEmail);
        pnlRegisterBody.Controls.Add(txtRegEmail);
        pnlRegisterBody.Controls.Add(lblRegPassword);
        pnlRegisterBody.Controls.Add(txtRegPassword);
        pnlRegisterBody.Controls.Add(lblRegConfirmPassword);
        pnlRegisterBody.Controls.Add(txtRegConfirmPassword);
        pnlRegisterBody.Controls.Add(btnRegister);
        pnlRegisterBody.Controls.Add(lblRegStatus);

        pnlRight.Controls.Add(pnlLoginBody);
        pnlRight.Controls.Add(pnlRegisterBody);
        pnlRight.Controls.Add(pnlTabs);

        Controls.Add(pnlRight);
        Controls.Add(pnlLeft);

        ResumeLayout(false);
    }

    private void AddFeatureItem(Panel container, int y, IconChar icon, string text)
    {
        var pic = new IconPictureBox
        {
            IconChar = icon,
            IconColor = AppTheme.Accent,
            IconSize = 18,
            Size = new Size(20, 20),
            Location = new Point(0, y + 2),
            BackColor = Color.Transparent
        };

        var lbl = new Label
        {
            Text = text,
            Font = AppTheme.FontCaption,
            ForeColor = AppTheme.SidebarText,
            Location = new Point(28, y),
            AutoSize = true
        };

        container.Controls.Add(pic);
        container.Controls.Add(lbl);
    }
}
