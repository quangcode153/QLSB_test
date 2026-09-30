using System.Drawing;
using System.Windows.Forms;
using SportChain.Shared.DTOs.Bookings;
using SportChain.Shared.DTOs.Courts;
using SportChain.Shared.Enums;
using SportChain.WinForms.Forms;
using SportChain.WinForms.Services;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UserControls;

public partial class BranchReportControl : UserControl
{
    private int _branchId = 1;
    private BranchDto? _currentBranch;
    private List<CourtDto> _courts = new();
    private List<BookingDto> _branchBookings = new();

    public BranchReportControl()
    {
        InitializeComponent();
        SetupGridColumns();
        SetupEvents();
    }

    public async Task InitializeForBranchAsync(int branchId)
    {
        _branchId = branchId;
        await LoadReportDataAsync();
    }

    private void SetupGridColumns()
    {
        // Bật DoubleBuffered qua Reflection để chống giật nháy hình
        try
        {
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty;
            typeof(DataGridView).InvokeMember("DoubleBuffered", flags, null, dgvCourts, new object[] { true });
            typeof(DataGridView).InvokeMember("DoubleBuffered", flags, null, dgvBranchBookings, new object[] { true });
        }
        catch { }

        // 1. dgvCourts
        dgvCourts.Columns.Clear();
        dgvCourts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Tên Sân", Width = 140 });
        dgvCourts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Location", HeaderText = "Vị Trí / Địa Chỉ Cụ Thể", Width = 190 });
        dgvCourts.Columns.Add(new DataGridViewTextBoxColumn { Name = "SportType", HeaderText = "Môn Thể Thao", Width = 120 });
        dgvCourts.Columns.Add(new DataGridViewTextBoxColumn { Name = "SurfaceType", HeaderText = "Mặt Sân", Width = 130 });
        dgvCourts.Columns.Add(new DataGridViewTextBoxColumn { Name = "DefaultPrice", HeaderText = "Giá Giờ Chuẩn", Width = 110 });
        dgvCourts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng Thái Hoạt Động", Width = 140 });

        // 2. dgvBranchBookings
        dgvBranchBookings.Columns.Clear();
        dgvBranchBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = "BookingCode", HeaderText = "Mã Đơn", Width = 120 });
        dgvBranchBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = "CustomerName", HeaderText = "Khách Hàng", Width = 140 });
        dgvBranchBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = "CourtName", HeaderText = "Sân", Width = 90 });
        dgvBranchBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = "Slots", HeaderText = "Khung Giờ", Width = 120 });
        dgvBranchBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng Thái", Width = 120 });
        dgvBranchBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalAmount", HeaderText = "Tổng Tiền", Width = 110 });
        dgvBranchBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = "DepositAmount", HeaderText = "Đã Cọc", Width = 100 });
        dgvBranchBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedAt", HeaderText = "Thời Gian Tạo", Width = 130 });
    }

    private void SetupEvents()
    {
        btnRefresh.Click += async (s, e) => await LoadReportDataAsync();

        btnAddCourt.Click += async (s, e) =>
        {
            using var dlg = new AddCourtModalForm(_branchId, _currentBranch?.Name, _currentBranch?.Address);
            if (dlg.ShowDialog(this.FindForm()) == DialogResult.OK)
            {
                ToastNotifier.Show(this, "Đã thêm sân thể thao mới thành công!", ToastType.Success);
                await LoadReportDataAsync();
            }
        };

        btnToggleMaintenance.Click += async (s, e) =>
        {
            if (dgvCourts.SelectedRows.Count == 0 || dgvCourts.SelectedRows[0].Tag is not CourtDto court)
            {
                MessageBox.Show("Vui lòng chọn một sân trong bảng để điều chuyển trạng thái.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var newStatus = court.Status == SlotStatus.Maintenance ? SlotStatus.Available : SlotStatus.Maintenance;
            var actionText = newStatus == SlotStatus.Maintenance ? "bật bảo trì (khóa đặt lịch)" : "mở lại hoạt động bình thường";

            var confirm = MessageBox.Show($"Bạn có chắc muốn {actionText} cho sân '{court.Name}'?", "Xác Nhận Thay Đổi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            btnToggleMaintenance.Enabled = false;
            try
            {
                var res = await WinFormsApiClient.Instance.UpdateCourtStatusAsync(court.Id, newStatus);
                if (res.Success)
                {
                    ToastNotifier.Show(this, $"Đã cập nhật trạng thái sân '{court.Name}'!", ToastType.Success);
                    await LoadReportDataAsync();
                }
                else
                {
                    MessageBox.Show(res.Message ?? "Không thể cập nhật trạng thái sân.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnToggleMaintenance.Enabled = true;
            }
        };
    }

    private async Task LoadReportDataAsync()
    {
        try
        {
            // Tải thông tin chi nhánh
            var branchListRes = await WinFormsApiClient.Instance.GetBranchesAsync();
            if (branchListRes.Success && branchListRes.Data != null)
            {
                _currentBranch = branchListRes.Data.FirstOrDefault(b => b.Id == _branchId);
            }

            // 1. Tải danh sách sân
            var courtRes = await WinFormsApiClient.Instance.GetCourtsByBranchAsync(_branchId);
            if (courtRes.Success && courtRes.Data != null)
            {
                _courts = courtRes.Data;
                dgvCourts.Rows.Clear();
                foreach (var c in _courts)
                {
                    var isMaint = c.Status == SlotStatus.Maintenance;
                    var statusText = isMaint ? "🔧 ĐANG BẢO TRÌ" : "🟢 SẴN SÀNG HOẠT ĐỘNG";

                    string courtDisplayName = c.Name;
                    string courtLocation = "-";
                    int openParen = c.Name.IndexOf('(');
                    int closeParen = c.Name.LastIndexOf(')');
                    if (openParen > 0 && closeParen > openParen)
                    {
                        courtDisplayName = c.Name.Substring(0, openParen).Trim();
                        courtLocation = c.Name.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                    }
                    else if (_currentBranch != null && !string.IsNullOrEmpty(_currentBranch.Address))
                    {
                        courtLocation = _currentBranch.Address;
                    }

                    var rowIndex = dgvCourts.Rows.Add(
                        courtDisplayName,
                        courtLocation,
                        c.SportType.ToString(),
                        c.SurfaceType.ToString(),
                        $"{c.DefaultPrice:N0} đ/h",
                        statusText
                    );

                    var row = dgvCourts.Rows[rowIndex];
                    row.Tag = c;

                    if (isMaint)
                    {
                        row.DefaultCellStyle.BackColor = AppTheme.DangerLight;
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = AppTheme.SuccessLight;
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(6, 95, 70);
                    }
                }
            }

            // 2. Tải danh sách đơn đặt trong ngày
            var bookRes = await WinFormsApiClient.Instance.GetBranchBookingsAsync(_branchId, DateOnly.FromDateTime(DateTime.Today));
            if (bookRes.Success && bookRes.Data != null)
            {
                _branchBookings = bookRes.Data.OrderByDescending(b => b.CreatedAt).ToList();
                dgvBranchBookings.Rows.Clear();

                decimal totalRevenue = 0;
                int completedCount = 0;
                int activeCount = 0;
                int noShowCount = 0;

                foreach (var b in _branchBookings)
                {
                    // Tính doanh thu chuẩn xác theo BR-INTEGRITY
                    switch (b.Status)
                    {
                        case BookingStatus.Completed:
                        case BookingStatus.InUse:
                            totalRevenue += b.TotalAmount + b.ServiceAmount;
                            if (b.Status == BookingStatus.Completed) completedCount++;
                            else activeCount++;
                            break;
                        case BookingStatus.Confirmed:
                            totalRevenue += b.DepositAmount;
                            activeCount++;
                            break;
                        case BookingStatus.NoShow:
                            totalRevenue += b.DepositAmount;
                            noShowCount++;
                            break;
                        case BookingStatus.Cancelled:
                            var retained = b.DepositAmount - b.RefundAmount;
                            if (retained > 0) totalRevenue += retained;
                            noShowCount++;
                            break;
                    }

                    var statusText = b.Status switch
                    {
                        BookingStatus.InUse => "🎾 Đang Chơi",
                        BookingStatus.Confirmed => "⏳ Đã Cọc",
                        BookingStatus.Completed => "🏁 Hoàn Tất",
                        BookingStatus.NoShow => "⚠️ Vắng Mặt",
                        BookingStatus.Cancelled => "❌ Đã Hủy",
                        _ => b.Status.ToString()
                    };

                    var rowIndex = dgvBranchBookings.Rows.Add(
                        b.BookingCode,
                        b.CustomerName,
                        b.CourtName,
                        string.Join(", ", b.SlotLabels),
                        statusText,
                        $"{b.TotalAmount:N0} đ",
                        $"{b.DepositAmount:N0} đ",
                        b.CreatedAt.ToLocalTime().ToString("HH:mm:ss")
                    );

                    dgvBranchBookings.Rows[rowIndex].Tag = b;
                }

                // Cập nhật thẻ KPI
                cardRevenue.Value = $"{totalRevenue:N0} đ";
                cardCompleted.Value = $"{completedCount} ca";
                cardActive.Value = $"{activeCount} ca";
                cardNoShow.Value = $"{noShowCount} đơn";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải dữ liệu báo cáo: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
