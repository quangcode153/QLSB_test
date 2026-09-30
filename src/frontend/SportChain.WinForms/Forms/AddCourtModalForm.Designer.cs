using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using SportChain.WinForms.UI.Controls;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.Forms;

partial class AddCourtModalForm
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlHeader;
    private IconPictureBox picIcon;
    private Label lblTitle;
    private Label lblSubtitle;

    private Panel pnlBody;

    private Label lblBranch;
    private ComboBox cboBranch;
    private Label lblBranchHint;
    private Label lblBranchAddress;

    private Label lblCourtName;
    private AppTextBox txtCourtName;

    private Label lblCourtLocation;
    private AppTextBox txtCourtLocation;
    private Label lblLocationTip;

    private Label lblSportType;
    private ComboBox cboSportType;

    private Label lblSurfaceType;
    private ComboBox cboSurfaceType;

    private Label lblDefaultPrice;
    private AppTextBox txtDefaultPrice;

    private Panel pnlFooter;
    private AppButton btnCancel;
    private AppButton btnSave;

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
        picIcon = new IconPictureBox();
        lblTitle = new Label();
        lblSubtitle = new Label();

        pnlBody = new Panel();

        lblBranch = new Label();
        cboBranch = new ComboBox();
        lblBranchHint = new Label();
        lblBranchAddress = new Label();

        lblCourtName = new Label();
        txtCourtName = new AppTextBox();

        lblCourtLocation = new Label();
        txtCourtLocation = new AppTextBox();
        lblLocationTip = new Label();

        lblSportType = new Label();
        cboSportType = new ComboBox();

        lblSurfaceType = new Label();
        cboSurfaceType = new ComboBox();

        lblDefaultPrice = new Label();
        txtDefaultPrice = new AppTextBox();

        pnlFooter = new Panel();
        btnCancel = new AppButton();
        btnSave = new AppButton();

        SuspendLayout();

        // 
        // AddCourtModalForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(560, 680);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Thêm Sân Thể Thao Mới";
        BackColor = AppTheme.MainBg;
        Font = AppTheme.FontBody;

        // 
        // pnlHeader
        // 
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Height = 74;
        pnlHeader.BackColor = AppTheme.CardBg;
        pnlHeader.Padding = new Padding(24, 14, 24, 12);
        pnlHeader.Paint += (s, e) =>
        {
            using var pen = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
        };

        picIcon.IconChar = IconChar.PlusCircle;
        picIcon.IconColor = AppTheme.Primary;
        picIcon.IconSize = 28;
        picIcon.Size = new Size(28, 28);
        picIcon.Location = new Point(24, 18);
        picIcon.BackColor = Color.Transparent;

        lblTitle.Text = "THÊM SÂN MỚI VÀO CƠ SỞ";
        lblTitle.Font = AppTheme.FontCardTitle;
        lblTitle.ForeColor = AppTheme.TextMain;
        lblTitle.Location = new Point(60, 14);
        lblTitle.AutoSize = true;

        lblSubtitle.Text = "Khai báo sân thi đấu mới và định vị địa chỉ cụ thể cho khách hàng";
        lblSubtitle.Font = AppTheme.FontSmall;
        lblSubtitle.ForeColor = AppTheme.TextMuted;
        lblSubtitle.Location = new Point(61, 40);
        lblSubtitle.AutoSize = true;

        pnlHeader.Controls.Add(picIcon);
        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(lblSubtitle);

        // 
        // pnlBody
        // 
        pnlBody.Dock = DockStyle.Fill;
        pnlBody.Padding = new Padding(28, 14, 28, 14);
        pnlBody.BackColor = AppTheme.MainBg;
        pnlBody.AutoScroll = true;

        // 1. Chi Nhánh Quản Lý
        lblBranch.Text = "Chi Nhánh Quản Lý:";
        lblBranch.Font = AppTheme.FontCaptionBold;
        lblBranch.ForeColor = AppTheme.TextMain;
        lblBranch.Location = new Point(28, 12);
        lblBranch.AutoSize = true;

        cboBranch.DropDownStyle = ComboBoxStyle.DropDownList;
        cboBranch.Font = AppTheme.FontBody;
        cboBranch.Location = new Point(28, 34);
        cboBranch.Size = new Size(480, 32);

        lblBranchHint.Text = "Đang tải danh sách cơ sở...";
        lblBranchHint.Font = AppTheme.FontSmall;
        lblBranchHint.ForeColor = AppTheme.TextSubtle;
        lblBranchHint.Location = new Point(30, 70);
        lblBranchHint.AutoSize = true;

        lblBranchAddress.Text = "📍 Địa chỉ cơ sở: ...";
        lblBranchAddress.Font = AppTheme.FontCaptionBold;
        lblBranchAddress.ForeColor = AppTheme.Primary;
        lblBranchAddress.Location = new Point(30, 92);
        lblBranchAddress.AutoSize = true;

        // 2. Tên Sân
        lblCourtName.Text = "Tên Sân Thể Thao:";
        lblCourtName.Font = AppTheme.FontCaptionBold;
        lblCourtName.ForeColor = AppTheme.TextMain;
        lblCourtName.Location = new Point(28, 126);
        lblCourtName.AutoSize = true;

        txtCourtName.PlaceholderText = "Ví dụ: Sân Pickleball 01, Sân Cầu Lông VIP 02...";
        txtCourtName.LeftIcon = IconChar.Volleyball;
        txtCourtName.Location = new Point(28, 148);
        txtCourtName.Size = new Size(480, 44);

        // 3. Vị trí / Địa chỉ cụ thể
        lblCourtLocation.Text = "Địa Chỉ / Vị Trí Cụ Thể Trong Cơ Sở:";
        lblCourtLocation.Font = AppTheme.FontCaptionBold;
        lblCourtLocation.ForeColor = AppTheme.TextMain;
        lblCourtLocation.Location = new Point(28, 200);
        lblCourtLocation.AutoSize = true;

        txtCourtLocation.PlaceholderText = "Ví dụ: Tầng 2 - Tòa A (Nhà đa năng) hoặc Cổng 2 Cụm ngoài trời";
        txtCourtLocation.LeftIcon = IconChar.MapMarkerAlt;
        txtCourtLocation.Location = new Point(28, 222);
        txtCourtLocation.Size = new Size(480, 44);

        lblLocationTip.Text = "💡 Giúp khách hàng nắm rõ vị trí và tiếp cận sân nhanh chóng khi đến nơi.";
        lblLocationTip.Font = AppTheme.FontSmall;
        lblLocationTip.ForeColor = AppTheme.TextSubtle;
        lblLocationTip.Location = new Point(30, 270);
        lblLocationTip.AutoSize = true;

        // 4. Bộ môn & Mặt sân
        lblSportType.Text = "Bộ Môn Thể Thao:";
        lblSportType.Font = AppTheme.FontCaptionBold;
        lblSportType.ForeColor = AppTheme.TextMain;
        lblSportType.Location = new Point(28, 298);
        lblSportType.AutoSize = true;

        cboSportType.DropDownStyle = ComboBoxStyle.DropDownList;
        cboSportType.Font = AppTheme.FontBody;
        cboSportType.Location = new Point(28, 320);
        cboSportType.Size = new Size(230, 32);

        lblSurfaceType.Text = "Loại Mặt Sân:";
        lblSurfaceType.Font = AppTheme.FontCaptionBold;
        lblSurfaceType.ForeColor = AppTheme.TextMain;
        lblSurfaceType.Location = new Point(278, 298);
        lblSurfaceType.AutoSize = true;

        cboSurfaceType.Font = AppTheme.FontBody;
        cboSurfaceType.Location = new Point(278, 320);
        cboSurfaceType.Size = new Size(230, 32);

        // 5. Giá giờ chuẩn
        lblDefaultPrice.Text = "Giá Thuê Chuẩn (VNĐ / giờ):";
        lblDefaultPrice.Font = AppTheme.FontCaptionBold;
        lblDefaultPrice.ForeColor = AppTheme.TextMain;
        lblDefaultPrice.Location = new Point(28, 366);
        lblDefaultPrice.AutoSize = true;

        txtDefaultPrice.PlaceholderText = "Mặc định theo chuẩn môn (VD: 150000)";
        txtDefaultPrice.LeftIcon = IconChar.Coins;
        txtDefaultPrice.Location = new Point(28, 388);
        txtDefaultPrice.Size = new Size(480, 44);

        pnlBody.Controls.Add(lblBranch);
        pnlBody.Controls.Add(cboBranch);
        pnlBody.Controls.Add(lblBranchHint);
        pnlBody.Controls.Add(lblBranchAddress);
        pnlBody.Controls.Add(lblCourtName);
        pnlBody.Controls.Add(txtCourtName);
        pnlBody.Controls.Add(lblCourtLocation);
        pnlBody.Controls.Add(txtCourtLocation);
        pnlBody.Controls.Add(lblLocationTip);
        pnlBody.Controls.Add(lblSportType);
        pnlBody.Controls.Add(cboSportType);
        pnlBody.Controls.Add(lblSurfaceType);
        pnlBody.Controls.Add(cboSurfaceType);
        pnlBody.Controls.Add(lblDefaultPrice);
        pnlBody.Controls.Add(txtDefaultPrice);

        // 
        // pnlFooter
        // 
        pnlFooter.Dock = DockStyle.Bottom;
        pnlFooter.Height = 68;
        pnlFooter.BackColor = AppTheme.CardBg;
        pnlFooter.Padding = new Padding(24, 14, 24, 14);
        pnlFooter.Paint += (s, e) =>
        {
            using var pen = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
        };

        btnCancel.Text = "Hủy Bỏ";
        btnCancel.ButtonType = AppButtonType.Secondary;
        btnCancel.Size = new Size(110, 40);
        btnCancel.Location = new Point(300, 14);
        btnCancel.DialogResult = DialogResult.Cancel;

        btnSave.Text = "Tạo Sân Mới";
        btnSave.ButtonType = AppButtonType.Primary;
        btnSave.Icon = IconChar.Plus;
        btnSave.Size = new Size(140, 40);
        btnSave.Location = new Point(420, 14);

        pnlFooter.Controls.Add(btnCancel);
        pnlFooter.Controls.Add(btnSave);

        Controls.Add(pnlBody);
        Controls.Add(pnlFooter);
        Controls.Add(pnlHeader);

        AcceptButton = btnSave;
        CancelButton = btnCancel;

        ResumeLayout(false);
    }
}
