using Microsoft.AspNetCore.SignalR;
using SportChain.Shared.Enums;

namespace SportChain.WebApi.Hubs;

public class CourtHub : Hub
{
    private readonly ILogger<CourtHub> _logger;

    public CourtHub(ILogger<CourtHub> logger)
    {
        _logger = logger;
    }

    // Khi khách hàng mở màn hình ma trận sân của một chi nhánh
    public async Task JoinBranchGroup(int branchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Branch_{branchId}");
        _logger.LogInformation("📡 [SignalR] Client {ConnectionId} đã tham gia nhóm Branch_{BranchId}", Context.ConnectionId, branchId);
    }

    public async Task LeaveBranchGroup(int branchId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Branch_{branchId}");
        _logger.LogInformation("📡 [SignalR] Client {ConnectionId} đã rời khỏi nhóm Branch_{BranchId}", Context.ConnectionId, branchId);
    }

    // Tương thích ngược với tên JoinBranchRoom / LeaveBranchRoom
    public async Task JoinBranchRoom(int branchId) => await JoinBranchGroup(branchId);
    public async Task LeaveBranchRoom(int branchId) => await LeaveBranchGroup(branchId);

    // Server broadcast cập nhật trạng thái ô ca (Xanh, Vàng, Đỏ) kèm ngày
    public async Task BroadcastSlotStatusChanged(int branchId, int courtId, int slotId, SlotStatus status, string? dateStr = null)
    {
        await Clients.Group($"Branch_{branchId}").SendAsync("OnSlotStatusChanged", courtId, slotId, status, dateStr);
    }
}
