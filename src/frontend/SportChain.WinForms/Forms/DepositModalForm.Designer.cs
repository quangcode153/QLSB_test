namespace SportChain.WinForms.Forms;

partial class DepositModalForm
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlHeader;
    private Label lblTitle;
    private Label lblSubTitle;

    private Panel pnlBody;
    private Label lblBankName;
    private Label lblAccountNo;
    private Label lblAccountName;
    private PictureBox picQrCode;
    private Label lblDepositAmount;
    private Label lblTransferContent;

    private Label lblPaymentMethodTitle;
    private RadioButton rdoVietQr;
    private RadioButton rdoCreditCard;
    private RadioButton rdoCash;

    private Label lblCountdownBadge;

    private Panel pnlFooter;
    private Button btnCancelHolding;
    private Button btnCloseLater;
    private Button btnConfirmPayment;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlHeader = new Panel();
        lblTitle = new Label();
        lblSubTitle = new Label();

        pnlBody = new Panel();
        lblBankName = new Label();
        lblAccountNo = new Label();
        lblAccountName = new Label();
        picQrCode = new PictureBox();
        lblDepositAmount = new Label();
        lblTransferContent = new Label();

        lblPaymentMethodTitle = new Label();
        rdoVietQr = new RadioButton();
        rdoCreditCard = new RadioButton();
        rdoCash = new RadioButton();

        lblCountdownBadge = new Label();

        pnlFooter = new Panel();
        btnCancelHolding = new Button();
        btnCloseLater = new Button();
        btnConfirmPayment = new Button();

        // 
        // pnlHeader
        // 
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Height = 70;
        pnlHeader.BackColor = Color.FromArgb(15, 23, 42);
        pnlHeader.Padding = new Padding(20, 12, 20, 12);

        lblTitle.Text = "💳 THANH TOÁN TIỀN CỌC GIỮ CHỖ (30%)";
        lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(20, 12);
        lblTitle.AutoSize = true;

        lblSubTitle.Text = "Quét mã VietQR hoặc chuyển khoản trong vòng 15 phút để hoàn tất đặt sân";
        lblSubTitle.Font = new Font("Segoe UI", 8.5F);
        lblSubTitle.ForeColor = Color.FromArgb(148, 163, 184);
        lblSubTitle.Location = new Point(22, 38);
        lblSubTitle.AutoSize = true;

        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(lblSubTitle);

        // 
        // pnlBody
        // 
        pnlBody.Dock = DockStyle.Fill;
        pnlBody.BackColor = Color.White;
        pnlBody.Padding = new Padding(24);

        lblBankName.Text = "NGÂN HÀNG QUÂN ĐỘI (MBBANK)";
        lblBankName.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblBankName.ForeColor = Color.FromArgb(37, 99, 235);
        lblBankName.Location = new Point(24, 15);
        lblBankName.Size = new Size(470, 24);
        lblBankName.TextAlign = ContentAlignment.MiddleCenter;

        lblAccountNo.Text = "Số tài khoản: 999988887777";
        lblAccountNo.Font = new Font("Segoe UI", 9.5F);
        lblAccountNo.ForeColor = Color.FromArgb(71, 85, 105);
        lblAccountNo.Location = new Point(24, 42);
        lblAccountNo.Size = new Size(470, 20);
        lblAccountNo.TextAlign = ContentAlignment.MiddleCenter;

        lblAccountName.Text = "Chủ TK: SPORTCHAIN VIETNAM";
        lblAccountName.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblAccountName.ForeColor = Color.FromArgb(15, 23, 42);
        lblAccountName.Location = new Point(24, 64);
        lblAccountName.Size = new Size(470, 20);
        lblAccountName.TextAlign = ContentAlignment.MiddleCenter;

        // QR Code Box
        picQrCode.Location = new Point(160, 92);
        picQrCode.Size = new Size(200, 200);
        picQrCode.SizeMode = PictureBoxSizeMode.Zoom;
        picQrCode.BorderStyle = BorderStyle.FixedSingle;
        picQrCode.BackColor = Color.White;

        lblDepositAmount.Text = "Số tiền cọc: 72.000 đ";
        lblDepositAmount.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblDepositAmount.ForeColor = Color.FromArgb(220, 38, 38); // Crimson
        lblDepositAmount.Location = new Point(24, 300);
        lblDepositAmount.Size = new Size(470, 28);
        lblDepositAmount.TextAlign = ContentAlignment.MiddleCenter;

        lblTransferContent.Text = "Nội dung CK: BK-123456";
        lblTransferContent.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblTransferContent.ForeColor = Color.FromArgb(13, 148, 136); // Teal
        lblTransferContent.Location = new Point(24, 332);
        lblTransferContent.Size = new Size(470, 22);
        lblTransferContent.TextAlign = ContentAlignment.MiddleCenter;

        // Payment method selector
        lblPaymentMethodTitle.Text = "Phương thức xác nhận:";
        lblPaymentMethodTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblPaymentMethodTitle.ForeColor = Color.FromArgb(71, 85, 105);
        lblPaymentMethodTitle.Location = new Point(40, 368);
        lblPaymentMethodTitle.AutoSize = true;

        rdoVietQr.Text = "📱 VietQR / Chuyển Khoản";
        rdoVietQr.Checked = true;
        rdoVietQr.Location = new Point(40, 392);
        rdoVietQr.AutoSize = true;

        rdoCreditCard.Text = "💳 Thẻ Tín Dụng";
        rdoCreditCard.Location = new Point(220, 392);
        rdoCreditCard.AutoSize = true;

        rdoCash.Text = "💵 Tiền Mặt (Tại Quầy)";
        rdoCash.Location = new Point(350, 392);
        rdoCash.AutoSize = true;

        // Countdown timer badge
        lblCountdownBadge.Text = "⏳ Thời gian giữ chỗ còn lại: 15:00";
        lblCountdownBadge.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblCountdownBadge.ForeColor = Color.FromArgb(180, 83, 9);
        lblCountdownBadge.BackColor = Color.FromArgb(254, 243, 199);
        lblCountdownBadge.Padding = new Padding(8, 4, 8, 4);
        lblCountdownBadge.Location = new Point(125, 428);
        lblCountdownBadge.AutoSize = true;

        pnlBody.Controls.Add(lblBankName);
        pnlBody.Controls.Add(lblAccountNo);
        pnlBody.Controls.Add(lblAccountName);
        pnlBody.Controls.Add(picQrCode);
        pnlBody.Controls.Add(lblDepositAmount);
        pnlBody.Controls.Add(lblTransferContent);
        pnlBody.Controls.Add(lblPaymentMethodTitle);
        pnlBody.Controls.Add(rdoVietQr);
        pnlBody.Controls.Add(rdoCreditCard);
        pnlBody.Controls.Add(rdoCash);
        pnlBody.Controls.Add(lblCountdownBadge);

        // 
        // pnlFooter
        // 
        pnlFooter.Dock = DockStyle.Bottom;
        pnlFooter.Height = 65;
        pnlFooter.BackColor = Color.FromArgb(248, 250, 252);
        pnlFooter.Padding = new Padding(20, 12, 20, 12);
        pnlFooter.BorderStyle = BorderStyle.FixedSingle;

        btnCancelHolding.Text = "❌ Hủy Giữ Chỗ";
        btnCancelHolding.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnCancelHolding.BackColor = Color.White;
        btnCancelHolding.ForeColor = Color.FromArgb(220, 38, 38);
        btnCancelHolding.FlatStyle = FlatStyle.Flat;
        btnCancelHolding.FlatAppearance.BorderColor = Color.FromArgb(252, 165, 165);
        btnCancelHolding.Location = new Point(20, 14);
        btnCancelHolding.Size = new Size(125, 36);
        btnCancelHolding.Cursor = Cursors.Hand;

        btnCloseLater.Text = "Để Sau";
        btnCloseLater.Font = new Font("Segoe UI", 9F);
        btnCloseLater.BackColor = Color.FromArgb(241, 245, 249);
        btnCloseLater.ForeColor = Color.FromArgb(71, 85, 105);
        btnCloseLater.FlatStyle = FlatStyle.Flat;
        btnCloseLater.FlatAppearance.BorderSize = 0;
        btnCloseLater.Location = new Point(220, 14);
        btnCloseLater.Size = new Size(80, 36);
        btnCloseLater.Cursor = Cursors.Hand;

        btnConfirmPayment.Text = "✔ Xác Nhận Đã Chuyển Khoản";
        btnConfirmPayment.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnConfirmPayment.BackColor = Color.FromArgb(16, 185, 129); // Emerald
        btnConfirmPayment.ForeColor = Color.White;
        btnConfirmPayment.FlatStyle = FlatStyle.Flat;
        btnConfirmPayment.FlatAppearance.BorderSize = 0;
        btnConfirmPayment.Location = new Point(310, 14);
        btnConfirmPayment.Size = new Size(185, 36);
        btnConfirmPayment.Cursor = Cursors.Hand;

        pnlFooter.Controls.Add(btnCancelHolding);
        pnlFooter.Controls.Add(btnCloseLater);
        pnlFooter.Controls.Add(btnConfirmPayment);

        // 
        // DepositModalForm
        // 
        this.AutoScaleDimensions = new SizeF(8F, 20F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(520, 610);
        this.Controls.Add(pnlBody);
        this.Controls.Add(pnlHeader);
        this.Controls.Add(pnlFooter);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "SportChain - Thanh Toán Cọc Giữ Chỗ";
    }
}
