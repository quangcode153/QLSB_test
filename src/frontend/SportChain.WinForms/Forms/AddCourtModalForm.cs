using System.Windows.Forms;
using SportChain.Shared.Constants;
using SportChain.Shared.DTOs.Courts;
using SportChain.Shared.Enums;
using SportChain.WinForms.Services;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.Forms;

public partial class AddCourtModalForm : Form
{
    private readonly int? _defaultBranchId;
    private List<BranchDto> _branches = new();

    public AddCourtModalForm(int? defaultBranchId = null, string? branchName = null, string? branchAddress = null)
    {
        _defaultBranchId = defaultBranchId;
        InitializeComponent();

        SetupData();
        SetupEvents();
        _ = LoadBranchesAsync();
    }

    private void SetupData()
    {
        // Danh sách bộ môn thể thao
        cboSportType.Items.Clear();
        cboSportType.Items.Add(SportType.Pickleball);
        cboSportType.Items.Add(SportType.Badminton);
        cboSportType.Items.Add(SportType.Football);
        cboSportType.Items.Add(SportType.Tennis);
        cboSportType.SelectedIndex = 0;

        // Gợi ý loại mặt sân phổ biến
        cboSurfaceType.Items.Clear();
        cboSurfaceType.Items.Add("Thảm PVC chuyên dụng 4.5mm");
        cboSurfaceType.Items.Add("Cỏ nhân tạo 5 người cao cấp");
        cboSurfaceType.Items.Add("Sàn gỗ thông đạt chuẩn thi đấu");
        cboSurfaceType.Items.Add("Sơn Acrylic chống trượt ngoài trời");
        cboSurfaceType.SelectedIndex = 0;

        cboSportType.SelectedIndexChanged += (s, e) =>
        {
            var st = (SportType)(cboSportType.SelectedItem ?? SportType.Pickleball);
            var stdPrice = SystemPolicies.GetStandardPrice(st, false);
            txtDefaultPrice.PlaceholderText = $"Giá chuẩn: {stdPrice:N0} đ/h";
        };
    }

    private void SetupEvents()
    {
        btnSave.Click += async (s, e) => await HandleSaveCourtAsync();

        cboBranch.SelectedIndexChanged += (s, e) =>
        {
            if (cboBranch.SelectedItem is BranchDto selectedBranch)
            {
                lblBranchAddress.Text = $"📍 Địa chỉ cơ sở: {selectedBranch.Address}, {selectedBranch.City}";
                txtCourtLocation.PlaceholderText = $"Ví dụ: Tầng 2 - Tòa A ({selectedBranch.Name}) hoặc Cổng số 2";
            }
        };
    }

    private async Task LoadBranchesAsync()
    {
        try
        {
            var res = await WinFormsApiClient.Instance.GetBranchesAsync();
            if (res.Success && res.Data != null && res.Data.Count > 0)
            {
                _branches = res.Data;
                cboBranch.DisplayMember = "Name";
                cboBranch.ValueMember = "Id";
                cboBranch.DataSource = _branches;

                var currentUser = SessionContext.CurrentUser;
                bool isSuperAdmin = currentUser?.Role == UserRole.SuperAdmin;

                // Quy tắc phân quyền chi nhánh:
                // Nếu tài khoản là Quản lý chi nhánh (hoặc gắn với 1 BranchId cụ thể), tự động chọn và KHÓA chi nhánh đó.
                if (!isSuperAdmin && currentUser?.BranchId != null)
                {
                    var targetId = currentUser.BranchId.Value;
                    if (_branches.Any(b => b.Id == targetId))
                    {
                        cboBranch.SelectedValue = targetId;
                    }
                    cboBranch.Enabled = false; // Khóa không cho chọn cơ sở khác

                    var branchObj = _branches.FirstOrDefault(b => b.Id == targetId);
                    lblBranchHint.Text = $"🔒 Cơ sở trực thuộc: {branchObj?.Name} (Cố định theo tài khoản quản lý của bạn)";
                    lblBranchHint.ForeColor = AppTheme.TextMuted;
                }
                else
                {
                    // Tài khoản SuperAdmin: Cho phép tự do chọn bất kỳ chi nhánh nào
                    cboBranch.Enabled = true;
                    if (_defaultBranchId.HasValue && _branches.Any(b => b.Id == _defaultBranchId.Value))
                    {
                        cboBranch.SelectedValue = _defaultBranchId.Value;
                    }

                    lblBranchHint.Text = "👑 Quyền SuperAdmin: Bạn có thể chọn và thêm sân cho bất kỳ cơ sở nào trong chuỗi.";
                    lblBranchHint.ForeColor = AppTheme.Primary;
                }

                if (cboBranch.SelectedItem is BranchDto currentBranch)
                {
                    lblBranchAddress.Text = $"📍 Địa chỉ cơ sở: {currentBranch.Address}, {currentBranch.City}";
                    txtCourtLocation.PlaceholderText = $"Ví dụ: Tầng 2 - Tòa A ({currentBranch.Name}) hoặc Cổng số 2";
                }
            }
            else
            {
                lblBranchHint.Text = "⚠️ Không thể tải danh sách chi nhánh.";
                lblBranchHint.ForeColor = AppTheme.Danger;
            }
        }
        catch (Exception ex)
        {
            lblBranchHint.Text = $"⚠️ Lỗi kết nối: {ex.Message}";
            lblBranchHint.ForeColor = AppTheme.Danger;
        }
    }

    private async Task HandleSaveCourtAsync()
    {
        if (cboBranch.SelectedValue == null)
        {
            MessageBox.Show("Vui lòng chọn cơ sở chi nhánh quản lý sân!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cboBranch.Focus();
            return;
        }

        var targetBranchId = (int)cboBranch.SelectedValue;

        var courtName = txtCourtName.Text.Trim();
        if (string.IsNullOrEmpty(courtName))
        {
            txtCourtName.ErrorMessage = "Vui lòng nhập tên sân thể thao!";
            txtCourtName.InnerTextBox.Focus();
            return;
        }

        var courtLocation = txtCourtLocation.Text.Trim();
        // Ghép tên sân kèm vị trí/địa chỉ cụ thể để khách hàng dễ dàng tiếp cận khi đến sân
        var finalCourtName = !string.IsNullOrWhiteSpace(courtLocation)
            ? $"{courtName} ({courtLocation})"
            : courtName;

        var sportType = (SportType)(cboSportType.SelectedItem ?? SportType.Pickleball);
        var surfaceType = cboSurfaceType.Text.Trim();
        if (string.IsNullOrEmpty(surfaceType))
        {
            surfaceType = "Thảm tiêu chuẩn";
        }

        decimal defaultPrice = 0;
        var rawPrice = txtDefaultPrice.Text.Replace(",", "").Replace(".", "").Trim();
        if (decimal.TryParse(rawPrice, out var parsedPrice) && parsedPrice > 0)
        {
            defaultPrice = parsedPrice;
        }
        else
        {
            defaultPrice = SystemPolicies.GetStandardPrice(sportType, false);
        }

        btnSave.Enabled = false;
        btnSave.Text = "Đang tạo...";

        try
        {
            var res = await WinFormsApiClient.Instance.CreateCourtAsync(new CourtDto
            {
                BranchId = targetBranchId,
                Name = finalCourtName,
                SportType = sportType,
                SurfaceType = surfaceType,
                DefaultPrice = defaultPrice,
                Status = SlotStatus.Available
            });

            if (res.Success)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(res.Message ?? "Không thể tạo sân mới.", "Lỗi Tạo Sân", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnSave.Enabled = true;
            btnSave.Text = "Tạo Sân Mới";
        }
    }
}
