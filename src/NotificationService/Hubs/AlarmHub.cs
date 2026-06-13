using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Hubs;

public class AlarmHub : Hub
{
    // Class is empty because the NotificationService doesn't recieve messages from the client
    // But SignalR needs a channel to push messages 
}