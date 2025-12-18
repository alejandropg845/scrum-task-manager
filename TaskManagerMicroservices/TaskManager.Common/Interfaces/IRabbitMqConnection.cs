using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Common.Interfaces
{
    public interface IRabbitMqConnection
    {
        IConnection GetConnection();
        IModel GetChannel();
        void Dispose();
    }
}
