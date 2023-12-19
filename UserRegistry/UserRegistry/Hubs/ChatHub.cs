using Microsoft.AspNetCore.SignalR;

namespace UserRegistry.Hubs;

public class ChatHub : Hub
{
    public Task SendMessage(string user, string message)
    {
        return Clients.All.SendAsync("RecieveMessage", user, message);
    }

}
