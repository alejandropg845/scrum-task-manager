using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Common.DTOs;
using TaskManager.Sprints.Interfaces;

namespace TaskManager.Sprints.Controllers
{
    [ApiController]
    [Route("api/sprints")]
    public class SprintsController : ControllerBase
    {
        private readonly ISprintWriteService _writeService;
        private readonly ISprintReadService _readService;
        public SprintsController(ISprintWriteService service, ISprintReadService readService)
        {
            _writeService = service;
            _readService = readService;
        }

        [HttpGet("GetGroupSprints/{groupName}")]
        [Authorize]
        public async Task<ActionResult> GetGroupSprints([FromRoute] string groupName)
        {
            return Ok(await _readService.GetGroupSprintsAsync(groupName));
        }

        [HttpGet("GetSprint/{groupName}")]
        [Authorize]
        public async Task<ActionResult> GetSprint([FromRoute] string groupName)
        {
            var (CurrentSprint, PreviousSprintId) = await _writeService.GetPreviousAndCurrentSprintAsync(groupName);

            return Ok(new { CurrentSprint, PreviousSprintId }); 

        }
        [HttpPost("CreateSprint/{groupName}")]
        [Authorize]
        public async Task<ActionResult> CreateSprint([FromRoute] string groupName, [FromQuery] int sprintNumber, [FromQuery] string sprintId)
        {
            string? token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");

            if (token is null) return Unauthorized();
            /* Al ser el argumento de sprintNumber 1, quiere decir que el usuario apenas creó un grupo con SCRUM */
            var r = await _writeService.CreateSprintAsync(sprintId, groupName, sprintNumber);

            return Ok(new
            {
                r.ExpirationTime,
                r.Status,
                CreatedSprintId = r.Id
            });
        }

        [HttpPut("BeginSprint")]
        [Authorize]
        public async Task<ActionResult> BeginSprint(BeginSprintDto dto)
        {
            var response = await _writeService.BeginSprintAsync(dto);

            return Ok(new
            {
                response.ExpirationTime,
                response.TasksIds,
                response.SprintId,
                response.SprintName,
                response.RemainingTime
            });
        }

        [HttpGet("GetSprintNumber/{groupName}")]
        [Authorize]
        public async Task<ActionResult> GetSprintNumber([FromRoute] string groupName)
        {
            int result = await _readService.GetSprintNumberAsync(groupName);
            return Ok(new { result });
        }

        [HttpPut("CycleSprint")]
        [Authorize]
        public async Task<ActionResult> SetSprintAsCompleted([FromBody] SprintToComplete s)
        {
            string? token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");

            if (token is null) return Unauthorized();

            var (completedSprintId, createdSprintId) = 
                await _writeService.CycleSprintAsync(s, token);

            bool allOk = (!string.IsNullOrEmpty(completedSprintId) && !string.IsNullOrEmpty(createdSprintId));

            if (allOk) return Ok(new { completedSprintId, createdSprintId });

            return StatusCode(500);
        }

        [HttpGet("CanMarkSprintTaskItemAsCompleted/{sprintId}")]
        [Authorize]
        public async Task<ActionResult> SprintExists(string sprintId)
        {
            bool result = await _readService.CanMarkSprintTaskItemAsCompletedAsync(sprintId);
            return Ok(new { result });
        }

        [HttpDelete("DeleteSprint/{sprintId}")]
        [Authorize]
        public async Task<ActionResult> DeleteSprintAsync(string sprintId)
        {
            await _writeService.DeleteSprintAsync(sprintId);
            return NoContent();

        }
        [HttpDelete("RevertCycledSprintAsync/{completedSprintId}")]
        [Authorize]
        public async Task<ActionResult> DeleteSetSprintAsCompleted([FromRoute] string completedSprintId, [FromQuery] string createdSprintId, [FromQuery] string groupName)
        {
            await _writeService.RevertCycledSprintAsync(groupName, completedSprintId, createdSprintId);

            return NoContent();

        }

        [HttpGet("GetSummary/{groupName}")]
        [Authorize]
        public async Task<ActionResult> GenerateSummary(string groupName)
        {
            string? token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");

            if (token is null) return Unauthorized();

            (byte[] PdfBytes, bool IsAuthorized) = await _writeService.GenerateSummaryAsync(token, groupName);

            if (PdfBytes.Length == 0) return BadRequest(new { Message = "There are no sprints in this group" });

            if (!IsAuthorized) return Forbid();

            return File(PdfBytes, "application/pdf", $"{groupName}_SprintsSummary.pdf");
        }

    }
    
}
