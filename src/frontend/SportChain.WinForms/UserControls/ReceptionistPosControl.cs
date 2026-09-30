using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using SportChain.Shared.DTOs.Bookings;
using SportChain.Shared.DTOs.Courts;
using SportChain.Shared.Enums;
using SportChain.WinForms.Services;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UserControls;

public partial class ReceptionistPosControl : UserControl
{
    private int _branchId = 1;
    private List<BookingDto> _activeBookings = new();
    private BookingDto? _selectedBooking;
    private List<ServiceItemDto> _serviceCatalog = new();
    private List<CourtDto> _courts = new();
    private List<TimeSlotDto> _availableTimeSlots = new();

    public ReceptionistPosControl()
    {
        InitializeComponent();
        SetupGridColumns();
        SetupEvents();
    }

    public async Task InitializeForBranchAsync(int branchId)
    {
        _branchId = branchId;
        await LoadAllDataAsync();
    }

    private void SetupGridColumns()
    {
        // Bật DoubleBuffered qua Reflection để chống giật nháy hình
        try
        {
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty;
            typeof(DataGridView).InvokeMember("DoubleBuffered", flags, null, dgvActiveBookings, new object[] { true });
            typeof(DataGridView).InvokeMember("DoubleBuffered", flags, null, dgvServices, new object[] { true });
        }
        catch { }

        // 1. dgvActiveBookings
        dgvActiveBookings.Columns.Clear();
        dgvActiveBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = "BookingCode", HeaderText = "Mã Đơn", Width = 110 });
        dgvActiveBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = "CustomerName", HeaderText = "Khách Hàng", Width = 130 });
        dgvActiveBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = "CourtName", HeaderText = "Sân", Width = 90 });
        dgvActiveBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = "Slots", HeaderText = "Khung Giờ", Width = 110 });
        dgvActiveBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng Thái", Width = 100 });
        dgvActiveBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = "DepositAmount", HeaderText = "Đã Cọc", Width = 90 });

        // 2. dgvServices
        dgvServices.Columns.Clear();
        dgvServices.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemName", HeaderText = "Dịch Vụ", Width = 140 });
        dgvServices.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "SL", Width = 45 });
        dgvServices.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "Đơn Giá", Width = 75 });
        dgvServices.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Thành Tiền", Width = 80 });

        // Payment Methods combo box
        cboWalkInPayment.DataSource = Enum.GetValues(typeof(PaymentMethod));
    }

    private void SetupEvents()
    {
        btnCheckIn.Click += async (s, e) => await HandleCheckInAsync();
        txtCheckInCode.KeyDown += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await HandleCheckInAsync();
            }
        };

        btnRefreshActive.Click += async (s, e) => await LoadActiveBookingsAsync();

        dgvActiveBookings.SelectionChanged += (s, e) =>
        {
            if (dgvActiveBookings.SelectedRows.Count > 0 && dgvActiveBookings.SelectedRows[0].Tag is BookingDto b)
            {
                SelectBooking(b);
            }
        };

        btnAddService.Click += async (s, e) => await HandleAddServiceAsync();
        btnCheckout.Click += async (s, e) => await HandleCheckoutAsync();
        btnPrintInvoice.Click += (s, e) => HandlePrintInvoice();

        // Walk-in tab
        cboWalkInCourt.SelectedIndexChanged += async (s, e) => await LoadWalkInSlotsForCourtAsync();
        chkWalkInSlots.ItemCheck += (s, e) =>
        {
            this.BeginInvoke(new Action(UpdateWalkInTotal));
        };
        btnCreateWalkIn.Click += async (s, e) => await HandleCreateWalkInAsync();
    }

    private async Task LoadAllDataAsync()
    {
        await LoadActiveBookingsAsync();
        await LoadServiceCatalogAsync();
        await LoadCourtsAsync();
    }

    private async Task LoadActiveBookingsAsync()
    {
        try
        {
            var res = await WinFormsApiClient.Instance.GetBranchBookingsAsync(_branchId, DateOnly.FromDateTime(DateTime.Today));
            if (res.Success && res.Data != null)
            {
                _activeBookings = res.Data
                    .Where(b => b.Status == BookingStatus.InUse || b.Status == BookingStatus.Confirmed)
                    .OrderByDescending(b => b.Status == BookingStatus.InUse)
                    .ThenBy(b => b.CreatedAt)
                    .ToList();

                dgvActiveBookings.Rows.Clear();
                foreach (var b in _activeBookings)
                {
                    var statusText = b.Status == BookingStatus.InUse ? "🎾 ĐANG CHƠI" : "⏳ ĐÃ CỌC (CHỜ)";
                    var rowIndex = dgvActiveBookings.Rows.Add(
                        b.BookingCode,
                        b.CustomerName,
                        b.CourtName,
                        string.Join(", ", b.SlotLabels),
                        statusText,
                        $"{b.DepositAmount:N0} đ"
                    );

                    var row = dgvActiveBookings.Rows[rowIndex];
                    row.Tag = b;

                    if (b.Status == BookingStatus.InUse)
                    {
                        row.DefaultCellStyle.BackColor = AppTheme.InUseLight;
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(107, 33, 168);
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = AppTheme.WarningLight;
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(146, 64, 14);
                    }
                }

                if (_activeBookings.Count > 0)
                {
                    dgvActiveBookings.Rows[0].Selected = true;
                    SelectBooking(_activeBookings[0]);
                }
                else
                {
                    ClearBookingDetail();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải danh sách sân: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadServiceCatalogAsync()
    {
        try
        {
            var res = await WinFormsApiClient.Instance.GetBranchServicesAsync(_branchId);
            if (res.Success && res.Data != null)
            {
                _serviceCatalog = res.Data;
                cboServiceCatalog.DisplayMember = "DisplayName";
                cboServiceCatalog.ValueMember = "Id";

                var list = _serviceCatalog.Select(s => new
                {
                    s.Id,
                    DisplayName = $"{s.Name} ({s.Price:N0} đ)"
                }).ToList();

                cboServiceCatalog.DataSource = list;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoadServiceCatalog] {ex.Message}");
        }
    }

    private async Task LoadCourtsAsync()
    {
        try
        {
            var res = await WinFormsApiClient.Instance.GetCourtsByBranchAsync(_branchId);
            if (res.Success && res.Data != null)
            {
                _courts = res.Data.Where(c => c.Status != SlotStatus.Maintenance).ToList();
                cboWalkInCourt.DisplayMember = "Name";
                cboWalkInCourt.ValueMember = "Id";
                cboWalkInCourt.DataSource = _courts;

                if (_courts.Count > 0)
                {
                    cboWalkInCourt.SelectedIndex = 0;
                    await LoadWalkInSlotsForCourtAsync();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoadCourts] {ex.Message}");
        }
    }

    private async Task LoadWalkInSlotsForCourtAsync()
    {
        chkWalkInSlots.Items.Clear();
        if (cboWalkInCourt.SelectedValue is not int courtId) return;

        try
        {
            var matrixRes = await WinFormsApiClient.Instance.GetBranchMatrixAsync(_branchId, DateOnly.FromDateTime(DateTime.Today));
            if (matrixRes.Success && matrixRes.Data != null)
            {
                var courtRow = matrixRes.Data.Rows.FirstOrDefault(r => r.CourtId == courtId);
                if (courtRow != null)
                {
                    var nowTime = DateTime.Now.TimeOfDay;
                    foreach (var slot in courtRow.Slots)
                    {
                        // Chỉ hiển thị ca chưa kết thúc và đang trống Available
                        if (slot.Status == SlotStatus.Available)
                        {
                            var header = matrixRes.Data.TimeHeaders.FirstOrDefault(h => h.Id == slot.TimeSlotId);
                            if (header != null && header.EndTime > nowTime)
                            {
                                chkWalkInSlots.Items.Add(new WalkInSlotItem
                                {
                                    TimeSlotId = slot.TimeSlotId,
                                    Display = $"{slot.TimeLabel} - {slot.Price:N0} đ",
                                    Price = slot.Price
                                });
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoadWalkInSlots] {ex.Message}");
        }

        UpdateWalkInTotal();
    }

    private void UpdateWalkInTotal()
    {
        decimal total = 0;
        foreach (var item in chkWalkInSlots.CheckedItems)
        {
            if (item is WalkInSlotItem slot)
            {
                total += slot.Price;
            }
        }

        lblWalkInTotal.Text = $"TỔNG TIỀN THU NGAY: {total:N0} VNĐ";
    }

    private void SelectBooking(BookingDto b)
    {
        _selectedBooking = b;
        lblDetailCustomer.Text = $"Khách: {b.CustomerName} | SĐT: {b.CustomerPhone}";
        lblDetailCourt.Text = $"Sân: {b.CourtName} ({string.Join(", ", b.SlotLabels)})";

        var remainingAmount = b.TotalAmount - b.DepositAmount;
        lblDetailFinancial.Text = $"Sân: {b.TotalAmount:N0} đ | Cọc: {b.DepositAmount:N0} đ\nCẦN THU NỐT: {remainingAmount:N0} đ";

        // Tải danh sách phụ phí từ invoice
        _ = LoadServicesForBookingAsync(b.Id);
    }

    private async Task LoadServicesForBookingAsync(int bookingId)
    {
        dgvServices.Rows.Clear();
        try
        {
            var res = await WinFormsApiClient.Instance.GetInvoiceAsync(bookingId);
            if (res.Success && res.Data != null)
            {
                decimal serviceTotal = 0;
                foreach (var item in res.Data.Services)
                {
                    dgvServices.Rows.Add(item.ServiceName, item.Quantity, $"{item.UnitPrice:N0} đ", $"{item.SubTotal:N0} đ");
                    serviceTotal += item.SubTotal;
                }

                var remainingCourt = res.Data.CourtRentAmount - res.Data.DepositPaid;
                var totalDue = res.Data.RemainingAmountToPay;
                lblDetailFinancial.Text = $"Sân còn: {remainingCourt:N0} đ | Dịch vụ: {serviceTotal:N0} đ\nTỔNG CẦN THU: {totalDue:N0} đ";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoadServices] {ex.Message}");
        }
    }

    private void ClearBookingDetail()
    {
        _selectedBooking = null;
        lblDetailCustomer.Text = "Khách hàng: (Chưa chọn đơn)";
        lblDetailCourt.Text = "Sân thi đấu: - | Ca: -";
        lblDetailFinancial.Text = "Tiền sân: 0 đ | Đã cọc: 0 đ\nCần thu còn lại: 0 đ";
        dgvServices.Rows.Clear();
    }

    private async Task HandleCheckInAsync()
    {
        var code = txtCheckInCode.Text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            MessageBox.Show("Vui lòng nhập hoặc quét mã Check-in!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtCheckInCode.Focus();
            return;
        }

        btnCheckIn.Enabled = false;
        lblCheckInStatus.Text = "⏳ Đang kiểm tra mã check-in...";
        lblCheckInStatus.ForeColor = Color.FromArgb(71, 85, 105);

        try
        {
            var res = await WinFormsApiClient.Instance.CheckInQrAsync(new CheckInRequest
            {
                CheckInCode = code
            });

            if (res.Success)
            {
                lblCheckInStatus.Text = "✅ CHECK-IN THÀNH CÔNG! Khách đã vào sân.";
                lblCheckInStatus.ForeColor = Color.FromArgb(6, 95, 70);
                ToastNotifier.Show(this, "Check-in thành công! Khách đã vào sân.", ToastType.Success);
                txtCheckInCode.Clear();
                await LoadActiveBookingsAsync();
            }
            else
            {
                lblCheckInStatus.Text = $"❌ LỖI: {res.Message}";
                lblCheckInStatus.ForeColor = AppTheme.Danger;
                MessageBox.Show(res.Message ?? "Check-in không thành công.", "Lỗi Check-in", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            lblCheckInStatus.Text = $"❌ LỖI KẾT NỐI: {ex.Message}";
            lblCheckInStatus.ForeColor = AppTheme.Danger;
        }
        finally
        {
            btnCheckIn.Enabled = true;
        }
    }

    private async Task HandleAddServiceAsync()
    {
        if (_selectedBooking == null)
        {
            MessageBox.Show("Vui lòng chọn một đơn đặt sân bên danh sách để gọi dịch vụ!", "Chưa Chọn Đơn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (cboServiceCatalog.SelectedValue is not int serviceId)
        {
            MessageBox.Show("Vui lòng chọn mặt hàng dịch vụ/nước uống.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int quantity = (int)nudQuantity.Value;
        btnAddService.Enabled = false;

        try
        {
            var res = await WinFormsApiClient.Instance.AddServiceItemAsync(new AddServiceItemRequest
            {
                BookingId = _selectedBooking.Id,
                ServiceItemId = serviceId,
                Quantity = quantity
            });

            if (res.Success)
            {
                ToastNotifier.Show(this, "Đã thêm dịch vụ vào hóa đơn thành công!", ToastType.Success);
                await LoadServicesForBookingAsync(_selectedBooking.Id);
            }
            else
            {
                MessageBox.Show(res.Message ?? "Không thể thêm dịch vụ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnAddService.Enabled = true;
        }
    }

    private async Task HandleCheckoutAsync()
    {
        if (_selectedBooking == null)
        {
            MessageBox.Show("Vui lòng chọn đơn cần tất toán!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn tất toán tiền và kết thúc phiên chơi cho đơn {_selectedBooking.BookingCode} ({_selectedBooking.CustomerName})?",
            "Xác Nhận Tất Toán", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        btnCheckout.Enabled = false;
        try
        {
            var res = await WinFormsApiClient.Instance.CompletePaymentAsync(new CompletePaymentRequest
            {
                BookingId = _selectedBooking.Id,
                PaymentMethod = PaymentMethod.Cash
            });

            if (res.Success)
            {
                ToastNotifier.Show(this, "Tất toán phiên chơi thành công! Sân đã hoàn tất.", ToastType.Success);
                HandlePrintInvoice();
                await LoadActiveBookingsAsync();
            }
            else
            {
                MessageBox.Show(res.Message ?? "Không thể tất toán.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnCheckout.Enabled = true;
        }
    }

    private async void HandlePrintInvoice()
    {
        if (_selectedBooking == null) return;

        try
        {
            var res = await WinFormsApiClient.Instance.GetInvoiceAsync(_selectedBooking.Id);
            if (res.Success && res.Data != null)
            {
                PrintInvoiceK80(res.Data);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi sinh hóa đơn: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PrintInvoiceK80(PosInvoiceDto invoice)
    {
        var pd = new PrintDocument();
        pd.DefaultPageSettings.PaperSize = new PaperSize("K80", 315, 800); // 80mm width (~315px)
        pd.PrintPage += (s, e) =>
        {
            var g = e.Graphics!;
            var fontTitle = new Font("Segoe UI", 12, FontStyle.Bold);
            var fontBold = new Font("Segoe UI", 9, FontStyle.Bold);
            var fontRegular = new Font("Segoe UI", 8.5f);
            var fontSmall = new Font("Segoe UI", 7.5f);
            var brush = Brushes.Black;

            float y = 15;
            float left = 15;
            float right = 295;

            // Header
            g.DrawString("⚡ SPORTCHAIN VIETNAM", fontTitle, brush, new RectangleF(0, y, 315, 25), new StringFormat { Alignment = StringAlignment.Center });
            y += 24;
            g.DrawString("HỆ THỐNG SÂN THỂ THAO ĐA NĂNG", fontSmall, brush, new RectangleF(0, y, 315, 18), new StringFormat { Alignment = StringAlignment.Center });
            y += 18;
            g.DrawString("----------------------------------------------------------------", fontRegular, brush, left, y);
            y += 16;

            // Invoice Info
            g.DrawString("PHIẾU THANH TOÁN (RECEIPT)", fontBold, brush, new RectangleF(0, y, 315, 20), new StringFormat { Alignment = StringAlignment.Center });
            y += 22;
            g.DrawString($"Số HĐ: {invoice.BookingCode}", fontRegular, brush, left, y);
            y += 18;
            g.DrawString($"Ngày: {DateTime.Now:dd/MM/yyyy HH:mm}", fontRegular, brush, left, y);
            y += 18;
            g.DrawString($"Khách hàng: {invoice.CustomerName}", fontRegular, brush, left, y);
            y += 18;
            g.DrawString($"Sân: {invoice.CourtName} | Ngày: {invoice.BookingDate:dd/MM/yyyy}", fontRegular, brush, left, y);
            y += 18;
            g.DrawString($"Ca chơi: {string.Join(", ", invoice.SlotLabels)}", fontRegular, brush, left, y);
            y += 18;
            g.DrawString("----------------------------------------------------------------", fontRegular, brush, left, y);
            y += 16;

            // Details
            g.DrawString("Khoản mục", fontBold, brush, left, y);
            g.DrawString("Thành tiền", fontBold, brush, right - 70, y);
            y += 18;

            g.DrawString("Tiền thuê sân:", fontRegular, brush, left, y);
            g.DrawString($"{invoice.CourtRentAmount:N0} đ", fontRegular, brush, right - 70, y);
            y += 18;

            if (invoice.Services.Count > 0)
            {
                g.DrawString("[Dịch vụ gọi thêm]:", fontBold, brush, left, y);
                y += 16;
                foreach (var item in invoice.Services)
                {
                    g.DrawString($" - {item.ServiceName} x{item.Quantity}", fontRegular, brush, left + 10, y);
                    g.DrawString($"{item.SubTotal:N0} đ", fontRegular, brush, right - 70, y);
                    y += 16;
                }
            }

            g.DrawString("----------------------------------------------------------------", fontRegular, brush, left, y);
            y += 16;

            // Summary
            var totalInvoice = invoice.CourtRentAmount + invoice.TotalServicesAmount;
            g.DrawString("Tổng cộng:", fontRegular, brush, left, y);
            g.DrawString($"{totalInvoice:N0} đ", fontRegular, brush, right - 70, y);
            y += 18;

            g.DrawString("Đã đặt cọc:", fontRegular, brush, left, y);
            g.DrawString($"-{invoice.DepositPaid:N0} đ", fontRegular, brush, right - 70, y);
            y += 18;

            g.DrawString("THANH TOÁN TẠI QUẦY:", fontBold, brush, left, y);
            g.DrawString($"{invoice.RemainingAmountToPay:N0} đ", fontBold, brush, right - 70, y);
            y += 24;

            // Footer
            g.DrawString("Phương thức: TIỀN MẶT / VIETQR", fontSmall, brush, new RectangleF(0, y, 315, 16), new StringFormat { Alignment = StringAlignment.Center });
            y += 16;
            g.DrawString("Cảm ơn quý khách và hẹn gặp lại!", fontSmall, brush, new RectangleF(0, y, 315, 16), new StringFormat { Alignment = StringAlignment.Center });
            y += 16;
            g.DrawString("Hotline hỗ trợ: 1900 6868", fontSmall, brush, new RectangleF(0, y, 315, 16), new StringFormat { Alignment = StringAlignment.Center });
        };

        var preview = new PrintPreviewDialog
        {
            Document = pd,
            Width = 450,
            Height = 650,
            StartPosition = FormStartPosition.CenterParent
        };
        preview.ShowDialog(this.FindForm());
    }

    private async Task HandleCreateWalkInAsync()
    {
        if (cboWalkInCourt.SelectedValue is not int courtId)
        {
            MessageBox.Show("Vui lòng chọn sân thi đấu.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var selectedSlots = chkWalkInSlots.CheckedItems.OfType<WalkInSlotItem>().Select(s => s.TimeSlotId).ToList();
        if (selectedSlots.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn ít nhất một ca giờ thi đấu.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var phone = txtWalkInPhone.Text.Trim();
        if (string.IsNullOrEmpty(phone))
        {
            MessageBox.Show("Vui lòng nhập số điện thoại của khách vãng lai!", "Thiếu Thông Tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtWalkInPhone.Focus();
            return;
        }

        var name = string.IsNullOrWhiteSpace(txtWalkInName.Text) ? "Khách Vãng Lai" : txtWalkInName.Text.Trim();
        var payment = (PaymentMethod)(cboWalkInPayment.SelectedItem ?? PaymentMethod.Cash);

        btnCreateWalkIn.Enabled = false;
        try
        {
            var res = await WinFormsApiClient.Instance.CreateWalkInBookingAsync(new WalkInBookingRequest
            {
                BranchId = _branchId,
                CourtId = courtId,
                BookingDate = DateOnly.FromDateTime(DateTime.Today),
                TimeSlotIds = selectedSlots,
                CustomerPhone = phone,
                CustomerName = name,
                PaymentMethod = payment
            });

            if (res.Success && res.Data != null)
            {
                MessageBox.Show("Tạo đơn đặt sân vãng lai thành công! Khách đã vào sân thi đấu ngay.", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                PrintInvoiceK80(res.Data);

                txtWalkInPhone.Clear();
                txtWalkInName.Clear();
                await LoadWalkInSlotsForCourtAsync();
                await LoadActiveBookingsAsync();
                tabPos.SelectedTab = tabActiveBookings;
            }
            else
            {
                MessageBox.Show(res.Message ?? "Không thể tạo đơn vãng lai.", "Lỗi Đặt Sân", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tạo đơn: {ex.Message}", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnCreateWalkIn.Enabled = true;
        }
    }

    private class WalkInSlotItem
    {
        public int TimeSlotId { get; set; }
        public string Display { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public override string ToString() => Display;
    }
}
