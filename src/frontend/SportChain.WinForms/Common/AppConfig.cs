namespace SportChain.WinForms.Common;

public static class AppConfig
{
    public const string AppName = "SportChain VN";
    public const string AppVersion = "2.0 (WinForms .NET 8)";
    
    // Đường dẫn máy chủ Web API
    public static string BaseApiUrl { get; set; } = "http://localhost:5097";
    
    // Đường dẫn WebSocket SignalR
    public static string HubUrl => $"{BaseApiUrl}/hubs/court";
}
