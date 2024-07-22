using Microsoft.AspNetCore.SignalR;

namespace Server.SignalR
{
    public class SendRowHub : Hub<ISendRow>
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task SendRow(string product)
        {
            await Clients.All.SendRow(product);
        }
    }
}
