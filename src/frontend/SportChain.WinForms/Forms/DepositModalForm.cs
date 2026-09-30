using QRCoder;
using SportChain.Shared.DTOs.Bookings;
using SportChain.Shared.Enums;
using SportChain.WinForms.Services;

namespace SportChain.WinForms.Forms;

public partial class DepositModalForm : Form
{
    private readonly BookingDto _booking;
    private System.Windows.Forms.Timer? _timer;
    private bool _isBusy = false;

    public BookingDto? ConfirmedBooking { get; private set; }

    public DepositModalForm(BookingDto booking)
    {
        InitializeComponent();
        _booking = booking;
        LoadBookingInfo();
        GenerateQrCode();
        StartCountdown();
        SetupEvents();
    }

    private void LoadBookingInfo()
    {
        lblDepositAmount.Text = $"Số tiền cọc: {_booking.DepositAmount:N0} đ (30%)";
        lblTransferContent.Text = $"Nội dung CK: {_booking.BookingCode}";
    }

    private async void GenerateQrCode()
    {
        try
        {
            // 1. Nếu có ảnh Base64 sẵn từ backend
            if (!string.IsNullOrEmpty(_booking.QrCodeBase64))
            {
                var rawBase64 = _booking.QrCodeBase64.Contains(",")
                    ? _booking.QrCodeBase64.Substring(_booking.QrCodeBase64.IndexOf(",") + 1)
                    : _booking.QrCodeBase64;

                var bytes = Convert.FromBase64String(rawBase64);
                using var ms = new MemoryStream(bytes);
                picQrCode.Image = new Bitmap(ms);
                return;
            }

            // 2. Tải trực tiếp ảnh chuẩn VietQR NAPAS từ gateway
            var vietQrUrl = $"https://img.vietqr.io/image/mbbank-999988887777-compact2.png?amount={(long)_booking.DepositAmount}&addInfo={Uri.EscapeDataString(_booking.BookingCode)}&accountName=SPORTCHAIN%20VIETNAM";
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var imageBytes = await http.GetByteArrayAsync(vietQrUrl);
            using var stream = new MemoryStream(imageBytes);
            picQrCode.Image = new Bitmap(stream);
        }
        catch
        {
            // 3. Fallback dự phòng bằng QRCoder
            try
            {
                var fallbackPayload = $"SPORTCHAIN|MBBANK|999988887777|{(long)_booking.DepositAmount}|{_booking.BookingCode}";
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(fallbackPayload, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);
                var qrBytes = qrCode.GetGraphic(20);

                using var memStream = new MemoryStream(qrBytes);
                picQrCode.Image = new Bitmap(memStream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GenerateQrCode Fallback] {ex.Message}");
            }
        }
    }

    private void StartCountdown()
    {
        // Chuẩn hóa múi giờ UTC tuyệt đối tránh lệch 7 tiếng
        var expireUtc = _booking.ExpireHoldingAt.Kind == DateTimeKind.Utc
            ? _booking.ExpireHoldingAt
            : DateTime.SpecifyKind(_booking.ExpireHoldingAt, DateTimeKind.Utc);

        _timer = new System.Windows.Forms.Timer { Interval = 1000 };
        _timer.Tick += (s, e) =>
        {
            var remaining = expireUtc - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                _timer.Stop();
                lblCountdownBadge.Text = "⚠️ ĐÃ HẾT THỜI GIAN GIỮ CHỖ (15 PHÚT)";
                lblCountdownBadge.BackColor = Color.FromArgb(254, 226, 226);
                lblCountdownBadge.ForeColor = Color.FromArgb(220, 38, 38);
                btnConfirmPayment.Enabled = false;

                MessageBox.Show("Thời gian giữ chỗ 15 phút đã kết thúc. Các ca sân đã được tự động mở lại cho người khác.",
                    "Hết Thời Gian", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
            else
            {
                lblCountdownBadge.Text = $"⏳ Thời gian giữ chỗ còn lại: {remaining.Minutes:D2}:{remaining.Seconds:D2}";
            }
        };
        _timer.Start();
    }

    private void SetupEvents()
    {
        btnCloseLater.Click += (s, e) =>
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        };

        btnCancelHolding.Click += async (s, e) =>
        {
            if (_isBusy) return;

            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn hủy giữ chỗ cho đơn {_booking.BookingCode}?",
                "Xác Nhận Hủy", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            SetBusy(true);
            try
            {
                var res = await WinFormsApiClient.Instance.CancelBookingAsync(_booking.Id, new CancelBookingRequest
                {
                    BookingId = _booking.Id,
                    Reason = "Khách hủy cọc từ màn hình thanh toán"
                });

                if (res.Success)
                {
                    MessageBox.Show("Đã hủy giữ chỗ thành công!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.Abort;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(res.Message ?? "Không thể hủy đơn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        };

        btnConfirmPayment.Click += async (s, e) =>
        {
            if (_isBusy) return;

            var method = PaymentMethod.VietQr;
            if (rdoCreditCard.Checked) method = PaymentMethod.CreditCard;
            else if (rdoCash.Checked) method = PaymentMethod.Cash;

            SetBusy(true);
            try
            {
                var res = await WinFormsApiClient.Instance.ConfirmDepositAsync(_booking.Id, new ConfirmDepositRequest
                {
                    PaymentMethod = method
                });

                if (res.Success && res.Data != null)
                {
                    ConfirmedBooking = res.Data;
                    _timer?.Stop();

                    MessageBox.Show("Thanh toán tiền cọc thành công!\nHệ thống đã tạo vé điện tử kèm mã QR check-in cho bạn.",
                        "Đặt Sân Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(res.Message ?? "Xác nhận cọc thất bại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thanh toán: {ex.Message}", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        };
    }

    private void SetBusy(bool busy)
    {
        _isBusy = busy;
        this.Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        btnConfirmPayment.Enabled = !busy;
        btnCancelHolding.Enabled = !busy;
        btnCloseLater.Enabled = !busy;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
        base.OnFormClosed(e);
    }
}
