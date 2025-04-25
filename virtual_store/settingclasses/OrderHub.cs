using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class OrderHub : Hub
{
    public async Task JoinStoreGroup(string storeId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, storeId);
    }
}
