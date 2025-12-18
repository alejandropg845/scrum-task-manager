using DnsClient.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Common.Configurations;
using TaskManager.Common.Interfaces;

namespace TaskManager.Common.Services
{
    public class RabbitMqConnection : IDisposable, IRabbitMqConnection
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        public RabbitMqConnection(IConfiguration config)
        {
            var rabbitSettings = config.GetSection(nameof(RabbitSettings)).Get<RabbitSettings>()!;

            Console.WriteLine();

            var factory = new ConnectionFactory
            {
                //Uri = new Uri(rabbitSettings.Uri),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                TopologyRecoveryEnabled = true,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(15),
                HostName = rabbitSettings.HostName,
                UserName = rabbitSettings.Username,
                Password = rabbitSettings.Password
            };

            _connection = factory.CreateConnection();

            _channel = _connection.CreateModel();

        }

        public IConnection GetConnection() => _connection;
        public IModel GetChannel() => _channel;

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
