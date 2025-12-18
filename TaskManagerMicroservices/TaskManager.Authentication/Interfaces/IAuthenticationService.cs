using System.Text.Json;
using TaskManager.Common.DTOs;

namespace TaskManager.Authentication.Interfaces
{
    public interface IAuthenticationService
    {
        Task<HttpResponseMessage> SendRequestAsync(RequestInfo info);

    }
}
