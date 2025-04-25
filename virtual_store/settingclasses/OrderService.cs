using Microsoft.AspNetCore.SignalR;
using virtual_store.Models;
using virtual_store.settingclasses;

public class OrderService
{
    private readonly IHubContext<OrderHub> _hubContext;
    private readonly VirtualStoreContext db;
    private readonly IConfiguration _config;

    public OrderService(IHubContext<OrderHub> hubContext, VirtualStoreContext _db, IConfiguration config)
    {
        _hubContext = hubContext;
        db = _db;
        _config = config;
    }

    public async Task AddNewOrder(int storeId)
    {
        // Your logic for adding a new order in the database
        await _hubContext.Clients.Group(storeId.ToString()).SendAsync("ReceiveOrderNotification", $"New Order Placed for Store {storeId}!");
    }
}
