namespace SportChain.WinForms.Common;

/// <summary>
/// Quản lý cấu hình toàn cục cho WinForms client.
/// Chỉ đọc (Read-only) địa chỉ máy chủ từ thư mục ẩn Config/ hoặc biến môi trường hệ thống.
/// Người dùng thông thường KHÔNG CÓ QUYỀN ghi hoặc thay đổi địa chỉ trên giao diện.
/// </summary>
public static class AppConfig
{
    public const string AppName = "SportChain VN";
    public const string AppVersion = "2.0 (WinForms .NET 8)";

    public const string DefaultUrl = "http://localhost:5097";
    
    /// <summary>
    /// Địa chỉ máy chủ Web API (Chỉ đọc - Read-only, được nạp một lần khi khởi động ứng dụng).
    /// </summary>
    public static string BaseApiUrl { get; private set; } = DefaultUrl;

    /// <summary>
    /// Đường dẫn WebSocket SignalR Hub (Chỉ đọc).
    /// </summary>
    public static string HubUrl => $"{BaseApiUrl}/hubs/court";

    static AppConfig()
    {
        LoadConfiguration();
    }

    /// <summary>
    /// Nạp cấu hình chỉ đọc theo thứ tự ưu tiên:
    /// 1. Ưu tiên 1: File cấu hình server.env hoặc .env trong thư mục ẩn Config/ (Thư mục này bị Git chặn 100%)
    /// 2. Ưu tiên 2: Biến môi trường hệ điều hành SPORTCHAIN_API_URL
    /// 3. Mặc định dự phòng an toàn: http://localhost:5097
    /// </summary>
    public static void LoadConfiguration()
    {
        try
        {
            // 1. Kiểm tra trong thư mục ẩn Config/ hoặc thư mục chạy
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var currentDir = Directory.GetCurrentDirectory();

            var candidatePaths = new[]
            {
                Path.Combine(baseDir, "Config", "server.env"),
                Path.Combine(baseDir, "Config", ".env"),
                Path.Combine(currentDir, "Config", "server.env"),
                Path.Combine(currentDir, "Config", ".env"),
                Path.Combine(baseDir, ".env"),
                Path.Combine(currentDir, ".env")
            };

            foreach (var path in candidatePaths)
            {
                if (File.Exists(path))
                {
                    foreach (var line in File.ReadAllLines(path))
                    {
                        var trimmed = line.Trim();
                        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#")) continue;

                        var parts = trimmed.Split('=', 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim();
                            var val = parts[1].Trim().Trim('"', '\'');

                            if (key.Equals("SPORTCHAIN_API_URL", StringComparison.OrdinalIgnoreCase) ||
                                key.Equals("API_URL", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    BaseApiUrl = NormalizeUrl(val);
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            // 2. Kiểm tra biến môi trường hệ thống
            var envVar = Environment.GetEnvironmentVariable("SPORTCHAIN_API_URL");
            if (!string.IsNullOrWhiteSpace(envVar))
            {
                BaseApiUrl = NormalizeUrl(envVar);
                return;
            }

            // 3. Fallback mặc định
            BaseApiUrl = DefaultUrl;
        }
        catch
        {
            BaseApiUrl = DefaultUrl;
        }
    }

    private static string NormalizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return DefaultUrl;
        url = url.Trim();
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = "http://" + url;
        }
        return url.TrimEnd('/');
    }
}
