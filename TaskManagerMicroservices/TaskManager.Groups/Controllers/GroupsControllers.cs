using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.Common.Documents;
using TaskManager.Groups.Interfaces;

namespace TaskManager.Groups.Controllers
{
    [ApiController]
    [Route("api/groups")]
    public class GroupsControllers : ControllerBase
    {
        private readonly IGroupWriteService _writeService;
        private readonly IGroupReadService _readService;
        public GroupsControllers(IGroupWriteService service, IGroupReadService readService)
        {
            _writeService = service;
            _readService = readService;
        }

        [HttpGet("GroupExists")]
        [Authorize]
        public async Task<ActionResult> GroupExists([FromHeader] string groupName)
        {
            bool result = await _readService.GroupExistsAsync(groupName);

            return Ok(new { result });
        }

        [HttpGet("UserGroupName/{groupName}")]
        [Authorize]
        public async Task<ActionResult<bool>> IsGroupOwner([FromRoute] string groupName)
        {
            string? username = User.FindFirstValue(ClaimTypes.Name);

            if (username is null) return Unauthorized();

            bool result = await _readService.IsGroupOwnerAsync(username, groupName);

            return Ok(new { result });
        }

        [HttpPost("CreateGroup/{groupName}")]
        [Authorize]
        public async Task<ActionResult> OnUserCreatesGroup([FromRoute] string groupName, [FromQuery] bool isScrum)
        {
            if (groupName.Length > 10) return
                    BadRequest(new { Message = "groupName cannot exceed 10 characters" });


            string? username = User.FindFirstValue(ClaimTypes.Name);


            if (username is not null)
            {
                string token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");

                var response = await _writeService.CreateGroupAsync(username, groupName, isScrum, token);

                if (response.IsError) return StatusCode(500);

                if (response is not null)
                    return Ok
                        (
                            new
                            {
                                Message = $"Group {response.GroupName} created successfully",
                                response.GroupName
                            }
                        );
                
                return StatusCode(500);
            }
            return Unauthorized();

        }

        [HttpPost("JoinGroup/{groupName}")]
        [Authorize]
        public async Task<ActionResult> OnJoinGroup(string groupName)
        {
            if (groupName.Length > 15) return BadRequest();

            string? username = User.FindFirstValue(ClaimTypes.Name);

            if (username is not null)
            {
                string token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");

                var result = await _writeService.JoinGroupAsync(username, groupName, token);

                if (result.GroupExists)
                    return Ok
                        (
                            new
                            {
                                Message = $"You joined the group with name \"{groupName}\"",
                                result.GroupName,
                                result.GroupRoleName
                            }
                        );
                else
                    return NotFound(new { Message = "This group doesn't exist" });
            }

            return Unauthorized();
        }

        [HttpGet("IsScrum/{groupName}")]
        [Authorize]
        public async Task<ActionResult> GetIsScrum([FromRoute] string groupName)
        {
            bool result = await _readService.IsScrumAsync(groupName);

            return Ok(new { result });
        }

        [HttpGet("IsAddingTasksAllowed/{groupName}")]
        [Authorize]
        public async Task<ActionResult> IsAddingTasksAllowed([FromRoute] string groupName)
        {
            bool result = await _readService.IsAddingTasksAllowedAsync(groupName);

            return Ok(new { result });
        }

        [HttpPost("SetAllowMembersToAddTask/{groupName}")]
        [Authorize]
        public async Task<ActionResult> SetAllowMembersToAddTask([FromRoute] string groupName, [FromQuery] bool isAllowed)
        {
            string? username = User.FindFirstValue(ClaimTypes.Name);

            if (username is not null)
            {
                bool result = await _writeService.SetAddingTasksAllowed(username, groupName, isAllowed);
                return Ok(new { result });
            }

            return Unauthorized();
        }

        [HttpPut("SetSprintIdToGroup/{sprintId}")]
        [Authorize]
        public async Task<ActionResult> SetSprintIdToGroup([FromRoute] string sprintId, [FromQuery] string groupName)
        {
            await _writeService.SetSprintIdToGroupAsync(groupName, sprintId);

            return NoContent();

        }

        [HttpDelete("DeleteGroup/{groupName}")]
        [Authorize]
        public async Task<ActionResult> OnRemoveGroup(string groupName)
        {

            string? username = User.FindFirstValue(ClaimTypes.Name);

            bool groupNameHasValue = !string.IsNullOrEmpty(groupName) && groupName != "null" && groupName != "undefined";

            if (username is not null && groupNameHasValue)
            {

                var response = await _writeService.RemoveGroupAsync(username, groupName);

                if (!response.GroupExists) return BadRequest(new { Message = "This group doesn't exist" });

                if (response.DeletedGroupOwnerName is not null && response.DeletedGroup is not null)
                    
                    return Ok(new {
                        Message = "Group deleted successfully",
                        response.DeletedGroupOwnerName,
                        response.DeletedGroup
                    });


                return StatusCode(500);

            }
            return Unauthorized();
        }

        [HttpGet("GetInitialGroupInfo/{groupName}")]
        [Authorize]
        public async Task<ActionResult> GetInitialGroupInfo(string groupName)
        {
            string? username = User.FindFirstValue(ClaimTypes.Name);

            if (username is null) return Unauthorized();

            var (IsScrum, IsAllowed, IsGroupOwner) = await _readService.GetInitialGroupInfoAsync(groupName, username);

            return Ok(new
            {
                IsScrum,
                IsAddingTasksAllowed = IsAllowed,
                IsGroupOwner
            });

        }
    }
}
