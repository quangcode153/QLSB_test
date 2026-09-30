using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using SportChain.WinForms.UI.Theme;

namespace SportChain.WinForms.UI.Controls;

/// <summary>
/// DataGridView chuẩn hóa theo phong cách phẳng hiện đại: Header cao 42px, Hàng cao 40px, Zebra striping nhẹ, Selection Teal nhạt và bật DoubleBuffered chống nháy.
/// </summary>
public class StyledGrid : DataGridView
{
    public StyledGrid()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        EnableDoubleBuffering();

        Dock = DockStyle.Fill;
        BackgroundColor = AppTheme.MainBg;
        BorderStyle = BorderStyle.None;
        CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        GridColor = AppTheme.Border;
        RowHeadersVisible = false;
        SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        MultiSelect = false;
        AllowUserToAddRows = false;
        AllowUserToDeleteRows = false;
        AllowUserToResizeRows = false;
        ReadOnly = true;
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        // Cấu hình Header
        EnableHeadersVisualStyles = false;
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        ColumnHeadersHeight = 42;
        ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = AppTheme.CardBgAlt,
            ForeColor = AppTheme.TextMain,
            Font = AppTheme.FontCaptionBold,
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 12, 0)
        };

        // Cấu hình Hàng và Màu chọn (Selection)
        RowTemplate.Height = 40;
        DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = AppTheme.CardBg,
            ForeColor = AppTheme.TextMain,
            Font = AppTheme.FontCaption,
            SelectionBackColor = Color.FromArgb(204, 251, 241), // Teal 100
            SelectionForeColor = AppTheme.TextMain,
            Padding = new Padding(12, 0, 12, 0)
        };

        // Zebra Striping nhẹ
        AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(248, 250, 252), // Slate 50
            ForeColor = AppTheme.TextMain,
            SelectionBackColor = Color.FromArgb(204, 251, 241),
            SelectionForeColor = AppTheme.TextMain,
            Padding = new Padding(12, 0, 12, 0)
        };
    }

    private void EnableDoubleBuffering()
    {
        try
        {
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, this, new object[] { true });
        }
        catch { }
    }
}
