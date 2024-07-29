using Domain.Models;

namespace Server.SignalR;

public interface ISendRow
{
    Task SendRow(Product product);
}