using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.TaskItems.Clients;
using TaskManager.TaskItems.Interfaces;

namespace TaskManager.TaskItems.Controllers
{
    [ApiController]
    [Route("api/taskItems")]
    public class TaskItemsController : ControllerBase
    {
        private readonly ITaskItemsWriteService _writeService;
        private readonly ITaskItemsReadService _readService;
        public TaskItemsController(ITaskItemsWriteService service, ITaskItemsReadService readService)
        {
            _writeService = service;
            _readService = readService;
        }

        [HttpGet("GetTaskItems/{taskId}")]
        [Authorize]
        public async Task<ActionResult<IReadOnlyList<TaskItem>>> GetTaskItems([FromRoute] string taskId)
        {
            return Ok(await _readService.GetTaskItemsAsync(taskId));
        }

        [HttpPost("CreateTaskItem")]
        [Authorize]
        public async Task<ActionResult> CreateTaskItem(CreateTaskItemDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string? username = User.FindFirstValue(ClaimTypes.Name);

            if(username is not null)
            {
                string token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");

                var r = await _writeService.CreateTaskItemAsync(dto, username, token);

                if (r.AssignToUserError) return BadRequest(new { Message = "You must assign this task to an user because your task is shared" });

                if (r.ContainsScrum) return BadRequest(new { Message = "Cannot add this taskItem because belongs to a sprint" });

                return Ok(r.ti);
            }
            return Unauthorized();
            
            
        }

        [HttpPut("UpdateTaskItem")]
        [Authorize]
        public async Task<ActionResult> UpdateTaskItem([FromBody] UpdateTaskItemDto dto)
        {
            string? token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");

            if (token is null) return Unauthorized();

            var response = await _writeService.UpdateTaskItemAsync(dto, token);

            if (!response.TaskExists) return BadRequest(new { Message = "The task of this task item no longer exists" });

            if (!response.TaskItemExists) return BadRequest(new { Message = "This task item no longer exists" });

            if (!response.IsTaskOwner) return BadRequest(new { Message = "This task is not yours" });

            if (response.IsAlreadyCompleted) return BadRequest(new { Message = "Cannot change a completed task item" });

            return Ok(new { Message = "Updated successfully" });
        }

        [HttpPut("SetTaskItemAsCompleted")]
        [Authorize]
        public async Task<ActionResult> SetTaskItemAsCompleted([FromBody] MarkTaskItemAsCompletedDto dto)
        {
            string? username = User.FindFirstValue(ClaimTypes.Name);

            if(username is not null)
            {
                string token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");

                var r = await _writeService.SetTaskItemAsCompletedAsync(username, dto, token);
                if (!r.TaskItemExists) return BadRequest(new { Message = "This task item doesn't exist" });
                if (!r.TaskExists) return BadRequest(new { Message = "This task doesn't exist" });
                if (!r.CanMarkSprintTaskItemAsCompleted) return NotFound(new { Message = "This sprint has either not begun or has finished" });


                return Ok(new
                {
                    Message = "Completed successfully",
                    r.TaskIsCompleted
                });
            }

            return Unauthorized();

        }

        [HttpGet("GetUserPendingTaskItems/{groupName}")]
        [Authorize]
        public async Task<ActionResult<IReadOnlyList<TaskItem>>> GetUserPendingTaskItems([FromRoute] string groupName)
        {

            if (string.IsNullOrWhiteSpace(groupName) || groupName == "null")
                return BadRequest(new { Message = "Group name is not provided" });

            string? username = User.FindFirstValue(ClaimTypes.Name);

            if (!string.IsNullOrEmpty(username))
            {
                return Ok(await _readService.GetUserPendingTaskItemsAsync(username, groupName));
            }

            return Unauthorized();
            
        }

        [HttpDelete("DeleteTaskItems/{taskId}")]
        [Authorize]
        public async Task<ActionResult> DeleteTaskItems(string taskId)
        {
            await _writeService.DeleteTaskItemsAsync(taskId);
            return NoContent();
        }

        [HttpDelete("DeleteSingleTaskItem/{taskItemId}")]
        [Authorize]
        public async Task<ActionResult<bool>> DeleteSingleTaskItem([FromRoute] string taskItemId, [FromQuery] string taskId, [FromQuery] string groupName)
        {
            var dto = new DeleteTaskItemDto(taskItemId, groupName, taskId);

            string token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");


            bool wasDeleted = await _writeService.DeleteTaskItemAsync(dto, token);

            if (!wasDeleted) return BadRequest(new { Message = "Cannot delete because this taskItem belongs to a sprint" });

            return NoContent();
            
        }

        [HttpPut("SetPriorityToTaskItem")]
        [Authorize]
        public async Task<ActionResult> SetPriorityToTaskItem([FromBody] SetPriorityToTaskItemDto dto)
        {

            string? username = User.FindFirstValue(ClaimTypes.Name);

            if (username is null) return Unauthorized();

            string token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");


            var response = await _writeService.SetPriorityToTaskItemAsync(dto, username, token);

            if (!response.CanPrioritize) return BadRequest(new
            {
                Message = "Only the Product Owner can set priorities"
            });

            return Ok(new
            {
                response.TaskItemId,
                response.TaskId,
                response.Priority,
                dto.GroupName
            });
        }

        [HttpPost("AskToGemini")]
        [Authorize]
        public async Task<ActionResult> AskToGemini([FromBody] AskToAssistantDto dto)
        {
            var result = await _writeService.AskToGeminiAsync(dto);

            if (result is null) return NotFound();

            return Ok(result);

        }
    }

    
}