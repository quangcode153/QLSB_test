using SportChain.Shared.DTOs.Auth;
using SportChain.WinForms.Services;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.Forms;

public partial class LoginForm : Form
{
    private bool _isBusy = false;

    public LoginForm()
    {
        InitializeComponent();
        SetupEvents();

        this.Load += (s, e) =>
        {
            this.WindowState = FormWindowState.Normal;
            this.Show();
            this.Activate();
            this.BringToFront();
            txtEmail.InnerTextBox.Focus();
        };
    }

    private void SetupEvents()
    {
        btnTabLogin.Click += (s, e) => SwitchTab(true);
        btnTabRegister.Click += (s, e) => SwitchTab(false);

        btnLogin.Click += async (s, e) => await HandleLoginAsync();
        btnRegister.Click += async (s, e) => await HandleRegisterAsync();

        // Validation khi rời ô
        txtEmail.InnerTextBox.Leave += (s, e) =>
        {
            var text = txtEmail.Text.Trim();
            if (!string.IsNullOrEmpty(text) && !text.Contains("@"))
            {
                txtEmail.ErrorMessage = "Định dạng email không hợp lệ.";
            }
            else
            {
                txtEmail.ErrorMessage = string.Empty;
            }
        };

        txtPassword.InnerTextBox.Leave += (s, e) =>
        {
            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                txtPassword.ErrorMessage = "Vui lòng nhập mật khẩu.";
            }
            else
            {
                txtPassword.ErrorMessage = string.Empty;
            }
        };

        // Phím Enter để đăng nhập hoặc đăng ký
        txtEmail.InnerTextBox.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await HandleLoginAsync(); };
        txtPassword.InnerTextBox.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await HandleLoginAsync(); };
        txtRegConfirmPassword.InnerTextBox.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await HandleRegisterAsync(); };

#if DEBUG
        btnQuickAdmin.Click += (s, e) => QuickLogin("admin@sportchain.vn");
        btnQuickManager.Click += (s, e) => QuickLogin("manager.caugiay@sportchain.vn");
        btnQuickReceptionist.Click += (s, e) => QuickLogin("letan.caugiay@sportchain.vn");
        btnQuickCustomer.Click += (s, e) => QuickLogin("khachhang@sportchain.vn");
#endif
    }

    private void SwitchTab(bool isLogin)
    {
        pnlLoginBody.Visible = isLogin;
        pnlRegisterBody.Visible = !isLogin;

        btnTabLogin.ForeColor = isLogin ? AppTheme.Primary : AppTheme.TextMuted;
        btnTabRegister.ForeColor = !isLogin ? AppTheme.Primary : AppTheme.TextMuted;

        pnlTabIndicator.Location = new Point(isLogin ? 0 : 135, 44);

        lblLoginStatus.Text = string.Empty;
        lblRegStatus.Text = string.Empty;

        if (isLogin)
        {
            txtEmail.InnerTextBox.Focus();
            AcceptButton = btnLogin;
        }
        else
        {
            txtRegFullName.InnerTextBox.Focus();
            AcceptButton = btnRegister;
        }
    }

    private async void QuickLogin(string email)
    {
        if (_isBusy) return;
        SwitchTab(true);
        txtEmail.Text = email;
        txtPassword.Text = "123456";
        await HandleLoginAsync();
    }

    private async Task HandleLoginAsync()
    {
        if (_isBusy) return;

        var email = txtEmail.Text.Trim();
        var password = txtPassword.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            lblLoginStatus.ForeColor = AppTheme.Danger;
            lblLoginStatus.Text = "Vui lòng nhập đầy đủ Email và Mật khẩu.";
            return;
        }

        SetBusy(true, "Đang xác thực thông tin đăng nhập...");

        try
        {
            var res = await WinFormsApiClient.Instance.LoginAsync(new LoginRequest
            {
                Email = email,
                Password = password
            });

            if (res.Success && res.Data != null)
            {
                lblLoginStatus.ForeColor = AppTheme.Success;
                lblLoginStatus.Text = "Đăng nhập thành công! Đang mở giao diện hệ thống...";

                // Khởi động kết nối SignalR Hub cho phiên làm việc
                _ = WinFormsRealtimeService.Instance.StartAsync();

                // Chuyển sang màn hình chính MainForm
                var mainForm = new MainForm();
                mainForm.FormClosed += (s, e) =>
                {
                    // Nếu đăng xuất (SessionContext bị xóa), hiện lại LoginForm
                    if (!SessionContext.IsLoggedIn)
                    {
                        txtPassword.Text = string.Empty;
                        lblLoginStatus.Text = string.Empty;
                        this.Show();
                    }
                    else
                    {
                        this.Close();
                    }
                };

                this.Hide();
                mainForm.Show();
            }
            else
            {
                lblLoginStatus.ForeColor = AppTheme.Danger;
                lblLoginStatus.Text = res.Message ?? "Đăng nhập thất bại. Vui lòng kiểm tra lại tài khoản.";
                ToastNotifier.Show(this, res.Message ?? "Email hoặc mật khẩu không chính xác.", ToastType.Danger, "Đăng Nhập Thất Bại");
            }
        }
        catch (Exception ex)
        {
            lblLoginStatus.ForeColor = AppTheme.Danger;
            lblLoginStatus.Text = $"Lỗi kết nối: {ex.Message}";
            ToastNotifier.Show(this, ex.Message, ToastType.Danger, "Lỗi Kết Nối Server");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task HandleRegisterAsync()
    {
        if (_isBusy) return;

        var fullName = txtRegFullName.Text.Trim();
        var phone = txtRegPhone.Text.Trim();
        var email = txtRegEmail.Text.Trim();
        var password = txtRegPassword.Text;
        var confirmPassword = txtRegConfirmPassword.Text;

        if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            lblRegStatus.ForeColor = AppTheme.Danger;
            lblRegStatus.Text = "Họ tên, Email và Mật khẩu không được để trống.";
            return;
        }

        if (password != confirmPassword)
        {
            lblRegStatus.ForeColor = AppTheme.Danger;
            lblRegStatus.Text = "Mật khẩu xác nhận không trùng khớp.";
            return;
        }

        if (password.Length < 6)
        {
            lblRegStatus.ForeColor = AppTheme.Danger;
            lblRegStatus.Text = "Mật khẩu phải có độ dài ít nhất 6 ký tự.";
            return;
        }

        SetBusy(true, "Đang đăng ký tài khoản khách hàng...");

        try
        {
            var res = await WinFormsApiClient.Instance.RegisterAsync(new RegisterRequest
            {
                FullName = fullName,
                PhoneNumber = phone,
                Email = email,
                Password = password
            });

            if (res.Success && res.Data != null)
            {
                ToastNotifier.Show(this, "Chúc mừng bạn đã tạo tài khoản thành công!", ToastType.Success, "Đăng Ký Thành Công");

                _ = WinFormsRealtimeService.Instance.StartAsync();

                var mainForm = new MainForm();
                mainForm.FormClosed += (s, e) =>
                {
                    if (!SessionContext.IsLoggedIn)
                    {
                        this.Show();
                    }
                    else
                    {
                        this.Close();
                    }
                };

                this.Hide();
                mainForm.Show();
            }
            else
            {
                lblRegStatus.ForeColor = AppTheme.Danger;
                lblRegStatus.Text = res.Message ?? "Đăng ký thất bại. Email có thể đã được sử dụng.";
            }
        }
        catch (Exception ex)
        {
            lblRegStatus.ForeColor = AppTheme.Danger;
            lblRegStatus.Text = $"Lỗi: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy, string? message = null)
    {
        _isBusy = busy;
        btnLogin.IsLoading = busy;
        btnRegister.IsLoading = busy;

#if DEBUG
        btnQuickAdmin.Enabled = !busy;
        btnQuickManager.Enabled = !busy;
        btnQuickReceptionist.Enabled = !busy;
        btnQuickCustomer.Enabled = !busy;
#endif

        if (busy && !string.IsNullOrEmpty(message))
        {
            lblLoginStatus.ForeColor = AppTheme.TextMuted;
            lblLoginStatus.Text = message;
            lblRegStatus.ForeColor = AppTheme.TextMuted;
            lblRegStatus.Text = message;
        }
    }
}
