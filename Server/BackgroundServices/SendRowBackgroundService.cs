using Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Server.Data;
using Server.SignalR;
using System.Diagnostics;

namespace Server.BackgroundServices;

public class SendRowBackgroundService : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(1);
    private readonly IGenerator<Product> _productGenerator;
    private readonly IHubContext<SendRowHub, ISendRow> _sendRowHub;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private bool _ready;

    public SendRowBackgroundService(
        IHostApplicationLifetime hostApplicationLifetime,
        IGenerator<Product> productGenerator,
        IHubContext<SendRowHub, ISendRow> sendRowHub,
        IServiceScopeFactory serviceScopeFactory)
    {
        hostApplicationLifetime.ApplicationStarted.Register(() => _ready = true);
        _productGenerator = productGenerator;
        _sendRowHub = sendRowHub;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //Preventing service from starting when IHost starts
        if (!_ready)
            return;

        using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await GenerateAndSendRow();
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Timed Hosted Service is stopping.");
        }
    }

    private async Task GenerateAndSendRow()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetService<ServerContext>();

        if (context == null)
            return;

        var product = _productGenerator.Generate();
        await context.AddAsync(product);
        await context.SaveChangesAsync();

        await _sendRowHub.Clients.All.SendRow(product);
    }
}