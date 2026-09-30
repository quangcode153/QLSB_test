using Microsoft.AspNetCore.SignalR.Client;
using SportChain.Shared.Enums;

namespace SportChain.WinForms.Services;

public class WinFormsRealtimeService : IAsyncDisposable
{
    private static WinFormsRealtimeService? _instance;
    public static WinFormsRealtimeService Instance => _instance ??= new WinFormsRealtimeService();

    private HubConnection? _hubConnection;
    private string HubUrl => Common.AppConfig.HubUrl;
    private int? _currentBranchId;
    private SynchronizationContext? _syncContext;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
    public HubConnectionState CurrentState => _hubConnection?.State ?? HubConnectionState.Disconnected;
    public event Action<int, int, SlotStatus, string?>? OnSlotStatusChanged;
    public event Action<HubConnectionState>? OnConnectionStateChanged;

    public WinFormsRealtimeService()
    {
        _syncContext = SynchronizationContext.Current;
    }

    public void SetSyncContext(SynchronizationContext context)
    {
        _syncContext = context;
    }

    private void NotifyStateChanged(HubConnectionState state)
    {
        if (_syncContext != null)
        {
            _syncContext.Post(_ => OnConnectionStateChanged?.Invoke(state), null);
        }
        else
        {
            OnConnectionStateChanged?.Invoke(state);
        }
    }

    public async Task StartAsync()
    {
        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
        {
            NotifyStateChanged(HubConnectionState.Connected);
            return;
        }

        if (_hubConnection == null)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(HubUrl)
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5) })
                .Build();

            _hubConnection.On<int, int, SlotStatus, string?>("OnSlotStatusChanged", (courtId, timeSlotId, newStatus, dateStr) =>
            {
                // Đảm bảo event được gọi về đúng UI thread của Windows Forms
                if (_syncContext != null)
                {
                    _syncContext.Post(_ => OnSlotStatusChanged?.Invoke(courtId, timeSlotId, newStatus, dateStr), null);
                }
                else
                {
                    OnSlotStatusChanged?.Invoke(courtId, timeSlotId, newStatus, dateStr);
                }
            });

            _hubConnection.Reconnecting += (exception) =>
            {
                NotifyStateChanged(HubConnectionState.Reconnecting);
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += async (connectionId) =>
            {
                NotifyStateChanged(HubConnectionState.Connected);
                if (_currentBranchId.HasValue)
                {
                    await JoinBranchAsync(_currentBranchId.Value);
                }
            };

            _hubConnection.Closed += (exception) =>
            {
                NotifyStateChanged(HubConnectionState.Disconnected);
                return Task.CompletedTask;
            };
        }

        try
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                NotifyStateChanged(HubConnectionState.Connecting);
                await _hubConnection.StartAsync();
                NotifyStateChanged(HubConnectionState.Connected);
            }

            if (_currentBranchId.HasValue)
            {
                await JoinBranchAsync(_currentBranchId.Value);
            }
        }
        catch (Exception ex)
        {
            NotifyStateChanged(HubConnectionState.Disconnected);
            System.Diagnostics.Debug.WriteLine($"[SignalR WinForms] Lỗi kết nối: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"[SignalR WinForms] LeaveBranchGroup error: {ex.Message}");
            }
        }

        _currentBranchId = branchId;
        if (IsConnected && _hubConnection != null)
        {
            try
            {
                await _hubConnection.InvokeAsync("JoinBranchGroup", branchId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SignalR WinForms] JoinBranchGroup error: {ex.Message}");
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SignalR WinForms] LeaveBranchGroup error: {ex.Message}");
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
