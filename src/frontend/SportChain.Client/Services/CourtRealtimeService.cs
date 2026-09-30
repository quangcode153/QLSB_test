using Microsoft.AspNetCore.SignalR.Client;
using SportChain.Shared.Enums;

namespace SportChain.Client.Services;

public interface ICourtRealtimeService : IAsyncDisposable
{
    bool IsConnected { get; }
    event Action<int, int, SlotStatus, string?>? OnSlotStatusChanged;
    Task StartAsync();
    Task JoinBranchAsync(int branchId);
    Task LeaveBranchAsync(int branchId);
}

public class CourtRealtimeService : ICourtRealtimeService
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl;
    private int? _currentBranchId;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
    public event Action<int, int, SlotStatus, string?>? OnSlotStatusChanged;

    public CourtRealtimeService(IConfiguration configuration)
    {
        var baseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5097";
        _hubUrl = $"{baseUrl}/hubs/court";
    }

    public async Task StartAsync()
    {
        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
        {
            return;
        }

        if (_hubConnection == null)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5) })
                .Build();

            _hubConnection.On<int, int, SlotStatus, string?>("OnSlotStatusChanged", (courtId, timeSlotId, newStatus, dateStr) =>
            {
                Console.WriteLine($"[SignalR] Received OnSlotStatusChanged: Court {courtId}, Slot {timeSlotId} -> {newStatus}, Date: {dateStr}");
                OnSlotStatusChanged?.Invoke(courtId, timeSlotId, newStatus, dateStr);
            });

            _hubConnection.Reconnected += async (connectionId) =>
            {
                Console.WriteLine($"[SignalR] Reconnected. Id: {connectionId}");
                if (_currentBranchId.HasValue)
                {
                    await JoinBranchAsync(_currentBranchId.Value);
                }
            };
        }

        try
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
                Console.WriteLine($"[SignalR] Connected to {_hubUrl}");
            }

            if (_currentBranchId.HasValue)
            {
                await JoinBranchAsync(_currentBranchId.Value);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Connection error: {ex.Message}");
        }
    }

    public async Task JoinBranchAsync(int branchId)
    {
        if (_hubConnection == null || _hubConnection.State == HubConnectionState.Disconnected)
        {
            _currentBranchId = branchId;
            await StartAsync();
            return;
        }

        if (_currentBranchId.HasValue && _currentBranchId.Value != branchId && IsConnected)
        {
            try
            {
                await _hubConnection.InvokeAsync("LeaveBranchGroup", _currentBranchId.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR] LeaveBranchGroup error: {ex.Message}");
            }
        }

        _currentBranchId = branchId;
        if (IsConnected && _hubConnection != null)
        {
            try
            {
                await _hubConnection.InvokeAsync("JoinBranchGroup", branchId);
                Console.WriteLine($"[SignalR] Joined branch group {branchId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR] JoinBranchGroup error: {ex.Message}");
            }
        }
    }

    public async Task LeaveBranchAsync(int branchId)
    {
        if (IsConnected && _hubConnection != null)
        {
            try
            {
                await _hubConnection.InvokeAsync("LeaveBranchGroup", branchId);
                Console.WriteLine($"[SignalR] Left branch group {branchId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR] LeaveBranchGroup error: {ex.Message}");
            }
        }
        if (_currentBranchId == branchId)
        {
            _currentBranchId = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
