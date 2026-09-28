using Microsoft.AspNetCore.SignalR;
using SportChain.Shared.Enums;

namespace SportChain.WebApi.Hubs;

public class CourtHub : Hub
{
    // Khi khách hàng mở màn hình ma trận sân của một chi nhánh
    public async Task JoinBranchRoom(int branchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Branch_{branchId}");
    }

    // Khi khách rời khỏi chi nhánh đó
    public async Task LeaveBranchRoom(int branchId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Branch_{branchId}");
    }

    // Server broadcast cập nhật trạng thái ô ca (Xanh, Vàng, Đỏ)
    public async Task BroadcastSlotStatusChanged(int branchId, int courtId, int slotId, SlotStatus status)
    {
        await Clients.Group($"Branch_{branchId}").SendAsync("OnSlotStatusChanged", courtId, slotId, status);
    }
}
