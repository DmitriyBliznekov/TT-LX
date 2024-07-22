namespace Server.SignalR
{
    public interface ISendRow
    {
        Task SendRow(string product);
    }
}