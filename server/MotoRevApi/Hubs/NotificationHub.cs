using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace MotoRevApi.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // O SignalR já mapeia Context.UserIdentifier automaticamente se o JWT estiver configurado
        await base.OnConnectedAsync();
    }
}