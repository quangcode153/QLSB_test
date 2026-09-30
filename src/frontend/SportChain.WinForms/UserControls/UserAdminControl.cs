using SportChain.Shared.DTOs.Admin;
using SportChain.Shared.DTOs.Courts;
using SportChain.Shared.Enums;
using SportChain.WinForms.Services;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UserControls;

public partial class UserAdminControl : UserControl
{
    private List<UserDto> _users = new();
    private List<BranchDto> _branches = new();
    private List<AuditLogDto> _auditLogs = new();

    public UserAdminControl()
    {
        InitializeComponent();
        SetupGridColumns();
        SetupEvents();
    }

    private static void EnableDoubleBuffer(Control control)
    {
        typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(control, true, null);
    }

    public async Task LoadAllAdminDataAsync()
    {
        await LoadUsersAsync();
        await LoadBranchesAsync();
        await LoadAuditLogsAsync();
    }

    private void SetupGridColumns()
    {
        EnableDoubleBuffer(dgvUsers);
        EnableDoubleBuffer(dgvBranches);
        EnableDoubleBuffer(dgvAuditLogs);

        // Apply AppTheme header styles
        StyleGrid(dgvUsers);
        StyleGrid(dgvBranches);
        StyleGrid(dgvAuditLogs);

        // 1. dgvUsers
        dgvUsers.Columns.Clear();
        dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50 });
        dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Họ và Tên", Width = 140 });
        dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", Width = 160 });
        dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Role", HeaderText = "Vai Trò", Width = 110 });
        dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "BranchName", HeaderText = "Chi Nhánh", Width = 140 });
        dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng Thái", Width = 100 });

        // 2. dgvBranches
        dgvBranches.Columns.Clear();
        dgvBranches.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50 });
        dgvBranches.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Tên Chi Nhánh", Width = 160 });
        dgvBranches.Columns.Add(new DataGridViewTextBoxColumn { Name = "Address", HeaderText = "Địa Chỉ", Width = 180 });
        dgvBranches.Columns.Add(new DataGridViewTextBoxColumn { Name = "City", HeaderText = "Thành Phố", Width = 100 });
        dgvBranches.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalCourts", HeaderText = "Tổng Sân", Width = 80 });
        dgvBranches.Columns.Add(new DataGridViewTextBoxColumn { Name = "IsApproved", HeaderText = "Phê Duyệt", Width = 110 });
        dgvBranches.Columns.Add(new DataGridViewTextBoxColumn { Name = "IsActive", HeaderText = "Hoạt Động", Width = 110 });

        // 3. dgvAuditLogs
        dgvAuditLogs.Columns.Clear();
        dgvAuditLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "Timestamp", HeaderText = "Thời Gian", Width = 130 });
        dgvAuditLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "UserName", HeaderText = "Người Thực Hiện", Width = 140 });
        dgvAuditLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "Action", HeaderText = "Hành Động", Width = 160 });
        dgvAuditLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "Entity", HeaderText = "Thực Thể", Width = 90 });
        dgvAuditLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "Details", HeaderText = "Nội Dung Chi Tiết", Width = 350 });
        dgvAuditLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "IpAddress", HeaderText = "IP", Width = 90 });

        // Staff Roles (Only BranchManager & Receptionist can be provisioned)
        cboStaffRole.Items.Clear();
        cboStaffRole.Items.Add(UserRole.BranchManager);
        cboStaffRole.Items.Add(UserRole.Receptionist);
        cboStaffRole.SelectedIndex = 0;
    }

    private void StyleGrid(DataGridView grid)
    {
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.SidebarDarker;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontBodyBold;
        grid.ColumnHeadersHeight = 36;
        grid.RowTemplate.Height = 32;
        grid.DefaultCellStyle.Font = AppTheme.FontBody;
        grid.DefaultCellStyle.SelectionBackColor = AppTheme.PrimaryLight;
        grid.DefaultCellStyle.SelectionForeColor = AppTheme.PrimaryPressed;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
    }

    private void SetupEvents()
    {
        btnRefreshAll.Click += async (s, e) => await LoadAllAdminDataAsync();
        btnToggleUserStatus.Click += async (s, e) => await HandleToggleUserStatusAsync();
        btnCreateStaff.Click += async (s, e) => await HandleCreateStaffAsync();

        btnApproveBranch.Click += async (s, e) => await HandleApproveBranchAsync();
        btnToggleBranchStatus.Click += async (s, e) => await HandleToggleBranchStatusAsync();

        btnLoadLogs.Click += async (s, e) => await LoadAuditLogsAsync();
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            var res = await WinFormsApiClient.Instance.GetUsersAsync();
            if (res.Success && res.Data != null)
            {
                _users = res.Data;
                dgvUsers.Rows.Clear();

                foreach (var u in _users)
                {
                    var status = u.IsActive ? "🟢 Hoạt Động" : "🔒 Đã Khóa";
                    var rowIndex = dgvUsers.Rows.Add(
                        u.Id,
                        u.FullName,
                        u.Email,
                        u.Role.ToString(),
                        u.BranchName,
                        status
                    );

                    var row = dgvUsers.Rows[rowIndex];
                    row.Tag = u;

                    if (!u.IsActive)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(254, 242, 242);
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoadUsers] {ex.Message}");
        }
    }

    private async Task LoadBranchesAsync()
    {
        try
        {
            var res = await WinFormsApiClient.Instance.GetBranchesAsync(onlyApproved: false);
            if (res.Success && res.Data != null)
            {
                _branches = res.Data;

                // Populate cboStaffBranch
                cboStaffBranch.DisplayMember = "Name";
                cboStaffBranch.ValueMember = "Id";
                cboStaffBranch.DataSource = _branches.ToList();

                dgvBranches.Rows.Clear();
                foreach (var b in _branches)
                {
                    var appText = b.IsApproved ? "✅ Đã Duyệt" : "⏳ Chờ Duyệt";
                    var actText = b.IsActive ? "🟢 Mở Cửa" : "🔒 Tạm Dừng";

                    var rowIndex = dgvBranches.Rows.Add(
                        b.Id,
                        b.Name,
                        b.Address,
                        b.City,
                        b.TotalCourts,
                        appText,
                        actText
                    );

                    var row = dgvBranches.Rows[rowIndex];
                    row.Tag = b;

                    if (!b.IsApproved)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(254, 243, 199);
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(146, 64, 14);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoadBranches] {ex.Message}");
        }
    }

    private async Task LoadAuditLogsAsync()
    {
        try
        {
            int limit = (int)nudLogLimit.Value;
            var res = await WinFormsApiClient.Instance.GetAuditLogsAsync(limit);
            if (res.Success && res.Data != null)
            {
                _auditLogs = res.Data;
                dgvAuditLogs.Rows.Clear();

                foreach (var a in _auditLogs)
                {
                    var rowIndex = dgvAuditLogs.Rows.Add(
                        a.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"),
                        a.UserName,
                        a.Action,
                        a.EntityName,
                        a.Details,
                        a.IpAddress
                    );

                    dgvAuditLogs.Rows[rowIndex].Tag = a;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoadAuditLogs] {ex.Message}");
        }
    }

    private async Task HandleToggleUserStatusAsync()
    {
        if (dgvUsers.SelectedRows.Count == 0 || dgvUsers.SelectedRows[0].Tag is not UserDto u)
        {
            MessageBox.Show("Vui lòng chọn một tài khoản để khóa/mở khóa.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (u.Role == UserRole.SuperAdmin)
        {
            MessageBox.Show("Không thể khóa tài khoản SuperAdmin tối cao của hệ thống!", "Cảnh Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var actionText = u.IsActive ? "khóa tài khoản" : "mở khóa tài khoản";
        var confirm = MessageBox.Show($"Bạn có chắc muốn {actionText} '{u.FullName}' ({u.Email})?", "Xác Nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        try
        {
            var res = await WinFormsApiClient.Instance.ToggleUserStatusAsync(u.Id);
            if (res.Success)
            {
                ToastNotifier.Show(this, res.Message ?? "Đã cập nhật trạng thái tài khoản thành công!", ToastType.Success);
                await LoadUsersAsync();
            }
            else
            {
                MessageBox.Show(res.Message ?? "Không thể cập nhật.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task HandleCreateStaffAsync()
    {
        var name = txtStaffName.Text.Trim();
        var email = txtStaffEmail.Text.Trim();
        var pass = txtStaffPass.Text.Trim();
        var phone = txtStaffPhone.Text.Trim();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            MessageBox.Show("Vui lòng điền đầy đủ Họ tên, Email và Mật khẩu khởi tạo!", "Thiếu Thông Tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (pass.Length < 6)
        {
            MessageBox.Show("Mật khẩu khởi tạo phải có tối thiểu 6 ký tự!", "Mật Khẩu Yếu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (cboStaffBranch.SelectedValue is not int branchId)
        {
            MessageBox.Show("Vui lòng chọn cơ sở làm việc cho nhân viên!", "Thiếu Chi Nhánh", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var role = (UserRole)(cboStaffRole.SelectedItem ?? UserRole.Receptionist);

        btnCreateStaff.Enabled = false;
        try
        {
            var res = await WinFormsApiClient.Instance.CreateStaffAsync(new CreateStaffRequest
            {
                FullName = name,
                Email = email,
                Password = pass,
                PhoneNumber = phone,
                Role = role,
                BranchId = branchId
            });

            if (res.Success)
            {
                ToastNotifier.Show(this, $"Cấp tài khoản thành công: {email}", ToastType.Success);
                txtStaffName.Clear();
                txtStaffEmail.Clear();
                txtStaffPass.Clear();
                txtStaffPhone.Clear();
                await LoadUsersAsync();
                await LoadAuditLogsAsync();
            }
            else
            {
                MessageBox.Show(res.Message ?? "Không thể tạo tài khoản nhân sự.", "Lỗi Cấp Quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnCreateStaff.Enabled = true;
        }
    }

    private async Task HandleApproveBranchAsync()
    {
        if (dgvBranches.SelectedRows.Count == 0 || dgvBranches.SelectedRows[0].Tag is not BranchDto b)
        {
            MessageBox.Show("Vui lòng chọn chi nhánh trong bảng để phê duyệt.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (b.IsApproved)
        {
            MessageBox.Show("Chi nhánh này đã được phê duyệt trước đó!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show($"Xác nhận phê duyệt cho chi nhánh '{b.Name}' ({b.Address}, {b.City}) chính thức đi vào hoạt động?", "Phê Duyệt Chi Nhánh", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        try
        {
            var res = await WinFormsApiClient.Instance.ApproveBranchAsync(b.Id);
            if (res.Success)
            {
                ToastNotifier.Show(this, "Đã phê duyệt hoạt động chi nhánh thành công!", ToastType.Success);
                await LoadBranchesAsync();
                await LoadAuditLogsAsync();
            }
            else
            {
                MessageBox.Show(res.Message ?? "Không thể duyệt chi nhánh.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task HandleToggleBranchStatusAsync()
    {
        if (dgvBranches.SelectedRows.Count == 0 || dgvBranches.SelectedRows[0].Tag is not BranchDto b)
        {
            MessageBox.Show("Vui lòng chọn chi nhánh trong bảng để thao tác.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var actionText = b.IsActive ? "tạm dừng hoạt động" : "mở lại hoạt động";
        var confirm = MessageBox.Show($"Bạn có chắc muốn {actionText} cho chi nhánh '{b.Name}'?", "Xác Nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        try
        {
            var res = await WinFormsApiClient.Instance.ToggleBranchStatusAsync(b.Id);
            if (res.Success)
            {
                MessageBox.Show("Đã cập nhật trạng thái chi nhánh thành công!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadBranchesAsync();
                await LoadAuditLogsAsync();
            }
            else
            {
                MessageBox.Show(res.Message ?? "Không thể cập nhật chi nhánh.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
