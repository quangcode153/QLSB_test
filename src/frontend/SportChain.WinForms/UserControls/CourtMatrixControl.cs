using System.Data;
using System.Reflection;
using SportChain.Shared.DTOs.Bookings;
using SportChain.Shared.DTOs.Courts;
using SportChain.Shared.Enums;
using SportChain.WinForms.Common;
using SportChain.WinForms.Forms;
using SportChain.WinForms.Services;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UserControls;

public partial class CourtMatrixControl : UserControl
{
    private int _branchId = 1;
    private DateOnly _currentDate = DateOnly.FromDateTime(DateTime.Now);
    private SportType? _selectedSportType = null;
    private BranchMatrixDto? _matrixData;

    private class SelectedSlotItem
    {
        public int CourtId { get; set; }
        public string CourtName { get; set; } = string.Empty;
        public int TimeSlotId { get; set; }
        public string TimeLabel { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    private readonly List<SelectedSlotItem> _selectedSlots = new();
    private BookingDto? _activeHoldingBooking;
    private System.Windows.Forms.Timer? _countdownTimer;

    public CourtMatrixControl()
    {
        InitializeComponent();
        EnableDoubleBuffering(dgvMatrix);
        SetupEventHandlers();
        SetupRealtimeListener();
    }

    private static void EnableDoubleBuffering(DataGridView dgv)
    {
        typeof(DataGridView).InvokeMember("DoubleBuffered",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
            null, dgv, new object[] { true });
    }

    private void SetupEventHandlers()
    {
        dtpDate.ValueChanged += async (s, e) =>
        {
            _currentDate = DateOnly.FromDateTime(dtpDate.Value);
            await LoadMatrixDataAsync(_branchId, _currentDate);
        };

        btnToday.Click += (s, e) => SetQuickDate(0);
        btnTomorrow.Click += (s, e) => SetQuickDate(1);
        btnPlus2.Click += (s, e) => SetQuickDate(2);
        btnPlus3.Click += (s, e) => SetQuickDate(3);

        btnRefresh.Click += async (s, e) => await LoadMatrixDataAsync(_branchId, _currentDate);

        btnSportAll.Click += (s, e) => SetSportFilter(null, btnSportAll);
        btnSportPickleball.Click += (s, e) => SetSportFilter(SportType.Pickleball, btnSportPickleball);
        btnSportBadminton.Click += (s, e) => SetSportFilter(SportType.Badminton, btnSportBadminton);
        btnSportFootball.Click += (s, e) => SetSportFilter(SportType.Football, btnSportFootball);
        btnSportTennis.Click += (s, e) => SetSportFilter(SportType.Tennis, btnSportTennis);

        dgvMatrix.CellClick += DgvMatrix_CellClick;
        dgvMatrix.CellFormatting += DgvMatrix_CellFormatting;

        btnClearSelection.Click += (s, e) => ClearSelection();
        btnHoldAndDeposit.Click += async (s, e) => await HandleHoldAndDepositAsync();

        btnPayDeposit.Click += (s, e) =>
        {
            if (_activeHoldingBooking != null)
            {
                OpenDepositModal(_activeHoldingBooking);
            }
        };

        btnCancelHolding.Click += async (s, e) => await HandleCancelHoldingAsync();

        this.Load += (s, e) =>
        {
            LayoutBottomBar();
            LayoutHoldingBanner();
        };

        this.Resize += (s, e) =>
        {
            LayoutBottomBar();
            LayoutHoldingBanner();
        };
    }

    public void LayoutBottomBar()
    {
        if (pnlBottomBar == null || btnHoldAndDeposit == null || btnClearSelection == null) return;

        int padRight = 16;
        int btnGap = 10;
        int btnWidth = 280;

        btnHoldAndDeposit.Size = new Size(btnWidth, 42);
        btnHoldAndDeposit.Top = Math.Max(4, (pnlBottomBar.ClientSize.Height - btnHoldAndDeposit.Height) / 2);
        btnHoldAndDeposit.Left = Math.Max(260, pnlBottomBar.ClientSize.Width - btnHoldAndDeposit.Width - padRight);

        btnClearSelection.Size = new Size(110, 42);
        btnClearSelection.Top = btnHoldAndDeposit.Top;
        btnClearSelection.Left = Math.Max(140, btnHoldAndDeposit.Left - btnClearSelection.Width - btnGap);

        int textWidth = Math.Max(120, btnClearSelection.Left - 30);
        lblSelectedSummary.Width = textWidth;
        lblPriceSummary.Width = textWidth;
    }

    public void LayoutHoldingBanner()
    {
        if (pnlHoldingBanner == null || btnPayDeposit == null || btnCancelHolding == null || lblHoldingTimer == null) return;

        int padRight = 16;
        int btnGap = 8;

        btnCancelHolding.Size = new Size(100, 36);
        btnCancelHolding.Top = Math.Max(4, (pnlHoldingBanner.ClientSize.Height - btnCancelHolding.Height) / 2);
        btnCancelHolding.Left = Math.Max(200, pnlHoldingBanner.ClientSize.Width - btnCancelHolding.Width - padRight);

        btnPayDeposit.Size = new Size(140, 36);
        btnPayDeposit.Top = btnCancelHolding.Top;
        btnPayDeposit.Left = Math.Max(100, btnCancelHolding.Left - btnPayDeposit.Width - btnGap);

        lblHoldingTimer.Top = Math.Max(4, (pnlHoldingBanner.ClientSize.Height - lblHoldingTimer.Height) / 2);
        lblHoldingTimer.Left = Math.Max(50, btnPayDeposit.Left - lblHoldingTimer.Width - btnGap - 4);

        int maxTextWidth = Math.Max(80, lblHoldingTimer.Left - lblHoldingText.Left - 10);
        lblHoldingText.MaximumSize = new Size(maxTextWidth, 40);
    }

    private void SetupRealtimeListener()
    {
        WinFormsRealtimeService.Instance.OnSlotStatusChanged += HandleRealtimeSlotChanged;
    }

    private void CleanupResources()
    {
        WinFormsRealtimeService.Instance.OnSlotStatusChanged -= HandleRealtimeSlotChanged;
        _countdownTimer?.Stop();
        _countdownTimer?.Dispose();
        _countdownTimer = null;
    }

    public async Task LoadMatrixDataAsync(int branchId, DateOnly? date = null)
    {
        _branchId = branchId;
        if (date.HasValue)
        {
            _currentDate = date.Value;
            dtpDate.Value = new DateTime(_currentDate.Year, _currentDate.Month, _currentDate.Day);
        }

        ClearSelection();
        dgvMatrix.Cursor = Cursors.WaitCursor;

        try
        {
            // 1. Tải ma trận sân chi nhánh
            var matrixRes = await WinFormsApiClient.Instance.GetBranchMatrixAsync(_branchId, _currentDate);
            if (matrixRes.Success && matrixRes.Data != null)
            {
                _matrixData = matrixRes.Data;
            }
            else
            {
                _matrixData = null;
                MessageBox.Show(matrixRes.Message ?? "Không thể tải ma trận lịch sân.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // 2. Kiểm tra xem user hiện tại có đơn giữ chỗ (holding) nào còn hiệu lực không
            await CheckActiveHoldingBookingAsync();

            // 3. Render lên bảng
            RenderMatrixGrid();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải ma trận lịch sân: {ex.Message}", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            dgvMatrix.Cursor = Cursors.Default;
        }
    }

    private async Task CheckActiveHoldingBookingAsync()
    {
        try
        {
            if (!SessionContext.IsLoggedIn)
            {
                HideHoldingBanner();
                return;
            }

            var myBookingsRes = await WinFormsApiClient.Instance.GetMyBookingsAsync();
            if (myBookingsRes.Success && myBookingsRes.Data != null)
            {
                var holding = myBookingsRes.Data
                    .FirstOrDefault(b => b.Status == BookingStatus.PendingPayment && b.ExpireHoldingAt > DateTime.UtcNow);

                if (holding != null)
                {
                    _activeHoldingBooking = holding;
                    ShowHoldingBanner(holding);
                    return;
                }
            }

            HideHoldingBanner();
        }
        catch
        {
            HideHoldingBanner();
        }
    }

    private void ShowHoldingBanner(BookingDto holding)
    {
        _activeHoldingBooking = holding;
        pnlHoldingBanner.Visible = true;
        lblHoldingText.Text = $"Bạn đang giữ chỗ: {holding.CourtName} ({string.Join(", ", holding.SlotLabels)}) | Cọc 30%: {holding.DepositAmount:N0} đ";

        StartCountdownTimer(holding.ExpireHoldingAt);
    }

    private void HideHoldingBanner()
    {
        _activeHoldingBooking = null;
        pnlHoldingBanner.Visible = false;
        _countdownTimer?.Stop();
        _countdownTimer?.Dispose();
        _countdownTimer = null;
    }

    private void StartCountdownTimer(DateTime expireAtUtc)
    {
        _countdownTimer?.Stop();
        _countdownTimer?.Dispose();

        // Chuẩn hóa múi giờ UTC tuyệt đối tránh lệch 7 tiếng
        var normalizedUtc = expireAtUtc.Kind == DateTimeKind.Utc
            ? expireAtUtc
            : DateTime.SpecifyKind(expireAtUtc, DateTimeKind.Utc);

        _countdownTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _countdownTimer.Tick += (s, e) =>
        {
            var remaining = normalizedUtc - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                _countdownTimer.Stop();
                lblHoldingTimer.Text = "Hết hạn giữ chỗ";
                HideHoldingBanner();
                _ = LoadMatrixDataAsync(_branchId, _currentDate);
            }
            else
            {
                lblHoldingTimer.Text = $"Còn lại: {remaining.Minutes:D2}:{remaining.Seconds:D2}";
            }
        };
        _countdownTimer.Start();
    }

    private void SetQuickDate(int daysFromToday)
    {
        var targetDate = DateTime.Today.AddDays(daysFromToday);
        dtpDate.Value = targetDate;
    }

    private void SetSportFilter(SportType? sport, Button activeButton)
    {
        _selectedSportType = sport;

        // Nếu giỏ hàng đang chọn ca của sân thuộc môn khác, hủy chọn để tránh đặt nhầm
        if (_selectedSlots.Count > 0 && sport.HasValue)
        {
            var currentCourt = _matrixData?.Rows.FirstOrDefault(r => r.CourtId == _selectedSlots[0].CourtId);
            if (currentCourt != null && currentCourt.SportType != sport.Value)
            {
                ClearSelection();
            }
        }

        // Reset visual state of all sport buttons
        var buttons = new[] { btnSportAll, btnSportPickleball, btnSportBadminton, btnSportFootball, btnSportTennis };
        foreach (var b in buttons)
        {
            b.BackColor = AppTheme.CardBgAlt;
            b.ForeColor = AppTheme.TextMuted;
        }

        activeButton.BackColor = AppTheme.Primary;
        activeButton.ForeColor = Color.White;

        RenderMatrixGrid();
    }

    private void RenderMatrixGrid()
    {
        dgvMatrix.SuspendLayout();
        dgvMatrix.Columns.Clear();
        dgvMatrix.Rows.Clear();

        if (_matrixData == null || _matrixData.Rows.Count == 0)
        {
            dgvMatrix.ResumeLayout();
            return;
        }

        // Cột 0: Tên Sân & Phân Loại
        var colCourt = new DataGridViewTextBoxColumn
        {
            Name = "ColCourt",
            HeaderText = "🏟️ TÊN SÂN / BỘ MÔN",
            Width = 180,
            Frozen = true,
            ReadOnly = true,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = AppTheme.CardBg,
                Font = AppTheme.FontBodyBold,
                ForeColor = AppTheme.TextMain,
                WrapMode = DataGridViewTriState.True
            }
        };
        dgvMatrix.Columns.Add(colCourt);

        // Các cột khung giờ (TimeSlots)
        foreach (var header in _matrixData.TimeHeaders)
        {
            var headerLabel = header.DisplayLabel;
            if (header.IsPeakHour)
            {
                headerLabel += "\n⭐ Giờ vàng";
            }

            var colSlot = new DataGridViewTextBoxColumn
            {
                Name = $"Col_Slot_{header.Id}",
                HeaderText = headerLabel,
                Width = 92,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    WrapMode = DataGridViewTriState.True,
                    Font = AppTheme.FontSmall
                }
            };
            dgvMatrix.Columns.Add(colSlot);
        }

        // Thêm dữ liệu từng hàng sân
        var filteredRows = _selectedSportType.HasValue
            ? _matrixData.Rows.Where(r => r.SportType == _selectedSportType.Value).ToList()
            : _matrixData.Rows;

        foreach (var rowData in filteredRows)
        {
            var rowIndex = dgvMatrix.Rows.Add();
            var gridRow = dgvMatrix.Rows[rowIndex];
            gridRow.Tag = rowData;

            // Cột 0: Tên sân
            var sportEmoji = GetSportEmoji(rowData.SportType);
            gridRow.Cells[0].Value = $"{sportEmoji} {rowData.CourtName}\n({rowData.SurfaceType})";

            // Các cột slot
            for (int i = 0; i < _matrixData.TimeHeaders.Count; i++)
            {
                var timeHeader = _matrixData.TimeHeaders[i];
                var cellData = rowData.Slots.FirstOrDefault(s => s.TimeSlotId == timeHeader.Id);
                var cell = gridRow.Cells[i + 1];

                if (cellData != null)
                {
                    cell.Tag = cellData;
                    UpdateCellVisual(cell, cellData, rowData.CourtId);
                }
                else
                {
                    cell.Value = "-";
                }
            }
        }

        dgvMatrix.ResumeLayout();
    }

    private bool IsPastSlot(int timeSlotId)
    {
        if (_currentDate < DateOnly.FromDateTime(DateTime.Today)) return true;
        if (_currentDate > DateOnly.FromDateTime(DateTime.Today)) return false;

        var header = _matrixData?.TimeHeaders.FirstOrDefault(h => h.Id == timeSlotId);
        if (header != null)
        {
            return header.EndTime <= DateTime.Now.TimeOfDay;
        }
        return false;
    }

    private void UpdateCellVisual(DataGridViewCell cell, SlotMatrixCellDto cellData, int courtId)
    {
        // 1. Kiểm tra nếu là ca đã kết thúc trong ngày hôm nay
        if (IsPastSlot(cellData.TimeSlotId))
        {
            cell.Style.BackColor = AppTheme.CardBgAlt;
            cell.Style.ForeColor = AppTheme.TextMuted;
            cell.Style.Font = AppTheme.FontSmall;
            cell.Value = "Đã qua";
            cell.ToolTipText = "Ca thi đấu này đã kết thúc trong ngày hôm nay.";
            return;
        }

        var isSelected = _selectedSlots.Any(s => s.CourtId == courtId && s.TimeSlotId == cellData.TimeSlotId);
        var isMyHolding = cellData.Status == SlotStatus.Holding &&
            ((cellData.CustomerId.HasValue && SessionContext.CurrentUser != null && cellData.CustomerId == SessionContext.CurrentUser.UserId) ||
             (_activeHoldingBooking != null && cellData.BookingId == _activeHoldingBooking.Id));

        if (isSelected)
        {
            cell.Style.BackColor = AppTheme.Primary;
            cell.Style.ForeColor = Color.White;
            cell.Style.Font = AppTheme.FontSmallBold;
            cell.Value = $"✔ Đang chọn\n{cellData.Price:N0}đ";
            cell.ToolTipText = "Nhấp để bỏ chọn ca này";
        }
        else if (isMyHolding)
        {
            cell.Style.BackColor = Color.FromArgb(245, 158, 11);
            cell.Style.ForeColor = Color.White;
            cell.Style.Font = AppTheme.FontSmallBold;
            cell.Value = $"⏳ Bạn giữ\n(Bấm cọc)";
            cell.ToolTipText = "Ca của bạn đang được giữ chỗ trong 15 phút. Nhấp để đặt cọc ngay!";
        }
        else
        {
            switch (cellData.Status)
            {
                case SlotStatus.Available:
                    if (cellData.IsPeakHour)
                    {
                        cell.Style.BackColor = Color.FromArgb(254, 249, 195);
                        cell.Style.ForeColor = Color.FromArgb(133, 77, 14);
                        cell.Style.Font = AppTheme.FontSmallBold;
                        cell.Value = $"⭐ {cellData.Price:N0}đ\nTrống";
                    }
                    else
                    {
                        cell.Style.BackColor = AppTheme.SuccessLight;
                        cell.Style.ForeColor = Color.FromArgb(6, 95, 70);
                        cell.Style.Font = AppTheme.FontSmall;
                        cell.Value = $"{cellData.Price:N0}đ\nTrống";
                    }
                    cell.ToolTipText = "Ca còn trống. Nhấp để chọn đặt sân.";
                    break;
                case SlotStatus.Holding:
                    cell.Style.BackColor = AppTheme.WarningLight;
                    cell.Style.ForeColor = Color.FromArgb(146, 64, 14);
                    cell.Style.Font = AppTheme.FontSmall;
                    cell.Value = "⏳ Giữ cọc\n(15p)";
                    cell.ToolTipText = "Khách khác đang giữ chỗ trong 15 phút.";
                    break;
                case SlotStatus.Booked:
                    cell.Style.BackColor = AppTheme.InfoLight;
                    cell.Style.ForeColor = Color.FromArgb(3, 105, 161);
                    cell.Style.Font = AppTheme.FontSmall;
                    cell.Value = "🔒 Đã đặt\n(Đã cọc)";
                    cell.ToolTipText = "Ca này đã được thanh toán cọc/đặt lịch.";
                    break;
                case SlotStatus.InUse:
                    cell.Style.BackColor = AppTheme.InUseLight;
                    cell.Style.ForeColor = Color.FromArgb(109, 40, 217);
                    cell.Style.Font = AppTheme.FontSmallBold;
                    cell.Value = "⚡ Đang chơi";
                    cell.ToolTipText = "Khách đang chơi trên sân.";
                    break;
                case SlotStatus.Maintenance:
                default:
                    cell.Style.BackColor = AppTheme.CardBgAlt;
                    cell.Style.ForeColor = AppTheme.TextMuted;
                    cell.Value = "Bảo trì";
                    cell.ToolTipText = "Sân đang bảo trì.";
                    break;
            }
        }
    }

    private void DgvMatrix_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex <= 0) return;

        var gridRow = dgvMatrix.Rows[e.RowIndex];
        var rowData = gridRow.Tag as CourtMatrixRowDto;
        var cell = gridRow.Cells[e.ColumnIndex];
        var cellData = cell.Tag as SlotMatrixCellDto;

        if (rowData == null || cellData == null) return;

        if (IsPastSlot(cellData.TimeSlotId))
        {
            MessageBox.Show("Ca này đã qua khung giờ thi đấu trong ngày hôm nay!", "Không Khả Dụng", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var isMyHolding = cellData.Status == SlotStatus.Holding &&
            ((cellData.CustomerId.HasValue && SessionContext.CurrentUser != null && cellData.CustomerId == SessionContext.CurrentUser.UserId) ||
             (_activeHoldingBooking != null && cellData.BookingId == _activeHoldingBooking.Id));

        if (isMyHolding)
        {
            if (_activeHoldingBooking != null)
            {
                OpenDepositModal(_activeHoldingBooking);
            }
            return;
        }

        if (cellData.Status != SlotStatus.Available)
        {
            string statusDesc = cellData.Status switch
            {
                SlotStatus.Holding => "Đang có người giữ cọc (15 phút).",
                SlotStatus.Booked => "Đã được đặt trước.",
                SlotStatus.InUse => "Đang trong giờ thi đấu.",
                _ => "Không khả dụng."
            };
            MessageBox.Show($"Ca này hiện không thể chọn: {statusDesc}", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Toggle chọn ô
        var existingIndex = _selectedSlots.FindIndex(s => s.CourtId == rowData.CourtId && s.TimeSlotId == cellData.TimeSlotId);
        if (existingIndex >= 0)
        {
            // Bỏ chọn
            _selectedSlots.RemoveAt(existingIndex);
        }
        else
        {
            // Kiểm tra quy tắc: Không được chọn cùng lúc nhiều sân khác nhau trong 1 đơn
            if (_selectedSlots.Count > 0 && _selectedSlots[0].CourtId != rowData.CourtId)
            {
                var confirm = MessageBox.Show(
                    $"Bạn chỉ có thể đặt các ca trên cùng một sân trong một lần đặt.\n" +
                    $"Bạn đang chọn sân '{_selectedSlots[0].CourtName}'. Bạn có muốn hủy lựa chọn cũ để chuyển sang sân '{rowData.CourtName}'?",
                    "Xác Nhận Đổi Sân", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    ClearSelection();
                }
                else
                {
                    return;
                }
            }

            _selectedSlots.Add(new SelectedSlotItem
            {
                CourtId = rowData.CourtId,
                CourtName = rowData.CourtName,
                TimeSlotId = cellData.TimeSlotId,
                TimeLabel = cellData.TimeLabel,
                Price = cellData.Price
            });
        }

        UpdateCellVisual(cell, cellData, rowData.CourtId);
        UpdateBottomSummary();
    }

    private void DgvMatrix_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        // Formatting already handled in UpdateCellVisual
    }

    private void UpdateBottomSummary()
    {
        if (_selectedSlots.Count == 0)
        {
            lblSelectedSummary.Text = "Chưa chọn ca nào. Nhấp vào các ô màu xanh trên ma trận để chọn ca đặt sân.";
            lblPriceSummary.Text = "Tổng tiền: 0 đ | Cọc 30%: 0 đ";
            btnHoldAndDeposit.Enabled = false;
            btnHoldAndDeposit.Text = "THANH TOÁN CỌC 30%";
            return;
        }

        var courtName = _selectedSlots[0].CourtName;
        var slotLabels = string.Join(", ", _selectedSlots.Select(s => s.TimeLabel));
        var totalAmount = _selectedSlots.Sum(s => s.Price);
        var depositAmount = totalAmount * 0.3m;

        lblSelectedSummary.Text = $"Đã chọn: {courtName} | Ca: {slotLabels} ({_selectedSlots.Count} ca)";
        lblPriceSummary.Text = $"Tổng tiền: {totalAmount:N0} đ | Cọc 30%: {depositAmount:N0} đ";
        btnHoldAndDeposit.Enabled = true;
        btnHoldAndDeposit.Text = $"THANH TOÁN CỌC {depositAmount:N0}đ";
    }

    private void ClearSelection()
    {
        if (_selectedSlots.Count == 0) return;

        var copied = _selectedSlots.ToList();
        _selectedSlots.Clear();

        foreach (DataGridViewRow row in dgvMatrix.Rows)
        {
            if (row.Tag is CourtMatrixRowDto rowData && copied.Any(c => c.CourtId == rowData.CourtId))
            {
                for (int i = 1; i < row.Cells.Count; i++)
                {
                    var cell = row.Cells[i];
                    if (cell.Tag is SlotMatrixCellDto cellData && copied.Any(c => c.CourtId == rowData.CourtId && c.TimeSlotId == cellData.TimeSlotId))
                    {
                        UpdateCellVisual(cell, cellData, rowData.CourtId);
                    }
                }
            }
        }

        UpdateBottomSummary();
    }

    private async Task HandleHoldAndDepositAsync()
    {
        if (_selectedSlots.Count == 0) return;

        if (!SessionContext.IsLoggedIn)
        {
            MessageBox.Show("Vui lòng đăng nhập để tiến hành giữ chỗ và đặt sân!", "Yêu Cầu Đăng Nhập",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnHoldAndDeposit.Enabled = false;
        btnHoldAndDeposit.Text = "⏳ Đang tạo đơn cọc...";

        try
        {
            var req = new CreateBookingRequest
            {
                CustomerId = SessionContext.CurrentUser!.UserId,
                BranchId = _branchId,
                CourtId = _selectedSlots[0].CourtId,
                BookingDate = _currentDate,
                TimeSlotIds = _selectedSlots.Select(s => s.TimeSlotId).ToList()
            };

            var res = await WinFormsApiClient.Instance.CreateHoldingBookingAsync(req);
            if (res.Success && res.Data != null)
            {
                ClearSelection();
                ShowHoldingBanner(res.Data);
                OpenDepositModal(res.Data);
            }
            else
            {
                MessageBox.Show(res.Message ?? "Không thể tạo đơn giữ chỗ. Có thể có ca vừa bị người khác chọn trước!",
                    "Giữ Chỗ Thất Bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                await LoadMatrixDataAsync(_branchId, _currentDate);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi kết nối đặt sân: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnHoldAndDeposit.Enabled = _selectedSlots.Count > 0;
            if (_selectedSlots.Count > 0)
            {
                var deposit = _selectedSlots.Sum(s => s.Price) * 0.3m;
                btnHoldAndDeposit.Text = $"THANH TOÁN CỌC {deposit:N0}đ";
            }
            else
            {
                btnHoldAndDeposit.Text = "THANH TOÁN CỌC 30%";
            }
        }
    }

    private void OpenDepositModal(BookingDto booking)
    {
        using var depositForm = new DepositModalForm(booking);
        var result = depositForm.ShowDialog(this.FindForm());

        if (result == DialogResult.OK)
        {
            // Thanh toán cọc thành công!
            HideHoldingBanner();
            _ = LoadMatrixDataAsync(_branchId, _currentDate);

            // Mở vé điện tử QR check-in
            if (depositForm.ConfirmedBooking != null)
            {
                using var ticketForm = new TicketModalForm(depositForm.ConfirmedBooking);
                ticketForm.ShowDialog(this.FindForm());
            }
        }
        else if (result == DialogResult.Abort)
        {
            // Khách bấm hủy giữ chỗ
            HideHoldingBanner();
            _ = LoadMatrixDataAsync(_branchId, _currentDate);
        }
    }

    private async Task HandleCancelHoldingAsync()
    {
        if (_activeHoldingBooking == null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc chắn muốn hủy giữ chỗ cho đơn {_activeHoldingBooking.BookingCode}?\n" +
            $"Các ca sân sẽ được mở lại ngay lập tức cho những khách hàng khác.",
            "Xác Nhận Hủy Giữ Chỗ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        try
        {
            var res = await WinFormsApiClient.Instance.CancelBookingAsync(_activeHoldingBooking.Id, new CancelBookingRequest
            {
                BookingId = _activeHoldingBooking.Id,
                Reason = "Khách hàng chủ động hủy giữ chỗ qua WinForms"
            });

            if (res.Success)
            {
                var cancelledCourtId = _activeHoldingBooking.CourtId;
                var cancelledSlotIds = _activeHoldingBooking.TimeSlotIds.ToList();

                HideHoldingBanner();

                // Giải phóng tức thì trên lưới cục bộ
                foreach (DataGridViewRow row in dgvMatrix.Rows)
                {
                    if (row.Tag is CourtMatrixRowDto rowData && rowData.CourtId == cancelledCourtId)
                    {
                        for (int i = 1; i < row.Cells.Count; i++)
                        {
                            var cell = row.Cells[i];
                            if (cell.Tag is SlotMatrixCellDto cellData && cancelledSlotIds.Contains(cellData.TimeSlotId))
                            {
                                cellData.Status = SlotStatus.Available;
                                cellData.BookingId = null;
                                cellData.CustomerId = null;
                                cellData.BookingCode = null;
                                UpdateCellVisual(cell, cellData, cancelledCourtId);
                            }
                        }
                        break;
                    }
                }

                MessageBox.Show("Đã hủy giữ chỗ thành công. Ca sân đã được mở lại.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadMatrixDataAsync(_branchId, _currentDate);
            }
            else
            {
                MessageBox.Show(res.Message ?? "Không thể hủy đơn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi hủy đơn: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void HandleRealtimeSlotChanged(int courtId, int timeSlotId, SlotStatus newStatus, string? dateStr)
    {
        if (this.IsDisposed || !this.IsHandleCreated) return;

        // Nếu sự kiện có gắn ngày cụ thể và không trùng ngày đang xem -> Bỏ qua, tránh nhảy ô chéo ngày
        if (!string.IsNullOrEmpty(dateStr) && dateStr != _currentDate.ToString("yyyy-MM-dd"))
        {
            return;
        }

        this.BeginInvoke(() =>
        {
            // Kiểm tra xem ô thay đổi có đang được render trên lưới không
            foreach (DataGridViewRow row in dgvMatrix.Rows)
            {
                if (row.Tag is CourtMatrixRowDto rowData && rowData.CourtId == courtId)
                {
                    for (int i = 1; i < row.Cells.Count; i++)
                    {
                        var cell = row.Cells[i];
                        if (cell.Tag is SlotMatrixCellDto cellData && cellData.TimeSlotId == timeSlotId)
                        {
                            cellData.Status = newStatus;

                            // Nếu user đang chọn ca này mà người khác vừa cướp/khóa chỗ
                            if (_selectedSlots.Any(s => s.CourtId == courtId && s.TimeSlotId == timeSlotId) && newStatus != SlotStatus.Available)
                            {
                                _selectedSlots.RemoveAll(s => s.CourtId == courtId && s.TimeSlotId == timeSlotId);
                                UpdateBottomSummary();
                                MessageBox.Show($"Ca lúc {cellData.TimeLabel} trên {rowData.CourtName} vừa được người khác giữ hoặc đặt trước!",
                                    "Cập Nhật Thời Gian Thực", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            UpdateCellVisual(cell, cellData, courtId);
                            break;
                        }
                    }
                    break;
                }
            }
        });
    }

    private static string GetSportEmoji(SportType sport) => sport switch
    {
        SportType.Pickleball => "🏓",
        SportType.Badminton => "🏸",
        SportType.Football => "⚽",
        SportType.Tennis => "🎾",
        _ => "🏟️"
    };
}
