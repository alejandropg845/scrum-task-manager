using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Diagnostics.Tracing;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TaskManager.Authentication.Interfaces;
using TaskManager.Common.Configurations;
using TaskManager.Common.DTOs;

namespace TaskManager.Authentication.Controllers
{
    [ApiController]
    [Route("api-gateway")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AuthenticationController> _logger;
        public AuthenticationController(IAuthenticationService authService, ILogger<AuthenticationController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("SendRequest")]
        [Authorize]
        public async Task<ActionResult> SendRequest([FromHeader] string method, [FromHeader] string microservice, [FromHeader] string endpoint)
        {
            string? username = User.FindFirstValue(ClaimTypes.Name);
            if (username is null) return Unauthorized();

            JsonDocument? jsonBody = null;

            if (method.ToUpper() is not "DELETE")
            {
                using var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);

                var body = await reader.ReadToEndAsync();

                jsonBody = string.IsNullOrEmpty(body) ? null : JsonSerializer.Deserialize<JsonDocument>(body);
            }


            var requestInfo = new RequestInfo(method, microservice, endpoint, username, jsonBody);

            var response = await _authService.SendRequestAsync(requestInfo);

            if (response.Content.Headers.TryGetValues("Content-Type", out var headers) && response.IsSuccessStatusCode)
            {
                if (headers.First() == "application/pdf")
                {
                    byte[] pdfBytes = await response.Content.ReadAsByteArrayAsync();
                    return File(pdfBytes, "application/pdf");
                }
            }

            /* La respuesta es de tipo json */
            var jsonContent = await response.Content.ReadAsStringAsync();
        
            var content = string.IsNullOrEmpty(jsonContent)
            ? null : await response.Content.ReadFromJsonAsync<JsonDocument>();

            if (response.IsSuccessStatusCode)
                return Ok(content);
            if ((int)response.StatusCode == 401)
                return Unauthorized();

            if ((int)response.StatusCode == 404)
                return NotFound(content);

            if ((int)response.StatusCode == 400)
                return BadRequest(content);

            if ((int)response.StatusCode == 403)
                return Forbid();
            else
                return StatusCode(500);

            
            
        }

        
    }
}
