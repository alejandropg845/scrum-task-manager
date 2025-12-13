using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;
using System.Transactions;
using TaskManager.Common.Documents;
using TaskManager.Sprints.Interfaces;

namespace TaskManager.Sprints.Controllers
{
    [ApiController]
    [Route("api/feedbacks")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _service;
        public FeedbackController(IFeedbackService s)
        {
            _service = s;
        }

        [HttpGet("IsFeedbackSubmited/{groupName}")]
        [Authorize]
        public async Task<ActionResult> IsFeedbackSubmited([FromRoute] string groupName, [FromQuery] string sprintId)
        {
            string? username = User.FindFirstValue(ClaimTypes.Name);

            if (username is null) return Unauthorized();

            var (SprintId, IsSubmited) = await _service.IsFeedbackSubmitedAsync(username, groupName, sprintId);

            return Ok(new { SprintId, IsSubmited });
        }

        [HttpPut("MarkFeedbackAsSubmited/{username}")]
        [Authorize]
        public async Task<ActionResult> MarkFeedbackAsSubmited([FromRoute] string username, [FromQuery] string groupName, [FromQuery] string sprintId)
        {
            await _service.MarkFeedbackAsSubmitedAsync(username, groupName, sprintId);

            return Ok(new { Message = "Done" });
        }

        [HttpPost("AddFeedbackToDevelopers/{groupName}")]
        [Authorize]
        public async Task<ActionResult> AddFeedbackToUsersAsync([FromRoute] string groupName, [FromQuery] string sprintId)
        {
            string? token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");

            if (token is null) return Unauthorized();

            await _service.AddFeedbackToUsersAsync(groupName, sprintId, token);

            return NoContent();
        }
    }
}
