using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Tasks.DTOs;
using TaskManager.Tasks.Interfaces;

namespace TaskManager.Tasks.Controllers 
{
    [ApiController]
    [Route("api/tasks")]
    public class TaskManagerController : ControllerBase
    {
        private readonly ITaskWriteService _writeService;
        private readonly ITaskReadService _readService;
        public TaskManagerController(ITaskWriteService tasksService, ITaskReadService readService)
        {
            _writeService = tasksService;
            _readService = readService;
        }

        [HttpGet("GetUserTasks/{groupName}")]
        [Authorize]
        public async Task<ActionResult<IReadOnlyList<UserTask>>> GetUserTasks([FromRoute] string groupName)
        {

            string? username = User.FindFirstValue(ClaimTypes.Name);
            
            if(username is not null)
            {
                string token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");
                return Ok(await _readService.GetUserTasksAsync(username, groupName, token));
            }
            return Unauthorized();
        }

        [HttpPost("AddUserTask")]
        [Authorize]
        public async Task<ActionResult> AddUserTask([FromBody] CreateTaskDto dto)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            string? username = User.FindFirstValue(ClaimTypes.Name);

            if(username is not null) 
            {
                string token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");
                var (createdUserTask, errorMessage) = await _writeService.AddUserTaskAsync(username, dto, token);

                if (!string.IsNullOrEmpty(errorMessage))
                    return BadRequest(new { Message = errorMessage });

                if (createdUserTask is not null)
                    return Ok(createdUserTask);
                else
                    return StatusCode(500);

            }

            return Unauthorized();

        }

        [HttpDelete("DeleteUserTask/{taskId}")]
        [Authorize]
        public async Task<ActionResult> DeleteUserTask([FromRoute] string taskId, [FromQuery] string? groupName)
        {
            bool TaskIdIsNull = string.IsNullOrEmpty(taskId) || taskId is "null";

            

            if (TaskIdIsNull) return BadRequest("TaskId cannot be null");

            string? username = User.FindFirstValue(ClaimTypes.Name);

            if(username is not null)
            {
                string token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");

                var response = await _writeService
                    .DeleteTaskAsync(groupName, taskId, token, username);

                if (!response.TaskExists) return NotFound(new { Message = "This task doesn't exist" });

                if (!response.TaskCanBeDeleted) return BadRequest(new { Message = "You cannot delete this task" });

                if (response.DeletedTask is not null) return NoContent();

                return BadRequest(new { Message = "Task couldn't be deleted" });
            }

            return Unauthorized();

        }

        [HttpGet("GetTaskOwnerName/{taskItemId}")]
        [Authorize]
        public async Task<ActionResult<string>> GetTaskOwnerName(string taskItemId)
        {
            string? result = await _readService.GetTaskOwnerNameAsync(taskItemId);

            if (result is not null)
                return Ok(new { result });
            else
                return NotFound();
        }

        [HttpPut("MarkSprintTasksAsFinished/{sprintId}")]
        [Authorize]
        public async Task<ActionResult> MarkSprintTasksAsFinished([FromRoute] string sprintId)
        {
            await _writeService.MarkSprintTasksAsFinishedAsync(sprintId);

            return NoContent();

        }

        [HttpGet("TaskContainsSprint/{taskId}")]
        [Authorize]
        public async Task<ActionResult> TaskContainsSprint(string taskId)
        {

            bool result = await _readService.TaskContainsSprintAsync(taskId);
            return Ok(new { result });

        }

        [HttpDelete("DeleteFinishedStatusToSprintTasks/{sprintId}")]
        [Authorize]
        public async Task<ActionResult> RevertFinishedStatusToSprintTasks(string sprintId)
        {
            await _writeService.RevertSprintTasksSetAsFinishedAsync(sprintId);

            return NoContent();
        }

        [HttpPut("SetTaskPriority")]
        [Authorize]
        public async Task<ActionResult> SetTaskPriority([FromBody] PrioritizeTaskDto dto)
        {

            string? username = User.FindFirstValue(ClaimTypes.Name);
            if (username == null) return Unauthorized();

            string? token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");

            bool prioritized = await _writeService.SetTaskPriorityAsync(dto, username, token);

            if (prioritized) return Ok(new { dto.TaskId, dto.Priority });

            return BadRequest(new { Message = "You cannot prioritize this Task" });
        }

        [HttpGet("TaskExists/{taskId}")]
        [Authorize]
        public async Task<ActionResult> TaskExists([FromRoute] string taskId)
        {
            var result = await _readService.TaskExistsAsync(taskId);

            return Ok(new { result });
        }

        [HttpGet("IsTaskOwner/{taskId}")]
        [Authorize]
        public async Task<ActionResult> IsTaskOwner([FromRoute] string taskId)
        {
            string? username = User.FindFirstValue(ClaimTypes.Name);

            if (username is null) return Unauthorized();

            var result = await _readService.IsTaskOwnerAsync(taskId, username);

            return Ok(new { result });
        }

        [HttpGet("GetSprintTasks/{groupName}")]
        [Authorize]
        public async Task<ActionResult> GetCompletedTask([FromRoute] string groupName)
        {
            string? token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");

            if (token == null) return Unauthorized();

            var tasks = await _readService.GetCompletedTasksAsync(token, groupName);

            return Ok(tasks);
        }
    }
}