using Domain.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Client.SignalR;

public class ProductMonitor
{
    private readonly HubConnection _connection;
    public event Action<Product> ProductReceived;

    public ProductMonitor(string serverAddress)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{serverAddress}/send_row")
            .Build();

        _connection.On("SendRow", (Product product) => ReceiveRow(product));
    }

    public async Task StartMonitoring() => await _connection.StartAsync();

    public async Task StopMonitoring() => await _connection.StopAsync();

    private void ReceiveRow(Product product) => ProductReceived?.Invoke(product);
}