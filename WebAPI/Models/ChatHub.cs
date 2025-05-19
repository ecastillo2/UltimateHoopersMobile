using DataLayer;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace WebAPI.Models
{
    // Install Microsoft.AspNetCore.SignalR
    public class ChatHub : Hub
    {
        private readonly HUDBContext _context;

        public ChatHub(HUDBContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
