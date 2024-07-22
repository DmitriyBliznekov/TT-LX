using Microsoft.AspNetCore.SignalR;
using Server.Data;
using Server.Models;
using Server.SignalR;
using System.Text.Json;

namespace Server.BackgroundServices
{
    public class SendRowBackgroundService : IHostedService, IDisposable
    {
        private bool _ready;
        private readonly ProductGenerator _productGenerator;
        private Timer _timer = null;
        private readonly IHubContext<SendRowHub, ISendRow> _sendRowHub;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SendRowBackgroundService(
            IHostApplicationLifetime hostApplicationLifetime, 
            ProductGenerator productGenerator, 
            IHubContext<SendRowHub, ISendRow> sendRowHub,
            IServiceScopeFactory serviceScopeFactory)
        {
            hostApplicationLifetime.ApplicationStarted.Register(() => _ready = true);
            _productGenerator = productGenerator;
            _sendRowHub = sendRowHub;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //Preventing service from starting at IHost start
            if (!_ready)
                return Task.CompletedTask;

            if (_timer == null)
            {
                _timer = new Timer(
                    GenerateAndSendRow,
                    null,
                    TimeSpan.FromSeconds(0),
                    TimeSpan.FromSeconds(10));
            }
            else
            {
                _timer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5));
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private void GenerateAndSendRow(object obj)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetService<ServerContext>();

            var product = _productGenerator.Generate();
            context.Product.Add(product);
            context.SaveChanges();
            
            _sendRowHub.Clients.All.SendRow(JsonSerializer.Serialize<Product>(product) + "\u001e");
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
