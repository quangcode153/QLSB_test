using System.Drawing;
using System.Windows.Forms;
using SportChain.Shared.DTOs.Bookings;
using SportChain.Shared.Enums;
using SportChain.WinForms.Forms;
using SportChain.WinForms.Services;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UserControls;

public partial class MyBookingsControl : UserControl
{
    private List<BookingDto> _bookings = new();
    private BookingDto? _selectedBooking;

    public MyBookingsControl()
    {
        InitializeComponent();
        SetupGridColumns();
        SetupEvents();
        ClearSelection();
    }

    public async Task LoadMyBookingsAsync()
    {
        try
        {
            var res = await WinFormsApiClient.Instance.GetMyBookingsAsync();
            if (res.Success && res.Data != null)
            {
                _bookings = res.Data.OrderByDescending(b => b.CreatedAt).ToList();
                RenderBookings();
            }
            else
            {
                MessageBox.Show(res.Message ?? "Không thể tải danh sách đơn.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SetupGridColumns()
    {
        dgvBookings.Columns.Clear();
        dgvBookings.AutoGenerateColumns = false;
        dgvBookings.RowTemplate.Height = 42;
        dgvBookings.ColumnHeadersHeight = 44;
        dgvBookings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgvBookings.EnableHeadersVisualStyles = false;
        dgvBookings.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.SidebarDarker;
        dgvBookings.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvBookings.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontCaptionBold;
        dgvBookings.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvBookings.DefaultCellStyle.Font = AppTheme.FontCaption;
        dgvBookings.DefaultCellStyle.SelectionBackColor = AppTheme.PrimaryLight;
        dgvBookings.DefaultCellStyle.SelectionForeColor = AppTheme.TextMain;

        // Bật DoubleBuffered qua Reflection để chống giật nháy hình khi cuộn bảng
        try
        {
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, dgvBookings, new object[] { true });
        }
        catch { }

        var colCode = new DataGridViewTextBoxColumn { Name = "BookingCode", HeaderText = "Mã Đơn", MinimumWidth = 145, FillWeight = 13 };
        var colQr = new DataGridViewTextBoxColumn { Name = "CheckInCode", HeaderText = "Mã Check-in", MinimumWidth = 120, FillWeight = 11, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } };
        var colBranch = new DataGridViewTextBoxColumn { Name = "BranchName", HeaderText = "Chi Nhánh", MinimumWidth = 140, FillWeight = 14 };
        var colCourt = new DataGridViewTextBoxColumn { Name = "CourtName", HeaderText = "Sân Đấu", MinimumWidth = 130, FillWeight = 12 };
        var colDate = new DataGridViewTextBoxColumn { Name = "BookingDate", HeaderText = "Ngày Chơi", MinimumWidth = 95, FillWeight = 9, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } };
        var colSlots = new DataGridViewTextBoxColumn { Name = "Slots", HeaderText = "Khung Giờ", MinimumWidth = 110, FillWeight = 10, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } };
        var colStatus = new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng Thái", MinimumWidth = 135, FillWeight = 13, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } };
        var colDeposit = new DataGridViewTextBoxColumn { Name = "DepositAmount", HeaderText = "Đã Cọc", MinimumWidth = 100, FillWeight = 9, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } };
        var colTotal = new DataGridViewTextBoxColumn { Name = "TotalAmount", HeaderText = "Tổng Tiền", MinimumWidth = 100, FillWeight = 9, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } };
        var colCreated = new DataGridViewTextBoxColumn { Name = "CreatedAt", HeaderText = "Thời Điểm Đặt", MinimumWidth = 130, FillWeight = 12, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } };

        dgvBookings.Columns.AddRange(colCode, colQr, colBranch, colCourt, colDate, colSlots, colStatus, colDeposit, colTotal, colCreated);
    }

    private void SetupEvents()
    {
        btnRefresh.Click += async (s, e) => await LoadMyBookingsAsync();

        dgvBookings.SelectionChanged += (s, e) =>
        {
            if (dgvBookings.SelectedRows.Count > 0 && dgvBookings.SelectedRows[0].Tag is BookingDto b)
            {
                SelectBooking(b);
            }
            else
            {
                ClearSelection();
            }
        };

        dgvBookings.CellClick += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvBookings.Rows.Count && dgvBookings.Rows[e.RowIndex].Tag is BookingDto b)
            {
                SelectBooking(b);
            }
        };

        // Nhấp đúp chuột vào hàng để mở ngay thông tin chi tiết / vé điện tử
        dgvBookings.CellDoubleClick += (s, e) =>
        {
            if (e.RowIndex >= 0 && dgvBookings.Rows[e.RowIndex].Tag is BookingDto b)
            {
                using var ticketForm = new TicketModalForm(b);
                ticketForm.ShowDialog(this.FindForm());
            }
        };

        btnViewTicket.Click += (s, e) =>
        {
            if (_selectedBooking == null)
            {
                MessageBox.Show("Vui lòng chọn một đơn đặt sân trong bảng danh sách.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var ticketForm = new TicketModalForm(_selectedBooking);
            ticketForm.ShowDialog(this.FindForm());
        };

        btnCopyCode.Click += (s, e) =>
        {
            if (_selectedBooking == null)
            {
                MessageBox.Show("Vui lòng chọn một đơn đặt sân trong bảng danh sách.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var code = string.IsNullOrEmpty(_selectedBooking.CheckInCode) ? _selectedBooking.BookingCode : _selectedBooking.CheckInCode;
            Clipboard.SetText(code);
            ToastNotifier.Show(this, $"Đã sao chép mã {code} vào bộ nhớ tạm!", ToastType.Success);
        };

        btnPayDeposit.Click += (s, e) =>
        {
            if (_selectedBooking == null)
            {
                MessageBox.Show("Vui lòng chọn một đơn đặt sân trong bảng danh sách.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_selectedBooking.Status != BookingStatus.PendingPayment)
            {
                MessageBox.Show($"Đơn hàng này đang ở trạng thái '{_selectedBooking.Status}', không cần nộp tiền cọc.",
                    "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_selectedBooking.ExpireHoldingAt <= DateTime.UtcNow)
            {
                MessageBox.Show("Thời gian giữ chỗ 15 phút của ca này đã hết hạn. Đơn đặt đã tự động hủy.",
                    "Hết Hạn Giữ Chỗ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _ = LoadMyBookingsAsync();
                return;
            }

            using var depForm = new DepositModalForm(_selectedBooking);
            var res = depForm.ShowDialog(this.FindForm());
            if (res == DialogResult.OK || res == DialogResult.Abort)
            {
                _ = LoadMyBookingsAsync();
            }
        };

        btnCancel.Click += async (s, e) =>
        {
            if (_selectedBooking == null)
            {
                MessageBox.Show("Vui lòng chọn một đơn đặt sân trong bảng danh sách.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_selectedBooking.Status == BookingStatus.Cancelled)
            {
                MessageBox.Show("Đơn đặt sân này đã được hủy trước đó rồi.",
                    "Đơn Đã Hủy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_selectedBooking.Status == BookingStatus.Completed || _selectedBooking.Status == BookingStatus.InUse)
            {
                MessageBox.Show("Trận đấu đang thi đấu hoặc đã hoàn tất, không thể hủy đơn.",
                    "Không Thể Hủy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_selectedBooking.Status == BookingStatus.NoShow)
            {
                MessageBox.Show("Đơn đặt này đã bị đánh dấu vắng mặt (No-Show), không thể hủy.",
                    "Không Thể Hủy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await HandleCancelBookingAsync();
        };
    }

    private void RenderBookings()
    {
        dgvBookings.Rows.Clear();
        foreach (var b in _bookings)
        {
            var statusLabel = b.Status switch
            {
                BookingStatus.PendingPayment => "⏳ Chờ đặt cọc (15p)",
                BookingStatus.Confirmed => "✅ Đã xác nhận cọc",
                BookingStatus.InUse => "🎾 Đang thi đấu",
                BookingStatus.Completed => "🏁 Đã hoàn tất",
                BookingStatus.Cancelled => "❌ Đã hủy",
                BookingStatus.NoShow => "⚠️ Vắng mặt (No-show)",
                _ => b.Status.ToString()
            };

            string qrDisplay;
            if (!string.IsNullOrEmpty(b.CheckInCode))
            {
                qrDisplay = $"🎟️ {b.CheckInCode}";
            }
            else if (b.Status == BookingStatus.PendingPayment)
            {
                qrDisplay = "⏳ Cần nộp cọc";
            }
            else
            {
                qrDisplay = "—";
            }

            var rowIndex = dgvBookings.Rows.Add(
                b.BookingCode,
                qrDisplay,
                b.BranchName,
                b.CourtName,
                b.BookingDate.ToString("dd/MM/yyyy"),
                string.Join(", ", b.SlotLabels),
                statusLabel,
                $"{b.DepositAmount:N0} đ",
                $"{b.TotalAmount:N0} đ",
                b.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
            );

            var row = dgvBookings.Rows[rowIndex];
            row.Tag = b;

            // Màu sắc theo trạng thái
            switch (b.Status)
            {
                case BookingStatus.PendingPayment:
                    row.DefaultCellStyle.BackColor = AppTheme.WarningLight;
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(146, 64, 14);
                    break;
                case BookingStatus.Confirmed:
                    row.DefaultCellStyle.BackColor = AppTheme.SuccessLight;
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(6, 95, 70);
                    break;
                case BookingStatus.InUse:
                    row.DefaultCellStyle.BackColor = AppTheme.InUseLight;
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(107, 33, 168);
                    break;
                case BookingStatus.Completed:
                    row.DefaultCellStyle.BackColor = AppTheme.CardBgAlt;
                    row.DefaultCellStyle.ForeColor = AppTheme.TextMuted;
                    break;
                case BookingStatus.Cancelled:
                case BookingStatus.NoShow:
                    row.DefaultCellStyle.BackColor = AppTheme.DangerLight;
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                    break;
            }
        }

        if (_bookings.Count > 0)
        {
            dgvBookings.Rows[0].Selected = true;
            SelectBooking(_bookings[0]);
        }
        else
        {
            ClearSelection();
        }
    }

    private void SelectBooking(BookingDto b)
    {
        _selectedBooking = b;

        switch (b.Status)
        {
            case BookingStatus.PendingPayment:
                int remainingMin = b.ExpireHoldingAt > DateTime.UtcNow ? (int)(b.ExpireHoldingAt - DateTime.UtcNow).TotalMinutes : 0;
                lblSelectedInfo.Text = $"Đơn: {b.BookingCode} ({b.CourtName}) • ⏳ Giữ chỗ (Còn {remainingMin}p nộp cọc)";
                lblSelectedInfo.ForeColor = Color.FromArgb(180, 83, 9);
                break;
            case BookingStatus.Confirmed:
                lblSelectedInfo.Text = $"Đơn: {b.BookingCode} ({b.CourtName}) • ✅ Đã cọc {b.DepositAmount:N0}đ | Mã Check-in: {b.CheckInCode}";
                lblSelectedInfo.ForeColor = Color.FromArgb(6, 95, 70);
                break;
            case BookingStatus.InUse:
                lblSelectedInfo.Text = $"Đơn: {b.BookingCode} ({b.CourtName}) • 🎾 Đang thi đấu trên sân | Mã Check-in: {b.CheckInCode}";
                lblSelectedInfo.ForeColor = Color.FromArgb(107, 33, 168);
                break;
            case BookingStatus.Completed:
                lblSelectedInfo.Text = $"Đơn: {b.BookingCode} ({b.CourtName}) • 🏁 Trận đấu đã hoàn tất.";
                lblSelectedInfo.ForeColor = AppTheme.TextMuted;
                break;
            case BookingStatus.Cancelled:
                lblSelectedInfo.Text = $"Đơn: {b.BookingCode} ({b.CourtName}) • ❌ Đơn đã hủy.";
                lblSelectedInfo.ForeColor = AppTheme.Danger;
                break;
            case BookingStatus.NoShow:
                lblSelectedInfo.Text = $"Đơn: {b.BookingCode} ({b.CourtName}) • ⚠️ Đã ghi nhận vắng mặt (No-Show).";
                lblSelectedInfo.ForeColor = AppTheme.Danger;
                break;
            default:
                lblSelectedInfo.Text = $"Đơn: {b.BookingCode} ({b.CourtName}) • Trạng thái: {b.Status}";
                lblSelectedInfo.ForeColor = AppTheme.TextMain;
                break;
        }

        // Cập nhật trạng thái tương tác của các nút hành động
        btnViewTicket.Enabled = true;
        btnCopyCode.Enabled = true;
        btnPayDeposit.Enabled = (b.Status == BookingStatus.PendingPayment && b.ExpireHoldingAt > DateTime.UtcNow);
        btnCancel.Enabled = (b.Status == BookingStatus.PendingPayment || b.Status == BookingStatus.Confirmed);
    }

    private void ClearSelection()
    {
        _selectedBooking = null;
        lblSelectedInfo.Text = "Vui lòng chọn một đơn đặt trong bảng để thao tác.";
        lblSelectedInfo.ForeColor = AppTheme.TextMuted;

        btnViewTicket.Enabled = false;
        btnCopyCode.Enabled = false;
        btnPayDeposit.Enabled = false;
        btnCancel.Enabled = false;
    }

    private async Task HandleCancelBookingAsync()
    {
        if (_selectedBooking == null) return;

        var msg = $"Bạn có chắc chắn muốn hủy đơn đặt sân {_selectedBooking.BookingCode}?\n\n" +
                  "Chính sách hoàn cọc:\n" +
                  "• Trước giờ chơi >= 24 giờ: Hoàn 100% tiền cọc\n" +
                  "• Trước giờ chơi từ 12 đến 24 giờ: Hoàn 50% tiền cọc\n" +
                  "• Sát giờ chơi < 12 giờ: Phạt 100% tiền cọc (Không hoàn lại)\n" +
                  "• Đơn chưa đặt cọc (Pending): Miễn phí hủy.";

        var confirm = MessageBox.Show(msg, "Xác Nhận Hủy Đơn", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        btnCancel.Enabled = false;
        try
        {
            var res = await WinFormsApiClient.Instance.CancelBookingAsync(_selectedBooking.Id, new CancelBookingRequest
            {
                BookingId = _selectedBooking.Id,
                Reason = "Khách hàng chủ động hủy qua ứng dụng Desktop"
            });

            if (res.Success)
            {
                ToastNotifier.Show(this, res.Message ?? "Đã hủy đơn đặt sân thành công!", ToastType.Success);
                await LoadMyBookingsAsync();
            }
            else
            {
                MessageBox.Show(res.Message ?? "Không thể hủy đơn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi hủy đơn: {ex.Message}", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnCancel.Enabled = true;
        }
    }
}

