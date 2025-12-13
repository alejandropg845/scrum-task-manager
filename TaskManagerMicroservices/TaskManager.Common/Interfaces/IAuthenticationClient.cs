using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskManager.Common.Interfaces
{
    public interface IAuthenticationClient
    {
        Task<JsonDocument?> SendRequestAsync(string microservice, string endpoint, string method, string token, object? body);
    }
}
