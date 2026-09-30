using QRCoder;
using SportChain.Shared.DTOs.Bookings;

namespace SportChain.WinForms.Forms;

public partial class TicketModalForm : Form
{
    private readonly BookingDto _booking;

    public TicketModalForm(BookingDto booking)
    {
        InitializeComponent();
        _booking = booking;
        LoadTicketDetails();
        RenderQrCode();
        btnClose.Click += (s, e) => this.Close();

        btnCopyCode.Click += (s, e) =>
        {
            var code = string.IsNullOrEmpty(_booking.CheckInCode) ? _booking.BookingCode : _booking.CheckInCode;
            Clipboard.SetText(code);
            MessageBox.Show($"Đã sao chép mã check-in: {code} vào bộ nhớ tạm (Clipboard)!",
                "Đã Sao Chép", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        btnSaveQr.Click += (s, e) =>
        {
            if (picTicketQr.Image == null)
            {
                MessageBox.Show("Chưa có ảnh mã QR để lưu!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg",
                FileName = $"SportChain_Ticket_{_booking.BookingCode}_{_booking.CheckInCode}.png",
                Title = "Lưu Mã QR Vé Điện Tử Làm Bằng Chứng Check-in"
            };

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    picTicketQr.Image.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    MessageBox.Show($"Đã lưu mã QR thành công vào:\n{sfd.FileName}",
                        "Lưu Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể lưu ảnh: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        };
    }

    private void LoadTicketDetails()
    {
        lblCheckInCode.Text = $"MÃ CHECK-IN: {_booking.CheckInCode}";
        lblBookingCode.Text = $"Mã đơn đặt:  {_booking.BookingCode}";
        lblBranchName.Text = $"Cơ sở:  {_booking.BranchName}";
        lblCourtName.Text = $"Sân thi đấu:  {_booking.CourtName}";
        lblBookingDate.Text = $"Ngày chơi:  {_booking.BookingDate:dd/MM/yyyy}";
        lblSlotLabels.Text = $"Khung ca:  {string.Join(", ", _booking.SlotLabels)}";
        lblDepositAmount.Text = $"Đã đặt cọc:  {_booking.DepositAmount:N0} đ (30%)";
        lblRemainingAmount.Text = $"Còn lại tại quầy:  {_booking.RemainingAmount:N0} đ";
    }

    private void RenderQrCode()
    {
        try
        {
            if (!string.IsNullOrEmpty(_booking.QrCodeBase64))
            {
                var rawBase64 = _booking.QrCodeBase64.Contains(",")
                    ? _booking.QrCodeBase64.Substring(_booking.QrCodeBase64.IndexOf(",") + 1)
                    : _booking.QrCodeBase64;

                var bytes = Convert.FromBase64String(rawBase64);
                using var ms = new MemoryStream(bytes);
                picTicketQr.Image = new Bitmap(ms);
                return;
            }

            // Sinh mã QR check-in qua QRCoder từ CheckInCode
            var qrText = string.IsNullOrEmpty(_booking.CheckInCode) ? _booking.BookingCode : _booking.CheckInCode;
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrBytes = qrCode.GetGraphic(20);

            using var memStream = new MemoryStream(qrBytes);
            picTicketQr.Image = new Bitmap(memStream);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RenderTicketQr] {ex.Message}");
        }
    }
}
