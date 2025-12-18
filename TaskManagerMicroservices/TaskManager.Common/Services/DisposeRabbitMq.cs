using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Common.Interfaces;

namespace TaskManager.Common.Services
{
    public class DisposeRabbitMq : BackgroundService
    {
        private readonly IRabbitMqConnection _rabbitConnection;
        public DisposeRabbitMq(IRabbitMqConnection r)
        {
            _rabbitConnection = r;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            } finally { DisposeRabbitMQ(); }
        }

        private void DisposeRabbitMQ()
        {
            Console.WriteLine("DISPOSE DE RABBIT EJECUTADO");
            _rabbitConnection.Dispose();
        }
    }
}
