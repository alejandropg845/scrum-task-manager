using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.Common.DTOs;
using TaskManager.Sprints.Interfaces;

namespace TaskManager.Sprints.Controllers
{
    [ApiController]
    [Route("api/retrospectives")]
    public class RetrospectivesController : ControllerBase
    {
        private readonly IRetrospectivesService _service;
        public RetrospectivesController(IRetrospectivesService service)
        {
            _service = service;
        }

        [HttpGet("GetRetrospectives/{groupName}")]
        [Authorize]
        public async Task<ActionResult> GetRetrospectives([FromRoute] string groupName)
        {
            if (!await IsAuthorized(groupName)) return Forbid(); 

            if (string.IsNullOrEmpty(groupName) || groupName is "undefined" || groupName is "null")
                return Forbid();

            var retrospectives = await _service.GetRetrospectivesAsync(groupName);

            return Ok(retrospectives);
        }

        [HttpPost("CreateRetrospective")]
        [Authorize]
        public async Task<ActionResult> CreateRetrospective([FromBody] CreateRetrospectiveDto dto)
        {
            string? username = User.FindFirstValue(ClaimTypes.Name);

            if (username is null) return Unauthorized();

            string? token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");

            if (token is null) return Unauthorized();

            var result = await _service.AddRetroAndFeedbackAsync(dto, username);

            return Ok(new { CreatedRetro = result.retro, Message = "Thanks for your feedback!"});

        }

        [NonAction]
        [Authorize]
        public async Task<bool> IsAuthorized(string groupName)
        {
            string? token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");

            return await _service.IsAuthorizedByGroupRoleAsync(groupName, token);
        }
    }
}
