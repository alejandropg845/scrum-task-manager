using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskManager.Common.Middlewares
{
    public class ExceptionHandling
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandling> _logger;
        public ExceptionHandling(RequestDelegate next, ILogger<ExceptionHandling> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try { await _next(httpContext); }
            
            catch (Exception ex) 
            {
                _logger.LogError("Error en respuesta desde Middleware\n" +
                    "Excepción: {Msg}\n" +
                    "StackTrace: {StackTrace}",
                    ex.Message, ex.StackTrace
                );

                httpContext.Response.Clear();
                httpContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                httpContext.Response.ContentType = "application/json";

                var response = new
                {
                    Status = httpContext.Response.StatusCode,
                    Title = "Server error",
                    Type = "Server error",
                    Message = "There was an unknown error. Please, try again"
                };

                var serializedResponse = JsonSerializer.Serialize(response);

                await httpContext.Response.WriteAsync(serializedResponse);
            }
        }
    }
}
