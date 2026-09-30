using System.Drawing;
using System.Windows.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using SportChain.Shared.DTOs.Courts;
using SportChain.Shared.Enums;
using SportChain.WinForms.Common;
using SportChain.WinForms.Services;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Dialogs;
using SportChain.WinForms.UI.Theme;
using SportChain.WinForms.UserControls;

namespace SportChain.WinForms.Forms;

public partial class MainForm : Form
{
    private List<BranchDto> _branches = new();
    private int _selectedBranchId = 1;
    private SidebarButton? _currentActiveButton;
    private bool _isSidebarCollapsed = false;
    private Action<HubConnectionState>? _realtimeStateListener;

    public static MainForm? Instance { get; private set; }

    public int SelectedBranchId => _selectedBranchId;
    public event Action<int>? OnBranchChanged;

    public MainForm()
    {
        Instance = this;
        InitializeComponent();
        KeyPreview = true;
        KeyDown += MainForm_KeyDown;
        FormClosing += MainForm_FormClosing;
        this.Load += async (s, e) => await InitializeDashboardAsync();
    }

    private async Task InitializeDashboardAsync()
    {
        ApplyUserInfo();
        ApplyPermissions();
        SetupClockTimer();
        SetupSidebarToggle();
        SetupRealtimeStateListener();
        await LoadBranchesAsync();

        // Mặc định mở Ma Trận Đặt Sân khi vừa vào hệ thống
        OpenMatrixView();
    }

    private void ApplyUserInfo()
    {
        var user = SessionContext.CurrentUser;
        if (user == null) return;

        lblUserName.Text = user.FullName;

        // Sinh 2 chữ cái viết tắt đại diện tên người dùng cho Avatar
        lblUserAvatarInitials.Text = GetInitials(user.FullName);

        switch (user.Role)
        {
            case UserRole.SuperAdmin:
                badgeUserRole.Type = BadgeType.Danger;
                badgeUserRole.Text = "TỔNG QUẢN TRỊ";
                lblUserBranch.Text = "Phạm vi: Toàn chuỗi cơ sở";
                break;
            case UserRole.BranchManager:
                badgeUserRole.Type = BadgeType.Warning;
                badgeUserRole.Text = "QUẢN LÝ CƠ SỞ";
                lblUserBranch.Text = $"Cơ sở ID: {user.BranchId ?? 1}";
                break;
            case UserRole.Receptionist:
                badgeUserRole.Type = BadgeType.Info;
                badgeUserRole.Text = "LỄ TÂN THU NGÂN";
                lblUserBranch.Text = $"Cơ sở ID: {user.BranchId ?? 1}";
                break;
            case UserRole.Customer:
            default:
                badgeUserRole.Type = BadgeType.Success;
                badgeUserRole.Text = "KHÁCH HÀNG";
                lblUserBranch.Text = "Tài khoản cá nhân";
                break;
        }
    }

    private static string GetInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "US";
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return parts[0].Length >= 2 ? parts[0].Substring(0, 2).ToUpper() : parts[0].ToUpper();
        }
        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }

    private void SetupClockTimer()
    {
        lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
        timerClock.Interval = 1000;
        timerClock.Tick += (s, e) => lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
        timerClock.Start();
    }

    private void SetupSidebarToggle()
    {
        btnToggleSidebar.Click += (s, e) => ToggleSidebar();
        btnNavLogout.Click += (s, e) => HandleLogout();
    }

    private void ToggleSidebar()
    {
        _isSidebarCollapsed = !_isSidebarCollapsed;

        pnlSidebar.SuspendLayout();
        pnlNavMenu.SuspendLayout();
        pnlUserInfo.SuspendLayout();
        pnlSidebarBottom.SuspendLayout();

        int targetSidebarWidth = _isSidebarCollapsed ? 72 : 260;
        int targetBtnWidth = _isSidebarCollapsed ? 52 : 240;
        bool isExpanded = !_isSidebarCollapsed;

        pnlSidebar.Width = targetSidebarWidth;

        lblBrandName.Visible = isExpanded;
        lblBrandSub.Visible = isExpanded;
        lblUserName.Visible = isExpanded;
        badgeUserRole.Visible = isExpanded;
        lblUserBranch.Visible = isExpanded;
        lblVersion.Visible = isExpanded;

        pnlUserAvatarCircle.Location = isExpanded ? new Point(16, 16) : new Point(15, 16);

        var navButtons = new[] { btnNavMatrix, btnNavMyBookings, btnNavPos, btnNavBranch, btnNavUsers, btnNavLogout };
        foreach (var btn in navButtons)
        {
            btn.IsCollapsed = _isSidebarCollapsed;
            btn.Size = new Size(targetBtnWidth, btn.Height);
            btn.Invalidate();
        }

        pnlNavMenu.ResumeLayout(true);
        pnlUserInfo.ResumeLayout(true);
        pnlSidebarBottom.ResumeLayout(true);
        pnlSidebar.ResumeLayout(true);
        pnlSidebar.PerformLayout();
        pnlSidebar.Refresh();
    }

    private void ApplyPermissions()
    {
        btnNavUsers.Visible = SessionContext.IsSuperAdmin;
        btnNavBranch.Visible = SessionContext.IsSuperAdmin || SessionContext.IsBranchManager;
        btnNavPos.Visible = SessionContext.IsSuperAdmin || SessionContext.IsBranchManager || SessionContext.IsReceptionist;
        btnNavMyBookings.Visible = SessionContext.IsCustomer || SessionContext.IsSuperAdmin;
        btnNavMatrix.Visible = true;

        // Gán sự kiện click cho các nút menu
        btnNavMatrix.Click += (s, e) => OpenMatrixView();
        btnNavMyBookings.Click += (s, e) => OpenMyBookingsView();
        btnNavPos.Click += (s, e) => OpenPosView();
        btnNavBranch.Click += (s, e) => OpenBranchReportView();
        btnNavUsers.Click += (s, e) => OpenUserAdminView();
    }

    private void OpenMatrixView()
    {
        var matrixControl = new CourtMatrixControl();
        SelectNavButton(btnNavMatrix, "Trang Chủ / Đặt Sân /", "Ma Trận Đặt Sân & Khóa Ca Tự Động", matrixControl);
        _ = matrixControl.LoadMatrixDataAsync(_selectedBranchId);
    }

    private void OpenMyBookingsView()
    {
        var bookingsControl = new MyBookingsControl();
        SelectNavButton(btnNavMyBookings, "Cá Nhân / Lịch Sử /", "Danh Sách Đơn Đặt & Vé Điện Tử Của Tôi", bookingsControl);
        _ = bookingsControl.LoadMyBookingsAsync();
    }

    private void OpenPosView()
    {
        var posControl = new ReceptionistPosControl();
        SelectNavButton(btnNavPos, "Vận Hành / Lễ Tân /", "Quầy Thu Ngân POS & Quét Mã Check-in", posControl);
        _ = posControl.InitializeForBranchAsync(_selectedBranchId);
    }

    private void OpenBranchReportView()
    {
        var reportControl = new BranchReportControl();
        SelectNavButton(btnNavBranch, "Báo Cáo / Thống Kê /", "Báo Cáo Doanh Thu & Giám Sát Chi Nhánh", reportControl);
        _ = reportControl.InitializeForBranchAsync(_selectedBranchId);
    }

    private void OpenUserAdminView()
    {
        var adminControl = new UserAdminControl();
        SelectNavButton(btnNavUsers, "Hệ Thống / Quản Trị /", "Quản Trị Người Dùng & Phân Quyền Nhân Sự", adminControl);
        _ = adminControl.LoadAllAdminDataAsync();
    }

    private async Task LoadBranchesAsync()
    {
        try
        {
            var res = await WinFormsApiClient.Instance.GetBranchesAsync();
            if (res.Success && res.Data != null && res.Data.Count > 0)
            {
                _branches = res.Data;
                cboBranchSelect.DisplayMember = "Name";
                cboBranchSelect.ValueMember = "Id";
                cboBranchSelect.DataSource = _branches;

                // Nếu user thuộc chi nhánh cụ thể, tự chọn chi nhánh đó
                var user = SessionContext.CurrentUser;
                if (user?.BranchId != null && _branches.Any(b => b.Id == user.BranchId.Value))
                {
                    cboBranchSelect.SelectedValue = user.BranchId.Value;
                }
                else
                {
                    cboBranchSelect.SelectedIndex = 0;
                }

                if (cboBranchSelect.SelectedValue is int initialBId)
                {
                    _selectedBranchId = initialBId;
                }

                // Khóa chọn chi nhánh đối với Quản lý chi nhánh & Lễ tân theo Rule RBAC
                if (SessionContext.IsBranchManager || SessionContext.IsReceptionist)
                {
                    cboBranchSelect.Enabled = false;
                    lblBranchSelectLabel.Text = "Cơ sở (Cố định):";
                }

                cboBranchSelect.SelectedIndexChanged += async (s, e) =>
                {
                    if (cboBranchSelect.SelectedValue is int bId)
                    {
                        _selectedBranchId = bId;
                        await WinFormsRealtimeService.Instance.JoinBranchAsync(bId);
                        OnBranchChanged?.Invoke(bId);

                        if (pnlContentHost.Controls.Count > 0)
                        {
                            if (pnlContentHost.Controls[0] is CourtMatrixControl matrixCtrl)
                            {
                                await matrixCtrl.LoadMatrixDataAsync(bId);
                            }
                            else if (pnlContentHost.Controls[0] is ReceptionistPosControl posCtrl)
                            {
                                await posCtrl.InitializeForBranchAsync(bId);
                            }
                            else if (pnlContentHost.Controls[0] is BranchReportControl repCtrl)
                            {
                                await repCtrl.InitializeForBranchAsync(bId);
                            }
                        }
                    }
                };

                // Tham gia nhóm Realtime cho chi nhánh được chọn ban đầu
                await WinFormsRealtimeService.Instance.JoinBranchAsync(_selectedBranchId);
            }
        }
        catch (Exception ex)
        {
            badgeSignalR.Type = BadgeType.Warning;
            badgeSignalR.Text = "Lỗi nạp cơ sở";
            System.Diagnostics.Debug.WriteLine($"[LoadBranches] {ex.Message}");
        }
    }

    private void SetupRealtimeStateListener()
    {
        UpdateRealtimeBadge(WinFormsRealtimeService.Instance.CurrentState);

        _realtimeStateListener = state =>
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            this.BeginInvoke(() => UpdateRealtimeBadge(state));
        };

        WinFormsRealtimeService.Instance.OnConnectionStateChanged += _realtimeStateListener;
    }

    private void UpdateRealtimeBadge(HubConnectionState state)
    {
        switch (state)
        {
            case HubConnectionState.Connected:
                badgeSignalR.Type = BadgeType.Success;
                badgeSignalR.Text = "Realtime Sẵn Sàng";
                break;
            case HubConnectionState.Reconnecting:
            case HubConnectionState.Connecting:
                badgeSignalR.Type = BadgeType.Warning;
                badgeSignalR.Text = "Đang Kết Nối...";
                break;
            case HubConnectionState.Disconnected:
            default:
                badgeSignalR.Type = BadgeType.Danger;
                badgeSignalR.Text = "Mất Kết Nối";
                break;
        }
    }

    public void SelectNavButton(SidebarButton btn, string breadcrumb, string title, Control content)
    {
        if (_currentActiveButton == btn && pnlContentHost.Controls.Count > 0)
        {
            return;
        }

        // Tắt active của nút trước đó
        if (_currentActiveButton != null)
        {
            _currentActiveButton.IsActive = false;
        }

        // Kích hoạt nút mới
        _currentActiveButton = btn;
        _currentActiveButton.IsActive = true;

        // Cập nhật Breadcrumb & Tiêu đề TopBar
        lblBreadcrumb.Text = breadcrumb;
        lblViewTitle.Text = title;

        // Dọn dẹp điều khiển cũ tránh memory leak và nạp giao diện mới
        pnlContentHost.SuspendLayout();
        foreach (Control oldCtrl in pnlContentHost.Controls)
        {
            oldCtrl.Dispose();
        }
        pnlContentHost.Controls.Clear();
        content.Dock = DockStyle.Fill;
        pnlContentHost.Controls.Add(content);
        pnlContentHost.ResumeLayout();
    }

    public void ShowLoading(string message = "Đang tải dữ liệu...")
    {
        _loadingOverlay.ShowLoading(message);
    }

    public void HideLoading()
    {
        _loadingOverlay.HideLoading();
    }

    private void HandleLogout()
    {
        if (ConfirmDialog.Show(this, "Xác Nhận Đăng Xuất", "Bạn có chắc chắn muốn đăng xuất khỏi hệ thống SportChain VN?", "Đăng Xuất", true))
        {
            SessionContext.ClearSession();
            this.Close();
        }
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control)
        {
            switch (e.KeyCode)
            {
                case Keys.D1:
                case Keys.NumPad1:
                    if (btnNavMatrix.Visible) { OpenMatrixView(); e.Handled = true; }
                    break;
                case Keys.D2:
                case Keys.NumPad2:
                    if (btnNavMyBookings.Visible) { OpenMyBookingsView(); e.Handled = true; }
                    break;
                case Keys.D3:
                case Keys.NumPad3:
                    if (btnNavPos.Visible) { OpenPosView(); e.Handled = true; }
                    break;
                case Keys.D4:
                case Keys.NumPad4:
                    if (btnNavBranch.Visible) { OpenBranchReportView(); e.Handled = true; }
                    break;
                case Keys.D5:
                case Keys.NumPad5:
                    if (btnNavUsers.Visible) { OpenUserAdminView(); e.Handled = true; }
                    break;
                case Keys.B:
                    ToggleSidebar();
                    e.Handled = true;
                    break;
                case Keys.L:
                    HandleLogout();
                    e.Handled = true;
                    break;
            }
        }
        else if (e.KeyCode == Keys.F5)
        {
            // F5 Làm mới dữ liệu màn hình hiện tại
            RefreshCurrentView();
            ToastNotifier.Show(this, "Đã gửi lệnh làm mới dữ liệu.", ToastType.Info);
            e.Handled = true;
        }
    }

    private void RefreshCurrentView()
    {
        if (pnlContentHost.Controls.Count == 0) return;
        var current = pnlContentHost.Controls[0];

        if (current is CourtMatrixControl matrixCtrl)
        {
            _ = matrixCtrl.LoadMatrixDataAsync(_selectedBranchId);
        }
        else if (current is MyBookingsControl bookingsCtrl)
        {
            _ = bookingsCtrl.LoadMyBookingsAsync();
        }
        else if (current is ReceptionistPosControl posCtrl)
        {
            _ = posCtrl.InitializeForBranchAsync(_selectedBranchId);
        }
        else if (current is BranchReportControl repCtrl)
        {
            _ = repCtrl.InitializeForBranchAsync(_selectedBranchId);
        }
        else if (current is UserAdminControl adminCtrl)
        {
            _ = adminCtrl.LoadAllAdminDataAsync();
        }
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // Gỡ bỏ timer & realtime listener để tránh rò rỉ bộ nhớ
        timerClock.Stop();
        timerClock.Dispose();

        if (_realtimeStateListener != null)
        {
            WinFormsRealtimeService.Instance.OnConnectionStateChanged -= _realtimeStateListener;
            _realtimeStateListener = null;
        }
    }
}

