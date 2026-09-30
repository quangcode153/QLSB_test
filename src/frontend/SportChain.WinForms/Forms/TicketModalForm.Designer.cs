namespace SportChain.WinForms.Forms;

partial class TicketModalForm
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlHeader;
    private Label lblHeaderTitle;
    private Label lblHeaderBadge;

    private Panel pnlBody;
    private PictureBox picTicketQr;
    private Label lblCheckInCode;
    private Label lblSubTitle;

    private Panel pnlDetails;
    private Label lblBookingCode;
    private Label lblBranchName;
    private Label lblCourtName;
    private Label lblBookingDate;
    private Label lblSlotLabels;
    private Label lblDepositAmount;
    private Label lblRemainingAmount;

    private Button btnCopyCode;
    private Button btnSaveQr;
    private Button btnClose;

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
        lblHeaderTitle = new Label();
        lblHeaderBadge = new Label();

        pnlBody = new Panel();
        picTicketQr = new PictureBox();
        lblCheckInCode = new Label();
        lblSubTitle = new Label();

        pnlDetails = new Panel();
        lblBookingCode = new Label();
        lblBranchName = new Label();
        lblCourtName = new Label();
        lblBookingDate = new Label();
        lblSlotLabels = new Label();
        lblDepositAmount = new Label();
        lblRemainingAmount = new Label();

        btnCopyCode = new Button();
        btnSaveQr = new Button();
        btnClose = new Button();

        // 
        // pnlHeader
        // 
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Height = 75;
        pnlHeader.BackColor = Color.FromArgb(15, 23, 42);
        pnlHeader.Padding = new Padding(20, 14, 20, 10);

        lblHeaderBadge.Text = "✔ ĐẶT CỌC THÀNH CÔNG";
        lblHeaderBadge.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        lblHeaderBadge.ForeColor = Color.White;
        lblHeaderBadge.BackColor = Color.FromArgb(16, 185, 129); // Emerald
        lblHeaderBadge.Padding = new Padding(8, 3, 8, 3);
        lblHeaderBadge.Location = new Point(20, 12);
        lblHeaderBadge.AutoSize = true;

        lblHeaderTitle.Text = "VÉ ĐIỆN TỬ & MÃ QR CHECK-IN";
        lblHeaderTitle.Font = new Font("Segoe UI", 12.5F, FontStyle.Bold);
        lblHeaderTitle.ForeColor = Color.White;
        lblHeaderTitle.Location = new Point(20, 38);
        lblHeaderTitle.AutoSize = true;

        pnlHeader.Controls.Add(lblHeaderBadge);
        pnlHeader.Controls.Add(lblHeaderTitle);

        // 
        // pnlBody
        // 
        pnlBody.Dock = DockStyle.Fill;
        pnlBody.BackColor = Color.White;
        pnlBody.Padding = new Padding(24, 16, 24, 16);

        picTicketQr.Location = new Point(140, 15);
        picTicketQr.Size = new Size(180, 180);
        picTicketQr.SizeMode = PictureBoxSizeMode.Zoom;
        picTicketQr.BorderStyle = BorderStyle.FixedSingle;
        picTicketQr.BackColor = Color.White;

        lblCheckInCode.Text = "CK-882194";
        lblCheckInCode.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblCheckInCode.ForeColor = Color.FromArgb(13, 148, 136); // Teal
        lblCheckInCode.Location = new Point(24, 202);
        lblCheckInCode.Size = new Size(412, 28);
        lblCheckInCode.TextAlign = ContentAlignment.MiddleCenter;

        lblSubTitle.Text = "Xuất trình mã này tại quầy Lễ tân trước giờ chơi 15 phút";
        lblSubTitle.Font = new Font("Segoe UI", 8.5F);
        lblSubTitle.ForeColor = Color.FromArgb(100, 116, 139);
        lblSubTitle.Location = new Point(24, 230);
        lblSubTitle.Size = new Size(412, 20);
        lblSubTitle.TextAlign = ContentAlignment.MiddleCenter;

        // pnlDetails
        pnlDetails.Location = new Point(24, 255);
        pnlDetails.Size = new Size(412, 195);
        pnlDetails.BackColor = Color.FromArgb(248, 250, 252);
        pnlDetails.BorderStyle = BorderStyle.FixedSingle;
        pnlDetails.Padding = new Padding(12);

        ConfigureDetailLabel(lblBookingCode, "Mã đơn đặt:", 12);
        ConfigureDetailLabel(lblBranchName, "Cơ sở:", 38);
        ConfigureDetailLabel(lblCourtName, "Sân thi đấu:", 64);
        ConfigureDetailLabel(lblBookingDate, "Ngày chơi:", 90);
        ConfigureDetailLabel(lblSlotLabels, "Khung ca:", 116);
        ConfigureDetailLabel(lblDepositAmount, "Đã đặt cọc:", 142);
        ConfigureDetailLabel(lblRemainingAmount, "Còn lại tại quầy:", 168);

        pnlDetails.Controls.Add(lblBookingCode);
        pnlDetails.Controls.Add(lblBranchName);
        pnlDetails.Controls.Add(lblCourtName);
        pnlDetails.Controls.Add(lblBookingDate);
        pnlDetails.Controls.Add(lblSlotLabels);
        pnlDetails.Controls.Add(lblDepositAmount);
        pnlDetails.Controls.Add(lblRemainingAmount);

        btnCopyCode.Text = "📋 Sao Chép Mã";
        btnCopyCode.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnCopyCode.BackColor = Color.FromArgb(241, 245, 249);
        btnCopyCode.ForeColor = Color.FromArgb(15, 23, 42);
        btnCopyCode.FlatStyle = FlatStyle.Flat;
        btnCopyCode.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        btnCopyCode.Location = new Point(24, 462);
        btnCopyCode.Size = new Size(200, 38);
        btnCopyCode.Cursor = Cursors.Hand;

        btnSaveQr.Text = "💾 Lưu Ảnh QR";
        btnSaveQr.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnSaveQr.BackColor = Color.FromArgb(16, 185, 129); // Emerald green
        btnSaveQr.ForeColor = Color.White;
        btnSaveQr.FlatStyle = FlatStyle.Flat;
        btnSaveQr.FlatAppearance.BorderSize = 0;
        btnSaveQr.Location = new Point(236, 462);
        btnSaveQr.Size = new Size(200, 38);
        btnSaveQr.Cursor = Cursors.Hand;

        btnClose.Text = "✔ Hoàn Tất & Đóng";
        btnClose.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnClose.BackColor = Color.FromArgb(15, 23, 42);
        btnClose.ForeColor = Color.White;
        btnClose.FlatStyle = FlatStyle.Flat;
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Location = new Point(24, 508);
        btnClose.Size = new Size(412, 40);
        btnClose.Cursor = Cursors.Hand;

        pnlBody.Controls.Add(picTicketQr);
        pnlBody.Controls.Add(lblCheckInCode);
        pnlBody.Controls.Add(lblSubTitle);
        pnlBody.Controls.Add(pnlDetails);
        pnlBody.Controls.Add(btnCopyCode);
        pnlBody.Controls.Add(btnSaveQr);
        pnlBody.Controls.Add(btnClose);

        // 
        // TicketModalForm
        // 
        this.AutoScaleDimensions = new SizeF(8F, 20F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(460, 645);
        this.Controls.Add(pnlBody);
        this.Controls.Add(pnlHeader);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "SportChain - Vé Điện Tử";
    }

    private void ConfigureDetailLabel(Label lbl, string defaultText, int y)
    {
        lbl.Text = defaultText;
        lbl.Font = new Font("Segoe UI", 9F);
        lbl.ForeColor = Color.FromArgb(51, 65, 85);
        lbl.Location = new Point(14, y);
        lbl.Size = new Size(380, 20);
    }
}
