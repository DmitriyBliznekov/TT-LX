using Domain.Models;
using Microsoft.AspNetCore.SignalR;

namespace Server.SignalR;

public class SendRowHub : Hub<ISendRow>
{
    public async Task SendRow(Product product) => await Clients.All.SendRow(product);
}