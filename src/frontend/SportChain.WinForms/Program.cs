namespace SportChain.WinForms;

static class Program
{
    /// <summary>
    ///  Điểm khởi chạy chính của ứng dụng SportChain Desktop (WinForms .NET 8).
    /// </summary>
    [STAThread]
    static void Main()
    {
        string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "winforms_debug.log");
        try
        {
            File.WriteAllText(logPath, $"[INFO] WinForms starting at {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

            // 1. Khởi tạo cấu hình ứng dụng WinForms .NET 8 (High DPI PerMonitorV2, Visual Styles)
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            ApplicationConfiguration.Initialize();
            File.AppendAllText(logPath, "[INFO] ApplicationConfiguration.Initialize() succeeded\n");

            // 2. Đăng ký bộ xử lý lỗi toàn cục tránh crash ứng dụng đột ngột
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) =>
            {
                File.AppendAllText(logPath, $"[THREAD_EXCEPTION] {e.Exception}\n");
                MessageBox.Show($"Đã xảy ra lỗi trong quá trình xử lý giao diện:\n{e.Exception.Message}",
                    "Thông Báo Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                File.AppendAllText(logPath, $"[UNHANDLED_EXCEPTION] {e.ExceptionObject}\n");
                if (e.ExceptionObject is Exception ex)
                {
                    MessageBox.Show($"Lỗi nghiêm trọng không xác định:\n{ex.Message}",
                        "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            File.AppendAllText(logPath, "[INFO] Creating LoginForm...\n");
            var loginForm = new Forms.LoginForm();
            File.AppendAllText(logPath, "[INFO] LoginForm created. Running Application.Run...\n");

            // 3. Khởi chạy màn hình Đăng Nhập (LoginForm)
            Application.Run(loginForm);

            File.AppendAllText(logPath, "[INFO] Application.Run exited.\n");
        }
        catch (Exception ex)
        {
            File.AppendAllText(logPath, $"[MAIN_EXCEPTION] {ex}\n");
        }
    }    
}